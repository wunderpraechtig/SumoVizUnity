using System.Collections.Generic;
using UnityEngine;

public class FloorExtrudeGeometry : Geometry
{
    private static GameObject geometryContainer = null;

    public static void create(string name, List<Vector2> verticesList, float zOffset)
    {
        if (geometryContainer == null)
            geometryContainer = GameObject.Find("SimulationObjects");

        Material sideMaterial;

        GeometryLoader gl = GameObject.Find("GeometryLoader").GetComponent<GeometryLoader>();

        sideMaterial = gl.theme.getWallsMaterialST();

        GameObject floor = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        floor.transform.parent = geometryContainer.transform;
        MeshFilter mesh_filter = floor.GetComponent<MeshFilter>();
        Mesh mesh = mesh_filter.mesh;
        floor.GetComponent<Renderer>().material = sideMaterial;
        mesh.Clear();

        floor.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        floor.GetComponent<Renderer>().material = sideMaterial;

        Vector2[] vertices2D = verticesList.ToArray();

        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

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

        floor.AddComponent<BoxCollider>();
        floor.layer = 11; // geometry floor
        floor.transform.localPosition = new Vector3(0, 0, 0);
    }
}