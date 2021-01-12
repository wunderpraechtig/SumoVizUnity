using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapHandler : MonoBehaviour
{
    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {

        transform.position += new Vector3(0, -1, 0);

    }

    public void setUpHeatmap()
    {

        mesh = this.GetComponent<MeshFilter>().mesh;

        //uv stuff

        Vector3[] vertices = mesh.vertices;

        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < uvs.Length; i++)
        //for (int i = 0; i < mesh.uv.Length; i++)
        {
            //uvs[i] = new Vector2(vertices[i].x, vertices[i].z);

            uvs[i] = new Vector2(0.25f, 0);

            //mesh.uv[i] = new Vector2(60, 0);
        }
        mesh.uv = uvs;

        this.GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
