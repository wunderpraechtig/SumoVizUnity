using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlainMeshGenerator : MonoBehaviour
{

    public MeshFilter meshFilter;
    public Vector3 startPoint, firstPoint, secondPoint;
    public float quadSize;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = generatePlainRectangularMesh(startPoint, firstPoint, secondPoint, quadSize);
    }

    /// <summary>
    /// Works only for rectangular plains
    /// </summary>
    /// <param name="startPoint"> Should be left upper corner</param>
    /// <param name="firstPoint"> Should be right upper corner</param>
    /// <param name="secondPoint"> Should be left bottom corner</param>
    /// <returns>a plain rectangular Mesh</returns>
    public Mesh generatePlainRectangularMesh(Vector3 startPoint, Vector3 firstPoint, Vector3 secondPoint, float quadSize)
    {
        Mesh mesh = new Mesh();

        Vector3 startToFirstPoint = firstPoint - startPoint;
        float lengthToFirst = startToFirstPoint.magnitude;
        Vector3 startToFirstPointNormalized = startToFirstPoint.normalized;
        float ratioOfPointsX = lengthToFirst/ quadSize;

        Vector3 startToSecondPoint = secondPoint - startPoint;
        float lengthToSecond = startToSecondPoint.magnitude;
        Vector3 startToSecondPointNormalized = startToSecondPoint.normalized;
        float ratioOfPointsY = lengthToSecond / quadSize;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int y = 0; y < ratioOfPointsY; y++)
        {
            for (int x = 0; x < ratioOfPointsX; x++)
            {
               
                int indexLO = vertices.Count;
                vertices.Add(startPoint + startToFirstPointNormalized * quadSize * x + startToSecondPointNormalized * quadSize * y); // links oben
                int indexRO = vertices.Count;
                vertices.Add(startPoint + startToFirstPointNormalized * quadSize * (x + Mathf.Clamp(ratioOfPointsX - x, 0.0f, 1.0f)) + startToSecondPointNormalized * quadSize * y); // rechts oben
                int indexRU = vertices.Count;
                vertices.Add(startPoint + startToFirstPointNormalized * quadSize * (x + Mathf.Clamp(ratioOfPointsX - x, 0.0f, 1.0f)) + startToSecondPointNormalized * quadSize * (y + Mathf.Clamp(ratioOfPointsY - y, 0.0f, 1.0f))); // rechts unten
                int indexLU = vertices.Count;
                vertices.Add(startPoint + startToFirstPointNormalized * quadSize * x + startToSecondPointNormalized * quadSize * (y + Mathf.Clamp(ratioOfPointsY - y, 0.0f, 1.0f))); // links unten

                uvs.Add(new Vector2(0,0));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 0));

                //rechtes oberes Dreieck
                triangles.Add(indexLO);
                triangles.Add(indexRO);
                triangles.Add(indexRU);
                // linkes unteres Dreieck
                triangles.Add(indexRU);
                triangles.Add(indexLU);
                triangles.Add(indexLO);

                
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        return mesh;
    }

    private void OnDrawGizmosSelected()
    {
        Color originColor = Gizmos.color;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(startPoint, new Vector3(0.2f, 0.2f, 0.2f));
        Gizmos.color = Color.red;
        Gizmos.DrawCube(firstPoint, new Vector3(0.2f, 0.2f, 0.2f));
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(secondPoint, new Vector3(0.2f, 0.2f, 0.2f));
        Gizmos.color = originColor;
    }

}
