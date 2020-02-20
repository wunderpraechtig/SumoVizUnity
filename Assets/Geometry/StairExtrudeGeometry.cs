using System.Collections.Generic;
using UnityEngine;
using System;

public class StairExtrudeGeometry : Geometry
{
    private static GameObject geometryContainer = null;

    public static void create(string name, List<Vector2> coordFirst, List<Vector2> coordSecond, float A_x, float B_y, float C_z, float pxUp, float pyUp, float pxDown, float pyDown)
    {
        if (geometryContainer == null)
            geometryContainer = GameObject.Find("SimulationObjects");
        Material topMaterial;

        GeometryLoader gl = GameObject.Find("GeometryLoader").GetComponent<GeometryLoader>();

        topMaterial = gl.theme.getWallsMaterial();

        GameObject stair = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        MeshFilter mesh_filter = stair.GetComponent<MeshFilter>();
        Mesh mesh = mesh_filter.mesh;
        mesh.Clear();
        stair.transform.parent = geometryContainer.transform;

        stair.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        stair.GetComponent<Renderer>().material = topMaterial;


        #region Vertices

        Vector2 origin = new Vector2((pxDown + pxUp) / 2, (pyDown + pyUp) / 2);

        Vector2[] pointsArray = new Vector2[]
        {
            coordFirst[0],
            coordFirst[1],
            coordSecond[0],
            coordSecond[1]
        };

        float[] angles = angleToOrigin(pointsArray, origin).ToArray();

        Array.Sort(angles, pointsArray);

        Vector3[] p = generateEdges(pointsArray, A_x, B_y, C_z).ToArray();

        Vector3[] vertices = new Vector3[]
        {
	        // Bottom
	        p[0], p[1], p[2], p[3],
 
        	// Left
	        p[7], p[4], p[0], p[3],
 
	        // Front
        	p[4], p[5], p[1], p[0],
 
	        // Back
	        p[6], p[7], p[3], p[2],
 
	        // Right
	        p[5], p[6], p[2], p[1],
 
        	// Top
	        p[7], p[6], p[5], p[4]
        };
        #endregion

        #region Normals
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 front = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        Vector3[] normals = new Vector3[]
        {
	// Bottom
	down, down, down, down,
 
	// Left
	left, left, left, left,
 
	// Front
	front, front, front, front,
 
	// Back
	back, back, back, back,
 
	// Right
	right, right, right, right,
 
	// Top
	up, up, up, up
        };
        #endregion

        #region UVs
        float lenghtRelX = 1; //(float) Math.Sqrt(Math.Pow(p[1].x - p[0].x, 2) + Math.Pow(p[1].y - p[0].y, 2));   //calculate actual length of stairs for correct texture mapping
        float lenghtRelY = 1; //(float) Math.Sqrt(Math.Pow(p[2].z - p[1].z, 2) + Math.Pow(p[2].y - p[1].y, 2));    

                              //Debug.Log(lenghtRelX + "  " + lenghtRelY);

        Vector2 _00 = new Vector2(0f, 0f);
        Vector2 _10 = new Vector2(lenghtRelX, 0f);
        Vector2 _01 = new Vector2(0f, lenghtRelY);
        Vector2 _11 = new Vector2(lenghtRelX, lenghtRelY);

        Vector2[] uvs = new Vector2[]
        {
	// Bottom
	_11, _01, _00, _10,
 
	// Left
	_11, _01, _00, _10,
 
	// Front
	_11, _01, _00, _10,
 
	// Back
	_11, _01, _00, _10,
 
	// Right
	_11, _01, _00, _10,
 
	// Top
	_11, _01, _00, _10,
        };
        #endregion

        #region Triangles
        int[] triangles = new int[]
        {
	// Bottom
	3, 1, 0,
    3, 2, 1,			
 
	// Left
	3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
    3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
	// Front
	3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
    3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
	// Back
	3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
    3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
	// Right
	3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
    3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
	// Top
	3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
    3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,

        };
        #endregion

        mesh = TangentHelper.TangentSolver(mesh);
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        MeshCollider collider = stair.AddComponent<MeshCollider>();
        collider.convex = true;
        stair.layer = 11; // geometry floor
        stair.transform.localPosition = new Vector3(0, 0, 0);
    }

    static List<float> angleToOrigin(Vector2[] pointsArray, Vector2 origin)
    {
        List<float> list = new List<float>();
        for (int i = 0; i < pointsArray.Length; i++)
        {
            double angleTemp = System.Math.Atan((pointsArray[i].x - origin.x) / (pointsArray[i].y - origin.y)) * 180 / System.Math.PI;
            if (pointsArray[i].x > origin.x && pointsArray[i].y < origin.y)
                angleTemp = System.Math.Atan((pointsArray[i].x - origin.x) / (origin.y - pointsArray[i].y)) * 180 / System.Math.PI + 90;
            else if (pointsArray[i].x < origin.x && pointsArray[i].y < origin.y)
                angleTemp = System.Math.Atan((origin.x - pointsArray[i].x) / (origin.y - pointsArray[i].y)) * 180 / System.Math.PI + 180;
            else if (pointsArray[i].x < origin.x && pointsArray[i].y > origin.y)
                angleTemp = System.Math.Atan((origin.x - pointsArray[i].x) / (pointsArray[i].y - origin.y)) * 180 / System.Math.PI + 270;

            float angle = (float)angleTemp;
            list.Add(angle); 
        }
        return list;
    }

    static List<Vector3> generateEdges(Vector2[] pointsArray, float A_x, float B_y, float C_z)
    {
        List<Vector3> list = new List<Vector3>();
        for (int i = 0; i < pointsArray.Length; i++)
        {
            float x = pointsArray[i].x;
            float y = A_x * pointsArray[i].x + B_y * pointsArray[i].y + C_z - 0.3f;
            float z = pointsArray[i].y;
            list.Add(new Vector3(x,y,z));
        }
        for (int i = 0; i < pointsArray.Length; i++)
        {
            float x = pointsArray[i].x;
            float y = A_x * pointsArray[i].x + B_y * pointsArray[i].y + C_z;
            float z = pointsArray[i].y;
            list.Add(new Vector3(x, y, z));
        }
        return list;
    }
}