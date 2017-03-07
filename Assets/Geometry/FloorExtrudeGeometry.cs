using System.Collections.Generic;
using UnityEngine;

public class FloorExtrudeGeometry : Geometry
{
    public static void create(string name, List<Vector2> verticesList, float zOffset)
    {

        Material sideMaterial;

        GeometryLoader gl = GameObject.Find("GeometryLoader").GetComponent<GeometryLoader>();

        sideMaterial = gl.theme.getWallsMaterialST();

        GameObject floor = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
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
                vertices[i] = new Vector3(vertices2D[i].x, zOffset + 0.05f, vertices2D[i].y);
            }  

        // Create the mesh
        mesh = TangentHelper.TangentSolver(mesh);
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateBounds();
    }
}