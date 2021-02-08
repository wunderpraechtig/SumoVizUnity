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
        //this.QuadSize = 1; //TODO: right now hardcoded
        this.PedestrianHeightTolerance = 0.01f;
    }


    public void AddToHeatmapData(HeatmapData data)
    {
        this.HeatmapData.Add(data);

    }



    public void RefreshUVs()
    {
        for (int i = 0; i < HeatmapData.Count; i++)
        {
            var currentHeatmap = HeatmapData[i];
            for (int j = 0; j < currentHeatmap.amountPedestriansPerQuad.Length; j++)
            {
                int currentEntry = currentHeatmap.amountPedestriansPerQuad[j];
                var UVs = HeatmapMeshes[i].uv;
                if (currentEntry > 0) //TODO: how many pedestrians lead to which color?
                {
                    UVs[j * 6] = new Vector2(1, 1);
                    UVs[j * 6+1] = new Vector2(1, 1);
                    UVs[j * 6+2] = new Vector2(1, 1);
                    UVs[j * 6+3] = new Vector2(1, 1);
                    UVs[j * 6+4] = new Vector2(1, 1);
                    UVs[j * 6+5] = new Vector2(1, 1);

                }
                else if (currentEntry == 0)
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
                    int debug = 1; //houston, we have a problem //TODO: delet this //exception

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

    //public bool AddPedestrian(PedestrianPosition position) //TODO: exception //TODO rename: transmit position oder so
    //{

    //    var resutingIndex = AffectedMeshIndex(position.getZ()); //for the pedestriandata z seems to be the height, while for meshes y is the height!
    //    int affectedIndex;
    //    if (resutingIndex.HasValue)
    //    {
    //        affectedIndex = resutingIndex.Value;
    //    }
    //    else
    //    {
    //        return false;
    //    }

    //    var affectedVertices = HeatmapData[affectedIndex].getVertexIndexFromCoords(position.getX(), position.getZ(), QuadSize);
    //    UpdateUVsAtIndex(affectedIndex, affectedVertices);
    //    return true; //successfully updated
    //}

    public bool AddPedestrian(Vector3 position) //TODO: exception //TODO rename: transmit position oder so
    {

        var resutingIndex = AffectedMeshIndex(position.y); //for the pedestriandata z seems to be the height, while for meshes y is the height! But the pedestriansystem uses y for height again...
        int affectedIndex;
        if (resutingIndex.HasValue)
        {
            affectedIndex = resutingIndex.Value;
        }
        else
        {
            return false; //currently gets here if pedestrian is on stairs
        }

        var affectedVertices = HeatmapData[affectedIndex].getVertexIndexFromCoords(position.x, position.z, QuadSize);
        if (affectedVertices == -1)
        {
            return false;
        }
        UpdateUVsAtIndex(affectedIndex, affectedVertices);
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
        RefreshUVs();
    }
}
