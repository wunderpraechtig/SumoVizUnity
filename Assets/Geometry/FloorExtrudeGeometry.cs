using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FloorExtrudeGeometry : Geometry
{
    private static GameObject geometryContainer = null;
    private static GeometryLoader gl = null;
    private static MiniatureTeleportationManager tpm = null;

    public static void create(string name, List<Vector2> verticesList, float zOffset)
    {
        if (geometryContainer == null)
            geometryContainer = GameObject.Find("SimulationObjects");

        if(gl == null)
            gl = GameObject.Find("GeometryLoader").GetComponent<GeometryLoader>();

        Material sideMaterial;
        sideMaterial = gl.theme.getFloorMaterial();

        GameObject floor = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        floor.transform.parent = geometryContainer.transform;
        MeshFilter mesh_filter = floor.GetComponent<MeshFilter>();
        Mesh mesh = mesh_filter.mesh;
        floor.GetComponent<Renderer>().material = sideMaterial;
        mesh.Clear();

        floor.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Vector2[] vertices2D = verticesList.ToArray();

        Triangulator tr = new Triangulator(vertices2D);
        int[] indicesUp = tr.Triangulate();

        // Generate indices for the ceiling
        int[] indices = new int[indicesUp.Length*2];

        for (int i = 0; i < indicesUp.Length; ++i) {
            indices[i] = indicesUp[i];
        }
        for (int i = 0; i < indicesUp.Length; ++i)
        {
            int index = indicesUp.Length - (1 + i);
            indices[i+ indicesUp.Length] = indicesUp[index];
        }

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices2D[i].x, zOffset -0.0001f, vertices2D[i].y);
            }  

        // Create the mesh
        mesh = TangentHelper.TangentSolver(mesh);
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateBounds();

        // collider setup
        floor.AddComponent<BoxCollider>();
        floor.layer = 11; // geometry floor

        floor.transform.localPosition = new Vector3(0, 0, 0);

        // teleportation area component setup
        GeometryTeleportationArea tpArea = floor.AddComponent<GeometryTeleportationArea>();
        tpArea.interactionLayerMask = 1 << LayerMask.NameToLayer("GeometryFloor"); // geometry floor
        tpArea.teleportTrigger = BaseTeleportationInteractable.TeleportTrigger.OnSelectEnter;
        if (tpm == null) tpm = FindObjectOfType<MiniatureTeleportationManager>();
        tpArea.setTeleportationManager(tpm);
        floor.transform.localScale = Vector3.one;
    }

    public static void CreateMesh(string name, List<Vector2> verticesList, float zOffset, ref List<Mesh> floorMeshes)
    {
        Mesh mesh = new Mesh();

        Vector2[] vertices2D = verticesList.ToArray();

        Triangulator tr = new Triangulator(vertices2D);
        int[] indicesUp = tr.Triangulate();

        // Generate indices for the ceiling
        int[] indices = new int[indicesUp.Length * 2];

        for (int i = 0; i < indicesUp.Length; ++i)
        {
            indices[i] = indicesUp[i];
        }
        for (int i = 0; i < indicesUp.Length; ++i)
        {
            int index = indicesUp.Length - (1 + i);
            indices[i + indicesUp.Length] = indicesUp[index];
        }

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, zOffset - 0.0001f, vertices2D[i].y);
        }

        // Create the mesh
        mesh = TangentHelper.TangentSolver(mesh);
        mesh.vertices = vertices;
        mesh.triangles = indices;
        //mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        floorMeshes.Add(mesh);
    }

}