using UnityEngine;

public class PedestrianPoolObject
{
    public GameObject obj;
    public Renderer pedestrianRenderer;
    public Animation componentAnimation;
    public AnimationState animationWalking;
    public GameObject tileObject;
    public LineRenderer trajectoryRenderer;
    public Renderer tileRenderer;
    public Color color;

    public PedestrianPoolObject(GameObject _obj, Color _color, Transform parent) {
        obj = _obj;
        pedestrianRenderer = obj.GetComponentInChildren<Renderer>();
        componentAnimation = obj.GetComponent<Animation>();
        animationWalking = componentAnimation["walking"];
        componentAnimation.Stop();
        color = _color;
        pedestrianRenderer.materials[1].color = color;

        // Reset transform
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // Add tile
        float side = 1.0f;
        tileObject = new GameObject("tile", typeof(MeshFilter), typeof(MeshRenderer));
        MeshFilter mesh_filter = tileObject.GetComponent<MeshFilter>();
        tileRenderer = tileObject.GetComponent<Renderer>();
        tileRenderer.material = (Material)Resources.Load("Tilematerial", typeof(Material));
        tileRenderer.material.color = Color.red;
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { new Vector3(-side / 2, 0.01f, -side / 2), new Vector3(side / 2, 0.01f, -side / 2), new Vector3(-side / 2, 0.01f, side / 2), new Vector3(side / 2, 0.01f, side / 2) };
        mesh.triangles = new int[] { 2, 1, 0, 1, 2, 3 };


        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        int i = 0;
        while (i < uvs.Length)
        {
            uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
            i++;
        }
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh_filter.mesh = mesh;

        tileObject.transform.position = obj.transform.position;
        tileObject.transform.parent = obj.transform;
        tileObject.transform.localScale = Vector3.one;

        // Create trajectory line gameobject
        GameObject lineObj = new GameObject("trajectory");
        lineObj.transform.parent = parent;
        lineObj.transform.localPosition = Vector3.zero;
        lineObj.transform.localRotation = Quaternion.identity;
        lineObj.transform.localScale = Vector3.one;
        lineObj.AddComponent<LineRenderer>();
        trajectoryRenderer = lineObj.GetComponent<LineRenderer>();

        trajectoryRenderer.material = new Material((Material)Resources.Load("LineMaterial", typeof(Material)));
        trajectoryRenderer.material.SetColor("_EmissionColor", color);
        trajectoryRenderer.material.SetColor("_Color", color);
        trajectoryRenderer.startColor = color;
        trajectoryRenderer.endColor = color;
        trajectoryRenderer.startWidth = 0.08f;
        trajectoryRenderer.endWidth = 0.08f;
        trajectoryRenderer.positionCount = 0;
        trajectoryRenderer.useWorldSpace = false;
    }
}
