using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;


public class FileLoader : MonoBehaviour
{
    [SerializeField] private GameObject miniatureNormalizer = null;
    [SerializeField] private GameObject simulationObjects = null;
    [SerializeField] private PedestrianSystem pedestrianSystem = null;
    [SerializeField] private GeometryLoader gl = null;
    [SerializeField] private GameState gameState = null;
    [SerializeField] private HeatmapHandler heatmapHandler = null;

    private int simulationLayers = 0x3C00;

    void Start()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        simulationLayers = LayerMask.GetMask("GeometryFloor", "GeometryWalls", "GeometryObstacles");
        gl.setTheme(new LabThemingMode());

#if UNITY_EDITOR
        var GeometryXML = EditorUtility.OpenFilePanel(
            "Load Geometry", "", "xml");

        var TrajectoryXML = EditorUtility.OpenFilePanel(
            "Load Trajectory", "", "xml");

        StartCoroutine(LoadSimulation(GeometryXML, TrajectoryXML));
#endif
    }

    public IEnumerator LoadSimulation(string pathGeo, string pathTraj)
    {

        yield return ClearCurrentSimulation();

        if (pathGeo != "")
            yield return loadGeometryFile(pathGeo);

        if (pathTraj != "")
            yield return loadPedestrianFile(pathTraj);


    }

    public IEnumerator ClearCurrentSimulation()
    {
        pedestrianSystem.ClearPedestrianEntities();
        List<GameObject> lastSimulation = new List<GameObject>();
        foreach (Transform child in simulationObjects.transform)
        {
            if ((1 << child.gameObject.layer & simulationLayers) != 0)
            {
                lastSimulation.Add(child.gameObject);
            }

        }

        for (int i = 0; i < lastSimulation.Count; ++i)
        {
            Destroy(lastSimulation[i]);
            yield return null;
        }
    }

    public void RecalculateSimulationTransform()
    {
        MeshFilter[] meshFilters = simulationObjects.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0)
            return;

        Bounds firstMeshBounds = meshFilters[0].mesh.bounds;
        Bounds combinedBounds = new Bounds(firstMeshBounds.center, firstMeshBounds.extents);
        foreach (MeshFilter filter in meshFilters)
        {
            if (filter.mesh != null)
            {
                combinedBounds.Encapsulate(filter.mesh.bounds);
            }
        }

        Vector3 centeredPos = -combinedBounds.center;
        centeredPos.y += combinedBounds.extents.y;
        simulationObjects.transform.localPosition = centeredPos;

        float maxExtends = combinedBounds.extents.x;
        if (maxExtends < combinedBounds.extents.y)
            maxExtends = combinedBounds.extents.y;
        if (maxExtends < combinedBounds.extents.z)
            maxExtends = combinedBounds.extents.z;
        miniatureNormalizer.transform.localScale = new Vector3(0.5f / maxExtends, 0.5f / maxExtends, 0.5f / maxExtends);
    }

    IEnumerator loadGeometryFile(string filename)
    {

        if (!System.IO.File.Exists(filename))
        {
            Debug.Log("Error: File " + filename + " not found.");
            yield break;
        }

        List<Mesh> meshesFloor = new List<Mesh>();
        List<Mesh> meshesWallSide = new List<Mesh>();
        List<Mesh> meshesWallTop = new List<Mesh>();
        List<Mesh> meshesStairs = new List<Mesh>();

        XmlDocument xmlDocGeo = new XmlDocument();
        xmlDocGeo.LoadXml(System.IO.File.ReadAllText(filename));

        int wallNr = 1;     //includes "Walls" and "Obsacles"
        int stairNr = 1;    //includes "Stairs" and "Escalators"
        int floorNr = 1;    //floors are generated for each subroom at this state

        XmlNode geometry = xmlDocGeo.SelectSingleNode("//geometry");
        foreach (XmlElement rooms in geometry.SelectNodes("//rooms"))
        {
            foreach (XmlElement room in rooms.SelectNodes("room"))
            {
                foreach (XmlElement subroom in room.SelectNodes("subroom[@class='subroom'" +
                                                                    " or @class = 'Office' " +
                                                                    " or @class = 'Not specified' " +
                                                                    " or @class='Corridor']"))
                {
                    float height = 1.0f;
                    float zOffset = TryParseWithDefault.ToSingle(subroom.GetAttribute("C_z"), 0);       //EXCEPTION: sometimes "C_z" is referred to as "C"

                    if (zOffset == 0)
                    {
                        zOffset = TryParseWithDefault.ToSingle(subroom.GetAttribute("C"), 0);
                    }

                    foreach (XmlElement openWall in subroom.SelectNodes("polygon[@caption='wall']"))
                    {
                        WallExtrudeGeometry.CreateMeshes(openWall.GetAttribute("caption") + (" ") + wallNr, parsePoints(openWall), height, 0.2f, zOffset, ref meshesWallSide, ref meshesWallTop);

                        wallNr++;
                        //yield return null;
                    }
                    foreach (XmlElement obstacle in subroom.SelectNodes("obstacle"))
                    {
                        foreach (XmlElement Wall in obstacle.SelectNodes("polygon"))
                        {
                            ObstacleExtrudeGeometry.CreateMeshes(Wall.GetAttribute("caption") + (" ") + wallNr, parsePoints(Wall), height, zOffset, ref meshesWallSide, ref meshesWallTop);
                            wallNr++;
                            //yield return null;
                        }
                    }


                    List<Vector2> floorVertices = parsePoints_floor(subroom);
                    //FloorExtrudeGeometry.create("floor " + floorNr, floorVertices, zOffset);
                    FloorExtrudeGeometry.CreateMesh("floor " + floorNr, floorVertices, zOffset, ref meshesFloor);
                    //floorVertices.Reverse();
                    //FloorExtrudeGeometry.CreateMesh("floor_r " + floorNr, floorVertices, zOffset, ref meshesFloor);
                    floorNr++;
                    //yield return null;
                }
                foreach (XmlElement subroom in room.SelectNodes("subroom[@class='stair' or @class ='Stair'"
                                                              + "or @class='idle_escalator']"))
                {
                    float A_x = TryParseWithDefault.ToSingle(subroom.GetAttribute("A_x"), 0);
                    float B_y = TryParseWithDefault.ToSingle(subroom.GetAttribute("B_y"), 0);
                    float C_z = TryParseWithDefault.ToSingle(subroom.GetAttribute("C_z"), 0);  //EXCEPTION: sometimes "C_z" is referred to as "C"
                    if (C_z == 0)
                    {
                        C_z = TryParseWithDefault.ToSingle(subroom.GetAttribute("C"), 0);
                    }

                    float pxUp = 0;     //Variables need to be assigned
                    float pyUp = 0;
                    float pxDown = 0;
                    float pyDown = 0;

                    foreach (XmlElement up in subroom.SelectNodes("up"))
                    {
                        pxUp = TryParseWithDefault.ToSingle(up.GetAttribute("px"), 0);
                        pyUp = TryParseWithDefault.ToSingle(up.GetAttribute("py"), 0);
                    }
                    foreach (XmlElement down in subroom.SelectNodes("down"))
                    {
                        pxDown = TryParseWithDefault.ToSingle(down.GetAttribute("px"), 0);
                        pyDown = TryParseWithDefault.ToSingle(down.GetAttribute("py"), 0);
                    }

                    bool lowerPart = true;
                    List<Vector2> coordFirst = new List<Vector2>();
                    List<Vector2> coordSecond = new List<Vector2>();
                    foreach (XmlElement stair in subroom.SelectNodes("polygon[@caption='wall']"))
                    {
                        if (lowerPart == true)
                        {
                            coordFirst = parsePoints(stair);
                            lowerPart = false;
                        }
                        else
                        {
                            coordSecond = parsePoints(stair);
                            lowerPart = true;
                        }
                    }


                    StairExtrudeGeometry.CreateMesh(subroom.GetAttribute("class") + (" ") + stairNr, coordFirst, coordSecond, A_x, B_y, C_z, pxUp, pyUp, pxDown, pyDown, ref meshesStairs);
                    stairNr++;
                    //yield return null;
                }
            }
        }
        CreateFloors(ref meshesFloor);
        CreateFloorsForHeatmap(ref meshesFloor);
        CreateWalls(ref meshesWallSide, ref meshesWallTop);
        CreateStairs(ref meshesStairs);
        CreateHeatmapStairs(ref meshesStairs);

        RecalculateSimulationTransform();
    }

    private void CreateHeatmapStairs(ref List<Mesh> meshesStairs)
    {

        GameObject HeatmapStairs = new GameObject("HeatmapStairs"); //Mother Object
        HeatmapStairs.transform.parent = simulationObjects.transform;


        //CreateCombinedMeshObject("StairsHeatmap", ref meshesStairs, 12, gl.theme.getWallsMaterial());

        for (int i = 0; i < meshesStairs.Count; i++)
        {
            Dictionary<int, Vector3> distinctVertices = new Dictionary<int, Vector3>();

            for (int j = 0; j < meshesStairs[i].vertexCount; j++)
            {
                var hashCode = meshesStairs[i].vertices[j].GetHashCode();

                if (!distinctVertices.ContainsKey(hashCode))
                {
                    distinctVertices.Add(hashCode, meshesStairs[i].vertices[j]);
                }

            }

            List<Vector3> boundingBoxVertices = new List<Vector3>();
            //Bounds
            int currentVertexIndex = 0;
            foreach (var entry in distinctVertices)
            {
                var currentVertex = entry.Value;
                int compareToIndex = 0;
                foreach (var compareTo in distinctVertices)
                {
                    if (compareToIndex <= currentVertexIndex)
                    {
                        compareToIndex++;
                        continue;
                    }

                    if (compareTo.Value.x == currentVertex.x && compareTo.Value.z == currentVertex.z)
                    {
                        if (compareTo.Value.y > currentVertex.y)
                        {
                            boundingBoxVertices.Add(compareTo.Value);
                        }
                        else
                        {
                            boundingBoxVertices.Add(currentVertex);
                        }
                        break;
                    }
                }
                currentVertexIndex++;

            }

            Bounds boundingBox;
          
            //was brauche ich? x min Value, x Max value, und das gleiche für z und y


            //Mesh combinedMesh = CombineMeshes(ref meshes);
            GameObject obj = new GameObject("StairsHeatmap" + (i + 1), typeof(MeshFilter), typeof(MeshRenderer));
            obj.transform.parent = HeatmapStairs.transform;
            MeshFilter mesh_filter = obj.GetComponent<MeshFilter>();
            //mesh_filter.mesh = combinedMesh;
            mesh_filter.mesh = meshesStairs[i];
            //obj.GetComponent<Renderer>().material = mat;
            obj.GetComponent<Renderer>().material = gl.theme.getWallsMaterial();
            obj.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            obj.layer = 12;
            //obj.layer = layer;
            obj.transform.localPosition = new Vector3(0, 0, 0);
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            //return obj;

        }
        //int meshNo = 0;
        ////////////////////////////////////
        /////
        ////for debugging purposes: this code instantiates all submeshes of the big floors mesh

        //for(int i=0; i<meshesStairs.Count; i++)
        //{
        //    var currentMesh = meshesStairs[i];

        //    GameObject heatmapFloors = CreateCombinedMeshObject("HeatmapFloor" + (++meshNo), ref currentMesh, 11, gl.theme.getFloorMaterial());
        //    heatmapFloors.transform.parent = HeatmapStairs.transform;
        //    heatmapFloors.AddComponent<MeshCollider>();
        //    MeshFilter tmpFilter = (MeshFilter)heatmapFloors.GetComponent("MeshFilter");

        //}








        ////////////////////////////
        //Dictionary<float, List<Mesh>> meshesOnSameLevel = new Dictionary<float, List<Mesh>>();

        //for (int i = 0; i < meshesStairs.Count; i++)
        //{
        //    Mesh currentMesh = meshesStairs[i];
        //    float currentYValue = currentMesh.vertices[0].y; //it shouldnt matter whether vertices[0], vertices[1] or vertices[2] is taken, since they all should have the same y value

        //    if (!meshesOnSameLevel.ContainsKey(currentYValue))
        //    {
        //        meshesOnSameLevel[currentYValue] = new List<Mesh>();
        //    }
        //    meshesOnSameLevel[currentYValue].Add(currentMesh);
        //}

        //foreach (var entry in meshesOnSameLevel)
        //{


        //    var key = entry.Key;
        //    var value = entry.Value;

        //    //for debugging purposes: this code instantiates all submeshes of the big floors mesh that are on the same level(?)
        //    GameObject heatmapFloors = CreateCombinedMeshObject("HeatmapFloor" + (++meshNo), ref value, 11, gl.theme.getFloorMaterial());
        //    heatmapFloors.transform.parent = HeatmapStairs.transform;
        //    heatmapFloors.AddComponent<MeshCollider>();
        //    MeshFilter tmpFilter = (MeshFilter)heatmapFloors.GetComponent("MeshFilter");

        //}
    }

    private void CreateFloorsForHeatmap(ref List<Mesh> meshesFloor)
    {
        // List<float> yValues = new List<float>();

        GameObject HeatmapFloors = new GameObject("HeatmapFloors"); //Mother Object

        HeatmapFloors.transform.parent = simulationObjects.transform;

        Dictionary<float, List<Mesh>> meshesOnSameLevel = new Dictionary<float, List<Mesh>>();

        for (int i = 0; i < meshesFloor.Count; i++)
        {
            Mesh currentMesh = meshesFloor[i];
            float currentYValue = currentMesh.vertices[0].y; //it shouldnt matter whether vertices[0], vertices[1] or vertices[2] is taken, since they all should have the same y value

            if (!meshesOnSameLevel.ContainsKey(currentYValue))
            {
                meshesOnSameLevel[currentYValue] = new List<Mesh>();
            }
            meshesOnSameLevel[currentYValue].Add(currentMesh);
        }

        int meshNo = 0;
        foreach (var entry in meshesOnSameLevel)
        {


            var key = entry.Key;
            var value = entry.Value;

            //for debugging purposes: this code instantiates all submeshes of the big floors mesh
            //GameObject heatmapFloors = CreateCombinedMeshObject("HeatmapFloor" + (++meshNo), ref value, 11, gl.theme.getFloorMaterial());
            //heatmapFloors.transform.parent = HeatmapFloors.transform;
            //heatmapFloors.AddComponent<MeshCollider>();
            //MeshFilter tmpFilter = (MeshFilter)heatmapFloors.GetComponent("MeshFilter");

            var combinedMesh = CombineMeshes(ref value);

            GameObject realHeatmapMesh = new GameObject("HeatmapMesh" + (++meshNo), typeof(MeshFilter), typeof(MeshRenderer));
            realHeatmapMesh.transform.parent = HeatmapFloors.transform;
            MeshFilter mesh_filter2 = realHeatmapMesh.GetComponent<MeshFilter>();

            Bounds meshBounds = combinedMesh.bounds;
            //Bounds meshFilterBounds = tmpFilter.mesh.bounds;

            int[] trianglesNew;


            //var result = RecalculateVerticesAndIndecesForMesh(meshBounds, heatmapHandler.QuadSize, out trianglesNew);
            var result = RecalculateVerticesAndIndecesForMeshDifferentHeights(meshBounds, heatmapHandler.QuadSize, out trianglesNew);


            mesh_filter2.mesh.vertices = result;


            mesh_filter2.mesh.triangles = trianglesNew;



            Vector2[] uvs = new Vector2[mesh_filter2.mesh.vertices.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                //uvs[i] = new Vector2(1f / 128f + 1f / 64f, 0); // the uv coordinate 1f / 128f + 1f / 64f is the first green pixel of the texture
                uvs[i] = new Vector2(0, 0); // the uv coordinate 0,0 is a transparent part of the texture
            }


            mesh_filter2.mesh.uv = uvs;
            Mesh meshOfFilter = mesh_filter2.mesh;

            heatmapHandler.AddToHeatmapMeshes(ref meshOfFilter);

            realHeatmapMesh.GetComponent<Renderer>().material = (Material)Resources.Load("Heatmap/HeatmapVisual", typeof(Material));
            realHeatmapMesh.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            realHeatmapMesh.layer = 11;
            realHeatmapMesh.transform.localPosition = new Vector3(0, 0, 0);
            realHeatmapMesh.transform.localScale = Vector3.one;
            realHeatmapMesh.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

    }

    private Vector3[] RecalculateVerticesAndIndecesForMesh(Bounds boundingBox, float size, out int[] triangles) //, Vector3[] oldVertices)
    {


        float width = boundingBox.size.x; //get the width of the bounding box (BB)
        float height = boundingBox.size.z; //get the height of the BB

        //how many quads fit into width/height?
        float amountQuadsWidthFloat = width / size;
        float amountQuadsHeightFloat = height / size;
        int amountQuadsWidth = (int)amountQuadsWidthFloat; //rounding down
        int amountQuadsHeight = (int)amountQuadsHeightFloat; //rounding down
        int roundedUpWidth = (int)Mathf.Ceil(amountQuadsWidthFloat);
        int roundedUpHeight = (int)Mathf.Ceil(amountQuadsHeightFloat);

        float sizeEdgeQuadsX = width - (amountQuadsWidth * size);
        float sizeEdgeQuadsZ = height - (amountQuadsHeight * size);

        //how many vertices will there be in the end? per quad we have 3*2 vertices (3 vertices per triangle, and we need 2 triangles per quad because every single vertex can have a different uv coordinate!)
        int amountVertices = amountQuadsWidth * 6 * amountQuadsHeight;

        if (sizeEdgeQuadsX != 0)
        {
            amountVertices += amountQuadsHeight * 6; //on the width side there is one more quad per quad along the height

        }
        if (sizeEdgeQuadsZ != 0)
        {
            amountVertices += amountQuadsWidth * 6; //on the height side: one more quad per quad along the width, and if there is an edge quad along the height -> one more quad
            if (sizeEdgeQuadsX != 0)
            {
                amountVertices += 6; //add 2 more triangles
            }
        }

        Vector3[] newVertices = new Vector3[amountVertices];
        triangles = new int[amountVertices]; //also 6 entries per quad
        Vector3 startingPoint = new Vector3(boundingBox.min.x, boundingBox.min.y, boundingBox.max.z); //take top left of the bounding box

        int currentVertex = 0;

        for (int i = 0; i < amountQuadsHeight; i++)
        {
            for (int j = 0; j < amountQuadsWidth; j++)
            {
                newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + size, startingPoint.y, startingPoint.z - size); //bottomright vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z - size); //bottomleft vertex

                newVertices[currentVertex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z); //topleft vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + size, startingPoint.y, startingPoint.z); //topright vertex
                newVertices[currentVertex++] = new Vector3(startingPoint.x + size, startingPoint.y, startingPoint.z - size); //bottomright vertex

                startingPoint.x += size;
            }

            if (sizeEdgeQuadsX != 0)
            {
                newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y, startingPoint.z - size); //bottomright vertex
                newVertices[currentVertex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z - size); //bottomleft vertex

                newVertices[currentVertex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z); //topleft vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y, startingPoint.z); //topright vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y, startingPoint.z - size); //bottomright vertex 

            }

            startingPoint.x = boundingBox.min.x;
            startingPoint.z -= size;
        }

        //look whether a last row of not fully sized quads is needed
        if (sizeEdgeQuadsZ != 0)
        {
            int startIndex = amountVertices - (amountQuadsWidth * 6); //or do it with currentVertex!
            if (sizeEdgeQuadsX != 0)
            {
                startIndex -= 6;
            }
            for (int i = 0; i < amountQuadsWidth; i++)
            {

                newVertices[startIndex++] = startingPoint; //topleft vertex
                newVertices[startIndex++] = new Vector3(startingPoint.x + size, startingPoint.y, startingPoint.z - sizeEdgeQuadsZ); //bottomright vertex
                newVertices[startIndex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z - sizeEdgeQuadsZ); //bottomleft vertex 

                newVertices[startIndex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z); //topleft vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + size, startingPoint.y, startingPoint.z); //topright vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + size, startingPoint.y, startingPoint.z - sizeEdgeQuadsZ); //bottomright vertex

                startingPoint.x += size;

            }
            if (sizeEdgeQuadsX != 0)
            {
                newVertices[startIndex++] = startingPoint; //topleft vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y, startingPoint.z - sizeEdgeQuadsZ); //bottomright vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z - sizeEdgeQuadsZ); //bottomleft vertex 

                newVertices[startIndex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z); //topleft vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y, startingPoint.z); //topright vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y, startingPoint.z - sizeEdgeQuadsZ); //bottomright vertex 
            }
        }

        for (int i = 0; i < amountVertices; i++)
        {
            triangles[i] = i;
        }
        startingPoint = new Vector3(boundingBox.min.x, boundingBox.min.y, boundingBox.max.z);
        //fill in all the information into the HeatmapHandler
        HeatmapData heatmapData = new HeatmapData(startingPoint, width, height, roundedUpWidth, roundedUpHeight);
        heatmapHandler.AddToHeatmapData(heatmapData);

        return newVertices;

    }

    private Vector3[] RecalculateVerticesAndIndecesForMeshDifferentHeights(Bounds boundingBox, float size, out int[] triangles) //, Vector3[] oldVertices)
    {


        float width = boundingBox.size.x; //get the width of the bounding box (BB)
        float length = boundingBox.size.z; //get the length of the BB
        float height = boundingBox.size.y;

        //how many quads fit into width/length?
        float amountQuadsWidthFloat = width / size;
        float amountQuadsLengthFloat = length / size;
        int amountQuadsWidth = (int)amountQuadsWidthFloat; //rounding down
        int amountQuadsLength = (int)amountQuadsLengthFloat; //rounding down
        int roundedUpWidth = (int)Mathf.Ceil(amountQuadsWidthFloat);
        int roundedUpLength = (int)Mathf.Ceil(amountQuadsLengthFloat);

        float sizeEdgeQuadsX = width - (amountQuadsWidth * size);
        float sizeEdgeQuadsZ = length - (amountQuadsLength * size);
        float heightEdgeQuads = sizeEdgeQuadsZ / length * height; //height of the edge quads
        float heightQuads = height - heightEdgeQuads; //remaining height for normal quads

        //how many vertices will there be in the end? per quad we have 3*2 vertices (3 vertices per triangle, and we need 2 triangles per quad because every single vertex can have a different uv coordinate!)
        int amountVertices = amountQuadsWidth * 6 * amountQuadsLength;

        if (sizeEdgeQuadsX != 0)
        {
            amountVertices += amountQuadsLength * 6; //on the width side there is one more quad per quad along the length

        }
        if (sizeEdgeQuadsZ != 0)
        {
            amountVertices += amountQuadsWidth * 6; //on the length side: one more quad per quad along the width, and if there is an edge quad along the length -> one more quad
            if (sizeEdgeQuadsX != 0)
            {
                amountVertices += 6; //add 2 more triangles
            }
        }

        Vector3[] newVertices = new Vector3[amountVertices];
        triangles = new int[amountVertices]; //also 6 entries per quad
        Vector3 startingPoint = new Vector3(boundingBox.min.x, boundingBox.max.y, boundingBox.max.z); //take top left of the bounding box

        int currentVertex = 0;

        for (int i = 0; i < amountQuadsLength; i++) //rows
        {
            for (int j = 0; j < amountQuadsWidth; j++) //columns within a row
            {

                newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + size, startingPoint.y - heightQuads, startingPoint.z - size); //bottomright vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x, startingPoint.y - heightQuads, startingPoint.z - size); //bottomleft vertex

                newVertices[currentVertex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z); //topleft vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + size, startingPoint.y, startingPoint.z); //topright vertex
                newVertices[currentVertex++] = new Vector3(startingPoint.x + size, startingPoint.y - heightQuads, startingPoint.z - size); //bottomright vertex

                startingPoint.x += size;
            }

            if (sizeEdgeQuadsX != 0)
            {
                newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y - heightQuads, startingPoint.z - size); //bottomright vertex
                newVertices[currentVertex++] = new Vector3(startingPoint.x, startingPoint.y - heightQuads, startingPoint.z - size); //bottomleft vertex

                newVertices[currentVertex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z); //topleft vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y, startingPoint.z); //topright vertex 
                newVertices[currentVertex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y - heightQuads, startingPoint.z - size); //bottomright vertex 

            }

            startingPoint.x = boundingBox.min.x;
            startingPoint.z -= size;
            startingPoint.y -= heightQuads;
        }

        //look whether a last row of not fully sized quads is needed
        if (sizeEdgeQuadsZ != 0)
        {
            int startIndex = amountVertices - (amountQuadsWidth * 6); //or do it with currentVertex!
            if (sizeEdgeQuadsX != 0)
            {
                startIndex -= 6;
            }
            for (int i = 0; i < amountQuadsWidth; i++)
            {

                newVertices[startIndex++] = startingPoint; //topleft vertex
                newVertices[startIndex++] = new Vector3(startingPoint.x + size, startingPoint.y - heightEdgeQuads, startingPoint.z - sizeEdgeQuadsZ); //bottomright vertex
                newVertices[startIndex++] = new Vector3(startingPoint.x, startingPoint.y - heightEdgeQuads, startingPoint.z - sizeEdgeQuadsZ); //bottomleft vertex 

                newVertices[startIndex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z); //topleft vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + size, startingPoint.y, startingPoint.z); //topright vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + size, startingPoint.y - heightEdgeQuads, startingPoint.z - sizeEdgeQuadsZ); //bottomright vertex

                startingPoint.x += size;

            }
            if (sizeEdgeQuadsX != 0)
            {
                newVertices[startIndex++] = startingPoint; //topleft vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y - heightEdgeQuads, startingPoint.z - sizeEdgeQuadsZ); //bottomright vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x, startingPoint.y - heightEdgeQuads, startingPoint.z - sizeEdgeQuadsZ); //bottomleft vertex 

                newVertices[startIndex++] = new Vector3(startingPoint.x, startingPoint.y, startingPoint.z); //topleft vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y, startingPoint.z); //topright vertex 
                newVertices[startIndex++] = new Vector3(startingPoint.x + sizeEdgeQuadsX, startingPoint.y - heightEdgeQuads, startingPoint.z - sizeEdgeQuadsZ); //bottomright vertex 
            }
        }

        for (int i = 0; i < amountVertices; i++)
        {
            triangles[i] = i;
        }
        startingPoint = new Vector3(boundingBox.min.x, boundingBox.min.y, boundingBox.max.z);
        //fill in all the information into the HeatmapHandler
        HeatmapData heatmapData = new HeatmapData(startingPoint, width, length, roundedUpWidth, roundedUpLength);
        heatmapHandler.AddToHeatmapData(heatmapData);

        return newVertices;

    }

    private GameObject CreateCombinedMeshObject(string name, ref List<Mesh> meshes, int layer, Material mat)
    {
        Mesh combinedMesh = CombineMeshes(ref meshes);
        GameObject obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        obj.transform.parent = simulationObjects.transform;
        MeshFilter mesh_filter = obj.GetComponent<MeshFilter>();
        mesh_filter.mesh = combinedMesh;
        obj.GetComponent<Renderer>().material = mat;
        obj.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        obj.layer = layer;
        obj.transform.localPosition = new Vector3(0, 0, 0);
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        return obj;
    }

    private void CreateFloors(ref List<Mesh> meshesFloor)
    {
        GameObject floors = CreateCombinedMeshObject("Floors", ref meshesFloor, 11, gl.theme.getFloorMaterial());
        floors.AddComponent<MeshCollider>();
    }

    private void CreateWalls(ref List<Mesh> sides, ref List<Mesh> tops)
    {
        CreateCombinedMeshObject("WallSides", ref sides, 12, gl.theme.getWallsMaterialST());
        CreateCombinedMeshObject("WallTops", ref tops, 12, gl.theme.getWallsMaterial());
    }

    private void CreateStairs(ref List<Mesh> stairs)
    {
        CreateCombinedMeshObject("Stairs", ref stairs, 12, gl.theme.getWallsMaterial());
    }

    private Mesh CombineMeshes(ref List<Mesh> meshes)
    {
        var combine = new CombineInstance[meshes.Count];
        for (int i = 0; i < meshes.Count; i++)
        {
            combine[i].mesh = meshes[i];
            combine[i].transform = Matrix4x4.identity;
        }

        var mesh = new Mesh();
        mesh.CombineMeshes(combine, true);
        return mesh;
    }

    static List<Vector2> parsePoints(XmlElement polyPoints)
    {
        List<Vector2> list = new List<Vector2>();

        foreach (XmlElement vertex in polyPoints.SelectNodes("vertex"))
        {
            float x;
            float y;
            if (float.TryParse(vertex.GetAttribute("px"), out x) && float.TryParse(vertex.GetAttribute("py"), out y))
            {
                list.Add(new Vector2(x, y));
            }
        }

        foreach (XmlElement openWall in polyPoints.SelectNodes("polygon[@caption='wall']"))  //used to list vertices of all walls in a subroom to create the floor
        {
            foreach (XmlElement vertex in openWall.SelectNodes("vertex"))
            {
                float x;
                float y;
                if (float.TryParse(vertex.GetAttribute("px"), out x) && float.TryParse(vertex.GetAttribute("py"), out y))
                {
                    list.Add(new Vector2(x, y));
                }
            }
        }



        return list;
    }

    static List<Vector2> parsePoints_floor(XmlElement polyPoints)
    {
        List<Vector2> list = new List<Vector2>();

        foreach (XmlElement vertex in polyPoints.SelectNodes("vertex"))
        {
            float x;
            float y;
            if (float.TryParse(vertex.GetAttribute("px"), out x) && float.TryParse(vertex.GetAttribute("py"), out y))
            {
                list.Add(new Vector2(x, y));
            }
        }

        foreach (XmlElement openWall in polyPoints.SelectNodes("polygon[@caption='wall']"))  //used to list vertices of all walls in a subroom to create the floor
        {
            foreach (XmlElement vertex in openWall.SelectNodes("vertex"))
            {
                float x;
                float y;
                if (float.TryParse(vertex.GetAttribute("px"), out x) && float.TryParse(vertex.GetAttribute("py"), out y))
                {
                    list.Add(new Vector2(x, y));
                }
            }
        }

        list = SortNodes(ref list);

        return list;
    }



    IEnumerator loadPedestrianFile(string filename)
    {
        if (!System.IO.File.Exists(filename))
        {
            Debug.Log("Error: File " + filename + " not found.");
            yield break;
        }

        List<PedestrianEntity> result = new List<PedestrianEntity>();
        float totalTime = 0;
        var thread = new Thread(() =>
        {
            try
            {
                XmlDocument xmlDocTraj = new XmlDocument();
                xmlDocTraj.LoadXml(System.IO.File.ReadAllText(filename));

                PedestrianAssembler assembler = new PedestrianAssembler();

                XmlNode trajectories = xmlDocTraj.SelectSingleNode("//trajectories");

                int fps = 8, FrameID;

                foreach (XmlElement header in trajectories.SelectNodes("header"))
                {

                    XDocument doc = XDocument.Load(filename);
                    var fpsInput = doc.Descendants("frameRate");

                    foreach (var fpsVal in fpsInput)
                    {
                        if (float.Parse(fpsVal.Value) != 0)
                            fps = (int)float.Parse(fpsVal.Value);
                    }
                }

                foreach (XmlElement frame in trajectories.SelectNodes("frame"))
                {
                    int.TryParse(frame.GetAttribute("ID"), out FrameID);

                    if (FrameID % fps == 0)
                    {
                        foreach (XmlElement agent in frame)
                        {
                            float time;
                            int id;
                            float x;
                            float y;
                            float z;
                            float.TryParse(frame.GetAttribute("ID"), out time);
                            int.TryParse(agent.GetAttribute("ID"), out id);
                            float.TryParse(agent.GetAttribute("x"), out x);
                            float.TryParse(agent.GetAttribute("y"), out y);
                            float.TryParse(agent.GetAttribute("z"), out z);
                            assembler.addPedestrianPosition(new PedestrianPosition(id, time / fps, x, y, z));

                        }
                    }
                }
                result = assembler.createPedestrians();
                totalTime = assembler.internalTotalTime;
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
                throw e;
            }
        });
        thread.Name = "PedestrianLoader";
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }
        thread.Join();

        gameState.TotalTime = totalTime;
        foreach (var entity in result)
        {
            pedestrianSystem.AddPedestrianEntity(entity);
        }
    }



    static List<Vector2> SortNodes(ref List<Vector2> PointSet)//, List<Node> ch
    {
        // remove same nodes.
        for (int i = 0; i < PointSet.Count; i++)  //loop num 
        {
            for (int j = PointSet.Count - 1; j > i; j--)  //comparison num 
            {

                if (PointSet[i].x == PointSet[j].x && PointSet[i].y == PointSet[j].y)
                {
                    PointSet.RemoveAt(j);
                }
            }
        }
        ConvexAogrithm ca = new ConvexAogrithm(PointSet);
        Vector2 p;
        ca.GetNodesByAngle(out p);
        Stack<Vector2> p_nodes = ca.SortedNodes;
        PointSet.Clear();
        Vector2[] vlist = new Vector2[1];
        vlist = p_nodes.ToArray();
        for (int m = 0; m < vlist.Length; m++)
        {
            PointSet.Add(vlist[m]);
        }
        return PointSet;
    }
}