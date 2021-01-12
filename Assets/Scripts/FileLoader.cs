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

        RecalculateSimulationTransform();
    }

    private void CreateFloorsForHeatmap(ref List<Mesh> meshesFloor)
    {

        //GameObject floors = CreateCombinedMeshObject("FloorsHeatmap", ref meshesFloor, 11, gl.theme.getFloorMaterial());
        //floors.AddComponent<MeshCollider>();
        //floors.AddComponent(System.Type.GetType("HeatmapHandler"));
        //floors.SetActive(false);

        //Mesh tmp = floors.GetComponent<MeshFilter>().sharedMesh;

        //Mesh newmesh = new Mesh();
        //newmesh.vertices = tmp.vertices;
        //newmesh.triangles = tmp.triangles;
        //newmesh.uv = tmp.uv;
        //newmesh.normals = tmp.normals;
        //newmesh.colors = tmp.colors;
        //newmesh.tangents = tmp.tangents;


        //GameObject HeatmapFloors = GameObject.Find("HeatmapVisual");
        //HeatmapFloors.GetComponent<MeshFilter>().sharedMesh = newmesh;
        //HeatmapFloors.GetComponent<HeatmapHandler>().setUpHeatmap();

        
        GameObject HeatmapFloors = new GameObject("HeatmapFloors"); //Mother Object

        HeatmapFloors.transform.parent = simulationObjects.transform;

        for (int i = 0; i < meshesFloor.Count; i++)
        {

            GameObject singleMesh = new GameObject("HeatmapMesh" + (i + 1), typeof(MeshFilter), typeof(MeshRenderer));
            singleMesh.transform.parent = HeatmapFloors.transform;
            MeshFilter mesh_filter = singleMesh.GetComponent<MeshFilter>();
            mesh_filter.mesh = meshesFloor[i];
            //singleMesh.GetComponent<Renderer>().material = gl.theme.getFloorMaterial();
            singleMesh.GetComponent<Renderer>().material = (Material)Resources.Load("Heatmap/HeatmapVisual", typeof(Material));
            //singleMesh.GetComponent<Renderer>().material = Resources.Load<Material>("HeatmapVisual");
            singleMesh.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            singleMesh.layer = 11;
            singleMesh.transform.localPosition = new Vector3(0, 0, 0);
            singleMesh.transform.localScale = Vector3.one;
            singleMesh.transform.localRotation = Quaternion.Euler(0, 0, 0);
            //meshesFloor[i]

            //poly label nach  https://github.com/mapbox/polylabel/issues/26 oben
            //braucht ein float[][][], float [0] sind äußere konturen (die für mich entscheidenden!?), float[0][i] sind die verschiedenen vertices, beim untervektor 0 ist x, bei 1 ist y (float[0][i][0/1]

            float[][][] forPolyLabel = new float[1][][]; //ein "jagged array"
            forPolyLabel[0] = new float[mesh_filter.mesh.vertices.Length][]; //ein "jagged array"

            for (int j = 0; j < mesh_filter.mesh.vertices.Length; j++)
            {
                forPolyLabel[0][j] = new float[2];
                forPolyLabel[0][j][0] = mesh_filter.mesh.vertices[j].x;
                forPolyLabel[0][j][1] = mesh_filter.mesh.vertices[j].z;
            }

            var result = PolyLabel.GetPolyLabel(forPolyLabel);
            float centerX = result[0];
            float centerZ = result[1];

            float boundLeft = float.MinValue, boundRight = float.MaxValue, boundBottom = float.MinValue, boundTop = float.MaxValue;
            Vector3[] meshVertices = mesh_filter.mesh.vertices;
            //int debug;
            //if (i == 2 || i == 4 || i == 18 || i == 24 || i == 38)
            //{

            //    debug = 1;
            //    var center = mesh_filter.mesh.bounds.center;
            //}

            //find best fitting rectangle inside the mesh
            //Rectangle gives limit to max and min x and z value!

            //start with left axis of the rectangle (from center to the left, along the x axis) - am i really allowed to ignore y? what about stairs? TODO:

            //float tmpMinLeft = float.MinValue;
            //float tmpMinBottom = float.MinValue;
            //float tmpMax = float.MaxValue;

            for (int j=0; j<meshVertices.Length; j++)
            {
                if(meshVertices[j].x < centerX && meshVertices[j].x > boundLeft) //it has to be left from center x, but it also has to be the one closest to x (so within these left values it needs to be the biggest one)
                {
                    boundLeft = meshVertices[j].x;
                }
                if (meshVertices[j].z < centerZ && meshVertices[j].z > boundBottom) //bot
                {
                    boundBottom = meshVertices[j].z;
                }
                if (meshVertices[j].x > centerX && meshVertices[j].x < boundRight) //right
                {
                    boundRight = meshVertices[j].x;
                }
                if (meshVertices[j].z > centerZ && meshVertices[j].z < boundTop) //top
                {
                    boundTop = meshVertices[j].z;
                }
            }
            if(boundLeft == float.MinValue)
            {
                //not good if true!
                boundLeft = centerX;
            }
            if (boundBottom == float.MinValue)
            {
                //not good if true!
                boundBottom = centerZ;
            }
            if (boundRight == float.MaxValue)
            {
                //not good if true!
                boundRight = centerX;
            }
            if (boundTop == float.MaxValue)
            {
                //not good if true!
                boundTop = centerZ;
            }

            GameObject realHeatmapMesh = new GameObject("RealHeatmapMesh" + (i + 1), typeof(MeshFilter), typeof(MeshRenderer));
            realHeatmapMesh.transform.parent = HeatmapFloors.transform;
            MeshFilter mesh_filter2 = realHeatmapMesh.GetComponent<MeshFilter>();


            //mesh_filter2.mesh = meshesFloor[i]; //take old mesh, but change vertices and triangles!

            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(boundLeft, meshesFloor[i].vertices[0].y+1, boundTop); //TODO: can i just take any y? are they all in one plane?
            vertices[1] = new Vector3(boundRight, meshesFloor[i].vertices[0].y+1, boundTop);
            vertices[2] = new Vector3(boundRight, meshesFloor[i].vertices[0].y+1, boundBottom);
            vertices[3] = new Vector3(boundLeft, meshesFloor[i].vertices[0].y+1, boundBottom);

            //TODO: vertices lassen sich nur ersetzen, wenn anzahl vertices vom mesh der anzahl vom neuen vertices entspricht..
           // mesh_filter2.mesh.vertices = new Vector3[4];
            mesh_filter2.mesh.vertices = vertices;

            //mesh_filter2.mesh.vertices = new Vector3[4];
            //mesh_filter2.mesh.vertices[0] = new Vector3(boundTop, meshesFloor[i].vertices[0].y, boundLeft); //TODO: can i just take any y? are they all in one plane?
            //mesh_filter2.mesh.vertices[1] = new Vector3(boundTop, meshesFloor[i].vertices[0].y, boundRight);
            //mesh_filter2.mesh.vertices[2] = new Vector3(boundBottom, meshesFloor[i].vertices[0].y, boundRight);
            //mesh_filter2.mesh.vertices[3] = new Vector3(boundBottom, meshesFloor[i].vertices[0].y, boundLeft);

            int[] triangles = new int[6]; //TODO triangles: currently you can only see the mesh from above. to be able to see it from below we need to add more triangles! counter clockwise so that the normals look to the bottom?
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 3;
            triangles[3] = 1;
            triangles[4] = 2;
            triangles[5] = 3;

            mesh_filter2.mesh.triangles = triangles;
            
            //mesh_filter2.mesh.triangles = new int[6];
            //mesh_filter2.mesh.triangles[0] = 0;
            //mesh_filter2.mesh.triangles[1] = 1;
            //mesh_filter2.mesh.triangles[2] = 3;
            //mesh_filter2.mesh.triangles[3] = 1;
            //mesh_filter2.mesh.triangles[4] = 2;
            //mesh_filter2.mesh.triangles[5] = 3;

            //realHeatmapMesh.GetComponent<Renderer>().material = gl.theme.getFloorMaterial();
            realHeatmapMesh.GetComponent<Renderer>().material = (Material)Resources.Load("Heatmap/HeatmapVisual", typeof(Material));
            //singleMesh.GetComponent<Renderer>().material = Resources.Load<Material>("HeatmapVisual");
            realHeatmapMesh.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            realHeatmapMesh.layer = 11;
            realHeatmapMesh.transform.localPosition = new Vector3(0, 0, 0);
            realHeatmapMesh.transform.localScale = Vector3.one;
            realHeatmapMesh.transform.localRotation = Quaternion.Euler(0, 0, 0);

        }

    }

    private void GetBasicRectangleMesh()
    {

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