using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapHandler : MonoBehaviour
{
    private List<Mesh> HeatmapMeshes;
    private List<HeatmapData> HeatmapData;
    private float PedestrianHeightTolerance;

    public float QuadSize;

    HeatmapHandler()
    {
        HeatmapMeshes = new List<Mesh>();
        HeatmapData = new List<HeatmapData>();
        this.PedestrianHeightTolerance = 0.01f;
    }


    public void AddToHeatmapData(HeatmapData data)
    {
        this.HeatmapData.Add(data);

    }



    public void UpdateAllUVs() //TODO: rename? die fkt updated nur uvs von leeren quads.. 
    {
        for (int i = 0; i < HeatmapData.Count; i++)
        {
            var currentHeatmap = HeatmapData[i];
            for (int j = 0; j < currentHeatmap.amountPedestriansPerQuad.Length; j++)
            {
                int currentEntry = currentHeatmap.amountPedestriansPerQuad[j];
                var UVs = HeatmapMeshes[i].uv;
                if (currentEntry == 0)
                {
                    UVs[j * 6] = new Vector2(0, 0);
                    UVs[j * 6 + 1] = new Vector2(0, 0);
                    UVs[j * 6 + 2] = new Vector2(0, 0);
                    UVs[j * 6 + 3] = new Vector2(0, 0);
                    UVs[j * 6 + 4] = new Vector2(0, 0);
                    UVs[j * 6 + 5] = new Vector2(0, 0);
                }
                else
                {
                    //never called??!! TODO: prob yes because they are all reset!
                    //1 should result in (1,1), values greater than X should be 1/64
                    UVs[j * 6] = new Vector2(1, 1);
                    UVs[j * 6 + 1] = new Vector2(1, 1);
                    UVs[j * 6 + 2] = new Vector2(1, 1);
                    UVs[j * 6 + 3] = new Vector2(1, 1);
                    UVs[j * 6 + 4] = new Vector2(1, 1);
                    UVs[j * 6 + 5] = new Vector2(1, 1);

                    //UVs[j * 6] = new Vector2(1 / 64, 1);
                    //UVs[j * 6 + 1] = new Vector2(1 / 64, 1);
                    //UVs[j * 6 + 2] = new Vector2(1 / 64, 1);
                    //UVs[j * 6 + 3] = new Vector2(1 / 64, 1);
                    //UVs[j * 6 + 4] = new Vector2(1 / 64, 1);
                    //UVs[j * 6 + 5] = new Vector2(1 / 64, 1);
                }

                HeatmapMeshes[i].uv = UVs;
            }
        }
    }

    public void UpdateUVsAtIndex(int indexMesh, int indexVertices)
    {
        var currentUVs = HeatmapMeshes[indexMesh].uv;
        for (int i = 0; i < 6; i++)
        {
            currentUVs[indexVertices++] = new Vector2(1, 1); //TODO: find rule for which color should be used, depending on the amount of pedestrians in quad
        }
        HeatmapMeshes[indexMesh].uv = currentUVs;
    }


    public bool TransmitPedestrianPosition(Vector3 position) //TODO: exception //TODO rename: transmit position oder so
    {

        var resutingMeshIndexNullable = AffectedMeshIndex(position.y); 
        int affectedMeshIndex; //TODO: with exception
        if (resutingMeshIndexNullable.HasValue)
        {
            affectedMeshIndex = resutingMeshIndexNullable.Value;
        }
        else
        {
            return false; //currently gets here if pedestrian is on stairs
        }

        var affectedVerticesStartIndex = HeatmapData[affectedMeshIndex].getVertexIndexFromCoords(position.x, position.z, QuadSize);
        if (affectedVerticesStartIndex == -1)
        {
            return false;
        }
        UpdateUVsAtIndex(affectedMeshIndex, affectedVerticesStartIndex);
        return true; //successfully updated
    }

    private int? AffectedMeshIndex(float pedestrianHeight) //TODO: exception
    {
        for (int i = 0; i < this.HeatmapMeshes.Count; i++)
        {
            var heightCurrentMesh = this.HeatmapMeshes[i].vertices[0].y;
            if (pedestrianHeight < heightCurrentMesh + PedestrianHeightTolerance && pedestrianHeight > heightCurrentMesh - PedestrianHeightTolerance) //if true, pedestrian ist most probably within this mesh
            {
                return i;
            }
        }
        return null; //should hopefully never happen //happens, if pedestrian is on stairs
    }

    public void AddToHeatmapMeshes(ref Mesh singleMesh)
    {
        this.HeatmapMeshes.Add(singleMesh);
    }

    // Start is called before the first frame update
    void Start()
    {


    }


    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < HeatmapData.Count; i++)
        {
            for (int j = 0; j < HeatmapData[i].amountPedestriansPerQuad.Length; j++)
            {
                HeatmapData[i].amountPedestriansPerQuad[j] = 0;
            }
        }
        UpdateAllUVs();
    }
}
