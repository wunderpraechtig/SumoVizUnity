using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        HeatmapStairs.transform.Translate(Vector3.up * 0.001f);
        HeatmapStairs.transform.parent = simulationObjects.transform;


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

            boundingBoxVertices = boundingBoxVertices.OrderBy(y => y.y).ToList<Vector3>();
            List<Vector3> lowerEdge = new List<Vector3> { boundingBoxVertices[0], boundingBoxVertices[1] };
            List<Vector3> upperEdge = new List<Vector3> { boundingBoxVertices[2], boundingBoxVertices[3] };

            Vector3 nearLeft = new Vector3();
            Vector3 farLeft = new Vector3();
            Vector3 farRight = new Vector3();

            if (upperEdge[0].x == upperEdge[1].x) //then order by z, because if x is the same value along the top and bot edge it doesnt help us to find out how to align the corners of the quad
            {
                lowerEdge = lowerEdge.OrderBy(v => v.z).ToList<Vector3>();
                upperEdge = upperEdge.OrderBy(v => v.z).ToList<Vector3>();

                if (lowerEdge[0].x > upperEdge[0].x) //if the bottom of the stair starts on the right (bigger x value), then top left of the quad has a smaller z value
                {
                    farLeft = upperEdge[0];
                    farRight = upperEdge[1];
                    nearLeft = lowerEdge[0];
                }
                else if (lowerEdge[0].x < upperEdge[0].x)// if bottom of stairs is left compared to the top of the stairs, then topleft has the bigger z value
                {
                    farLeft = upperEdge[1];
                    farRight = upperEdge[0];
                    nearLeft = lowerEdge[1];
                }
            }
            else
            {
                lowerEdge = lowerEdge.OrderBy(v => v.x).ToList<Vector3>();
                upperEdge = upperEdge.OrderBy(v => v.x).ToList<Vector3>();

                if (lowerEdge[0].z > upperEdge[0].z)
                {
                    farLeft = lowerEdge[0];
                    farRight = lowerEdge[1];
                    nearLeft = upperEdge[0];
                }
                else
                {
                    farLeft = upperEdge[0];
                    farRight = upperEdge[1];
                    nearLeft = lowerEdge[0];
                }
            }

            GameObject heatmapMesh = new GameObject("StairsHeatmap" + (i + 1), typeof(MeshFilter), typeof(MeshRenderer));
            heatmapMesh.transform.parent = HeatmapStairs.transform;
            MeshFilter mesh_filter = heatmapMesh.GetComponent<MeshFilter>();


            int[] trianglesNew;

            var result = RecalculateVerticesAndIndecesForMesh(farLeft, farRight, nearLeft, heatmapHandler.StairQuadSize, out trianglesNew, out HeatmapData heatmapData);

            //heatmapHandler.AddToStairsHeatmapData(heatmapData);



            mesh_filter.mesh.vertices = result;


            mesh_filter.mesh.triangles = trianglesNew;



            Vector2[] uvs = new Vector2[mesh_filter.mesh.vertices.Length];
            for (int j = 0; j < uvs.Length; j++)
            {
                uvs[j] = new Vector2(0, 0); // the uv coordinate 0,0 is a transparent part of the texture
                uvs[j] = new Vector2(1f / 128f + 1f / 64f, 0); // the uv coordinate 1f / 128f + 1f / 64f is the first green pixel of the texture
            }


            mesh_filter.mesh.uv = uvs;

            Mesh meshOfFilter = mesh_filter.mesh;

            heatmapData.setHeatmapMesh(meshOfFilter);
            //heatmapHandler.AddToHeatmapMeshes(ref meshOfFilter);
            heatmapHandler.AddToHeatmapData(ref heatmapData);
            //heatmapHandler.AddToStairHeatmapMeshes(ref meshOfFilter);

            heatmapMesh.GetComponent<Renderer>().material = (Material)Resources.Load("Heatmap/HeatmapVisual", typeof(Material));
            heatmapMesh.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            heatmapMesh.layer = 11;
            heatmapMesh.transform.localPosition = new Vector3(0, 0, 0);
            heatmapMesh.transform.localScale = Vector3.one;
            heatmapMesh.transform.localRotation = Quaternion.Euler(0, 0, 0);



        }

    }

    private void CreateFloorsForHeatmap(ref List<Mesh> meshesFloor)
    {
        GameObject HeatmapFloors = new GameObject("HeatmapFloors"); //Mother Object
        HeatmapFloors.transform.Translate(Vector3.up * 0.001f);

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

            ++meshNo;
            var key = entry.Key;
            var value = entry.Value;

            //////////////////////////////
            ////for debugging purposes: this code instantiates all submeshes of the big floors mesh
            //GameObject heatmapFloors = CreateCombinedMeshObject("HeatmapFloor" + (meshNo), ref value, 11, gl.theme.getFloorMaterial());
            //heatmapFloors.transform.parent = HeatmapFloors.transform;
            //heatmapFloors.AddComponent<MeshCollider>();
            //MeshFilter tmpFilter = (MeshFilter)heatmapFloors.GetComponent("MeshFilter");
            ///////////////////////////
            var combinedMesh = CombineMeshes(ref value);



            Bounds meshBounds = combinedMesh.bounds;
            //Bounds meshFilterBounds = tmpFilter.mesh.bounds;

            //int[] trianglesNew;


            var topleft = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z);
            var topright = meshBounds.max;
            var bottomleft = meshBounds.min;
            var result = RecalculateVerticesAndIndecesForMesh(topleft, topright, bottomleft, heatmapHandler.FloorQuadSize, out int[] trianglesNew, out HeatmapData heatmapData);


            //////////////////////
            ////create mesh that equals the bounding box

            //GameObject meshFromBounds = new GameObject("HeatmapMeshBounds" + meshNo, typeof(MeshFilter), typeof(MeshRenderer));
            //meshFromBounds.transform.parent = HeatmapFloors.transform;
            //MeshFilter mesh_filter3 = meshFromBounds.GetComponent<MeshFilter>();

            //Vector3[] verts = new Vector3[4];
            //verts[0] = new Vector3(meshBounds.min.x, meshBounds.min.y, meshBounds.max.z);
            //verts[1] = new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.min.z);
            //verts[2] = new Vector3(meshBounds.min.x, meshBounds.min.y, meshBounds.min.z);
            //verts[3] = new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.max.z);
            //mesh_filter3.mesh.vertices = verts;

            //int[] tris = new int[6];
            //tris[0] = 0;
            //tris[1] = 1;
            //tris[2] = 2;
            //tris[3] = 0;
            //tris[4] = 3;
            //tris[5] = 1;
            //mesh_filter3.mesh.triangles = tris;

            //Vector2[] uvss = new Vector2[4] { new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1)};
            ////uvss[0] = new Vector2(0,0);
            ////uvss[1] = new Vector2(0, 0);
            ////uvss[2] = new Vector2(0, 0);
            ////uvss[3] = new Vector2(0, 0);
            //mesh_filter3.mesh.uv = uvss;

            //meshFromBounds.GetComponent<Renderer>().material = (Material)Resources.Load("Heatmap/HeatmapVisual", typeof(Material));
            //meshFromBounds.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //meshFromBounds.layer = 11;
            //meshFromBounds.transform.localPosition = new Vector3(0, 0, 0);
            //meshFromBounds.transform.localScale = Vector3.one;
            //meshFromBounds.transform.localRotation = Quaternion.Euler(0, 0, 0);

            ///////////////////
            GameObject realHeatmapMesh = new GameObject("HeatmapMesh" + meshNo, typeof(MeshFilter), typeof(MeshRenderer));
            realHeatmapMesh.transform.parent = HeatmapFloors.transform;
            MeshFilter mesh_filter2 = realHeatmapMesh.GetComponent<MeshFilter>();
            mesh_filter2.mesh.vertices = result;
            mesh_filter2.mesh.triangles = trianglesNew;

            Vector2[] uvs = new Vector2[mesh_filter2.mesh.vertices.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(0, 0); // the uv coordinate 0,0 is a transparent part of the texture
                uvs[i] = new Vector2(1f / 128f + 1f / 64f, 0); // the uv coordinate 1f / 128f + 1f / 64f is the first green pixel of the texture

            }

            mesh_filter2.mesh.uv = uvs;
            Mesh meshOfFilter = mesh_filter2.mesh;
            //meshOfFilter.triangles = trianglesNew;

            //heatmapHandler.AddToHeatmapMeshes(ref meshOfFilter);
            heatmapData.setHeatmapMesh(meshOfFilter);
            //heatmapHandler.AddToFloorHeatmapMeshes(ref meshOfFilter);
            heatmapHandler.AddToHeatmapData(ref heatmapData);

            realHeatmapMesh.GetComponent<Renderer>().material = (Material)Resources.Load("Heatmap/HeatmapVisual", typeof(Material));
            realHeatmapMesh.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            realHeatmapMesh.layer = 11;
            realHeatmapMesh.transform.localPosition = new Vector3(0, 0, 0);
            realHeatmapMesh.transform.localScale = Vector3.one;
            realHeatmapMesh.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

    }

    private Vector3[] RecalculateVerticesAndIndecesForMesh(Vector3 topLeftVector, Vector3 topRightVector, Vector3 bottomLeftVector, float size, out int[] triangles, out HeatmapData heatmapData) //, Vector3[] oldVertices)
    {
        //topLeftVector.y += 0.1f;
        //topRightVector.y += 0.1f;
        //bottomLeftVector.y += 0.1f;

        Vector3 widthVector = topRightVector - topLeftVector;
        Vector3 widthVectorNormalized = (topRightVector - topLeftVector).normalized;
        Vector3 lengthVector = bottomLeftVector - topLeftVector;
        Vector3 lengthVectorNormalized = (bottomLeftVector - topLeftVector).normalized;
        //vector berechnen von oben links nach oben rechts und dann die länge davon nehmen für breite //TODO delete
        float width = widthVector.magnitude; //get the width 
        float length = lengthVector.magnitude; //get the length
        float height = (bottomLeftVector.y - topLeftVector.y); //TODO: falsch! nur y values?!

        //how many quads fit into width/length?
        float amountQuadsWidthFloat = width / size;
        float amountQuadsLengthFloat = length / size;
        int amountQuadsWidth = (int)amountQuadsWidthFloat; //rounding down
        int amountQuadsLength = (int)amountQuadsLengthFloat; //rounding down
        int roundedUpWidth = (int)Mathf.Ceil(amountQuadsWidthFloat);
        int roundedUpLength = (int)Mathf.Ceil(amountQuadsLengthFloat);

        float sizeEdgeQuadsX = width - (amountQuadsWidth * size);
        float sizeEdgeQuadsZ = length - (amountQuadsLength * size);
        float heightEdgeQuadsRatio = sizeEdgeQuadsZ / length; //ratio edge z Quads to total length --> equals ratio of height edge quads to total height!
        float heightEdgeQuads = heightEdgeQuadsRatio * height; //height of the edge quads
        float heightQuads = (height - heightEdgeQuads) / (float)amountQuadsLength; //remaining height for single normal quads

        //how many vertices will there be in the end? per quad we have 4 vertices (3 vertices per triangle, but 2 vertices are common in both triangles and we need 2 triangles per quad)
        int amountVertices = amountQuadsWidth * 4 * amountQuadsLength;


        ////remove!
        //sizeEdgeQuadsX = sizeEdgeQuadsZ = 0;
        ////remove!

        if (sizeEdgeQuadsX != 0)
        {
            amountVertices += amountQuadsLength * 4; //on the width side there is one more quad per quad along the length

        }
        if (sizeEdgeQuadsZ != 0)
        {
            amountVertices += amountQuadsWidth * 4; //on the length side: one more quad per quad along the width, and if there is an edge quad along the length -> one more quad
            if (sizeEdgeQuadsX != 0)
            {
                amountVertices += 4; //add 2 more triangles
            }
        }

        Vector3[] newVertices = new Vector3[amountVertices];
        triangles = new int[amountVertices + (amountVertices / 2)]; //6 entries per quad
        Vector3 startingPoint = topLeftVector; //take top left of the bounding box

        int currentVertex = 0;
        int currentTriangle = 0;
        int currentTriangleSetNo = 0;

        for (int i = 0; i < amountQuadsLength; i++) //rows
        {
            for (int j = 0; j < amountQuadsWidth; j++) //columns within a row
            {

                newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = startingPoint + size * widthVectorNormalized + size * lengthVectorNormalized; //bottomright vertex 
                newVertices[currentVertex++] = startingPoint + size * lengthVectorNormalized; //bottomleft vertex

                //newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = startingPoint + size * widthVectorNormalized; //topright vertex
                //newVertices[currentVertex++] = startingPoint + size * widthVectorNormalized + size * lengthVectorNormalized; ; //bottomright vertex

                triangles[currentTriangle++] = 0 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 1 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 2 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 0 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 3 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 1 + (currentTriangleSetNo * 4);
                currentTriangleSetNo++;

                startingPoint = startingPoint + size * widthVectorNormalized; //move to the right
            }

            if (sizeEdgeQuadsX != 0)
            {
                newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = startingPoint + sizeEdgeQuadsX * widthVectorNormalized + size * lengthVectorNormalized; //bottomright vertex 
                newVertices[currentVertex++] = startingPoint + size * lengthVectorNormalized; //bottomleft vertex

                //newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = startingPoint + sizeEdgeQuadsX * widthVectorNormalized; //topright vertex
                                                                                                       //newVertices[currentVertex++] = startingPoint + sizeEdgeQuadsX * widthVectorNormalized + size * lengthVectorNormalized; ; //bottomright vertex

                triangles[currentTriangle++] = 0 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 1 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 2 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 0 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 3 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 1 + (currentTriangleSetNo * 4);
                currentTriangleSetNo++;
            }

            //startingPoint.x = boundingBox.min.x; //TODOO --> normalize in the end
            //startingPoint.z -= size;
            //startingPoint.y -= heightQuads;

            startingPoint = topLeftVector + size * (i + 1) * lengthVectorNormalized; //TODO: muss ich jetzt height nicht mehr berücksichtigen, weil ich entlang der realen koordinaten gehe??
        }

        //look whether a last row of not fully sized quads is needed
        if (sizeEdgeQuadsZ != 0)
        {
            //int startIndex = amountVertices - (amountQuadsWidth * 4); //or do it with currentVertex!
            //if (sizeEdgeQuadsX != 0)
            //{
            //    startIndex -= 4;
            //}
            for (int i = 0; i < amountQuadsWidth; i++)
            {

                newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = startingPoint + size * widthVectorNormalized + sizeEdgeQuadsZ * lengthVectorNormalized; //bottomright vertex 
                newVertices[currentVertex++] = startingPoint + sizeEdgeQuadsZ * lengthVectorNormalized; //bottomleft vertex

                //newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = startingPoint + size * widthVectorNormalized; //topright vertex
                                                                                             //newVertices[currentVertex++] = startingPoint + size * widthVectorNormalized + sizeEdgeQuadsZ * lengthVectorNormalized; ; //bottomright vertex
                triangles[currentTriangle++] = 0 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 1 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 2 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 0 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 3 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 1 + (currentTriangleSetNo * 4);
                currentTriangleSetNo++;

                //startingPoint.x += size;

                startingPoint = startingPoint + size * widthVectorNormalized;

            }
            if (sizeEdgeQuadsX != 0)
            {
                newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = startingPoint + sizeEdgeQuadsX * widthVectorNormalized + sizeEdgeQuadsZ * lengthVectorNormalized; //bottomright vertex 
                newVertices[currentVertex++] = startingPoint + sizeEdgeQuadsZ * lengthVectorNormalized; //bottomleft vertex

                //newVertices[currentVertex++] = startingPoint; //topleft vertex 
                newVertices[currentVertex++] = startingPoint + sizeEdgeQuadsX * widthVectorNormalized; //topright vertex
                                                                                                       //newVertices[currentVertex++] = startingPoint + sizeEdgeQuadsX * widthVectorNormalized + sizeEdgeQuadsZ * lengthVectorNormalized; ; //bottomright vertex
                triangles[currentTriangle++] = 0 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 1 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 2 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 0 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 3 + (currentTriangleSetNo * 4);
                triangles[currentTriangle++] = 1 + (currentTriangleSetNo * 4);
                currentTriangleSetNo++;

            }
        }
        //currentTriangleSetNo = 0;
        //for (int i = 0; i < amountVertices + (amountVertices / 2); i += 6)
        //{
        //    //0,1,2 and then 0,3,1
        //    triangles[i] = 0 + (currentTriangleSetNo * 4);
        //    triangles[i + 1] = 1 + (currentTriangleSetNo * 4);
        //    triangles[i + 1] = 2 + (currentTriangleSetNo * 4);
        //    triangles[i + 1] = 0 + (currentTriangleSetNo * 4);
        //    triangles[i + 1] = 3 + (currentTriangleSetNo * 4);
        //    triangles[i + 1] = 1 + (currentTriangleSetNo++ * 4);
        //}
        startingPoint = topLeftVector;
        //fill in all the information into the HeatmapHandler
        heatmapData = new HeatmapData(startingPoint, widthVector, lengthVector, topRightVector, bottomLeftVector, width, length, roundedUpWidth, roundedUpLength, size);

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