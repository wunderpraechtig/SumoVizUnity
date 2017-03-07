using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExtrudeGeometry : Geometry  {

	public static void create (string name, List<Vector2> verticesList, float height, float zOffset, Material topMaterial, Material sideMaterial) {

		GameObject obstacle = new GameObject (name, typeof(MeshFilter), typeof(MeshRenderer));
		MeshFilter mesh_filter = obstacle.GetComponent<MeshFilter> ();

        //Shadows currently disabled for TopMaterial   -> switch on by    ShadowCastingMode.On
        obstacle.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        obstacle.GetComponent<Renderer>().material = topMaterial;

		Vector2[] vertices2D = verticesList.ToArray();

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D);
		int[] indicesArray = tr.Triangulate();
		List<int> indices = new List<int>();
		for (int i = 0;i<indicesArray.Length;i++) {
			indices.Add (indicesArray[i]);
		}

		// Create the Vector3 vertices
		List<Vector3> vertices = new List<Vector3>();

		for (int i=0; i<vertices2D.Length; i++) {
			vertices.Add (new Vector3(vertices2D[i].x, zOffset + height, vertices2D[i].y));
		}

		// Create the mesh
		Mesh mesh = new Mesh();
        GameObject walls = new GameObject (name + "_side", typeof(MeshFilter), typeof(MeshRenderer));
		MeshFilter mesh_filter_walls = walls.GetComponent<MeshFilter> ();
		walls.GetComponent<Renderer>().material = sideMaterial;

		List<Vector2> uvs_walls = new List<Vector2>();
		List<Vector3> vertices_walls = new List<Vector3>();
		List<int> indices_walls = new List<int>();

		foreach (Vector3 v in vertices) {
			vertices_walls.Add(new Vector3(v.x,zOffset,v.z));
			vertices_walls.Add(new Vector3(v.x,zOffset,v.z));
			vertices_walls.Add(new Vector3(v.x, height + zOffset, v.z));
            vertices_walls.Add(new Vector3(v.x, height + zOffset, v.z));
        }

        for (int i=1; i<=vertices_walls.Count; i=i+4) {
			indices_walls.Add(i);
			indices_walls.Add((i+3)%vertices_walls.Count);
			indices_walls.Add(i+2);

			indices_walls.Add(i+2);
			indices_walls.Add((i+3)%vertices_walls.Count);
			indices_walls.Add((i+5)%vertices_walls.Count);
		}

		//double sided walls
		int indices_walls_count = indices_walls.Count;
		for (int i = indices_walls_count-1;i>=0;i--) {
			indices_walls.Add (indices_walls[i]);
		}

		for (int i =0;i<vertices_walls.Count;i++) {
			float uv_height = height;
			int a = i-3;
			if (a<0) a = a+vertices_walls.Count;
			float uv_width = Vector3.Distance(vertices_walls[i],vertices_walls[a]);
			if ((i-1)%4==0) uvs_walls.Add (new Vector2 (0, 0));
			else if ((i-3)%4==0) uvs_walls.Add (new Vector2 (0, uv_height));
			else if (i%4==0) uvs_walls.Add (new Vector2 (uv_width, 0)); 
			else  uvs_walls.Add (new Vector2 (uv_width, uv_height));
		}

		Mesh mesh_walls = new Mesh();
		mesh_walls.vertices = vertices_walls.ToArray();
		mesh_walls.uv = uvs_walls.ToArray();
		mesh_walls.triangles = indices_walls.ToArray();
		mesh_walls.RecalculateNormals();
		mesh_walls.RecalculateBounds();
		mesh_walls = TangentHelper.TangentSolver (mesh_walls);
		mesh_filter_walls.mesh = mesh_walls;
	

		mesh.vertices = vertices.ToArray();
		mesh.uv = verticesList.ToArray();
		mesh.triangles = indices.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		//flip if needed
		if (mesh.normals [0].y == -1) {
			indices.Reverse ();
			mesh.triangles = indices.ToArray ();
			mesh.RecalculateNormals();
		}
		mesh = TangentHelper.TangentSolver (mesh);

		mesh_filter.mesh = mesh;
	}
}

