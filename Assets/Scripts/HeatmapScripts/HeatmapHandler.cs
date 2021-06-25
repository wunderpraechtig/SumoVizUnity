using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapHandler : MonoBehaviour
{
    //private List<Mesh> HeatmapMeshes;
    private List<HeatmapData> HeatmapData;

    public float FloorQuadSize;
    public float StairQuadSize;
    public float MaxPedestriansOnQuad;

    HeatmapHandler()
    {
        //HeatmapMeshes = new List<Mesh>();
        HeatmapData = new List<HeatmapData>();
    }

    public void AddToHeatmapData(ref HeatmapData data)
    {
        this.HeatmapData.Add(data);

    }


    public void AddPedestrianToMultipleQuads(int meshIndex, int centerQuadIndex)
    {
        var manipulatedQuadIndices = this.HeatmapData[meshIndex].IncreaseAmountPedestrianAtIndexAndSurrounding(centerQuadIndex, 0.5f);
        manipulatedQuadIndices.Add(centerQuadIndex);
        for (int i = 0; i < manipulatedQuadIndices.Count; i++)
        {
            //this.UpdateUVsAtIndex(ref this.HeatmapData, ref this.HeatmapMeshes, meshIndex, manipulatedQuadIndices[i]);
            HeatmapData[meshIndex].UpdateUVsAtIndex(manipulatedQuadIndices[i], this.MaxPedestriansOnQuad);
        }
    }
    public void RemovePedestriansFromMultipleQuads(int meshIndex, int centerQuadIndex)
    {
        var manipulatedQuadIndices = this.HeatmapData[meshIndex].DecreaseAmountPedestrianAtIndexAndSurrounding(centerQuadIndex, 0.5f);
        manipulatedQuadIndices.Add(centerQuadIndex);
        for (int i = 0; i < manipulatedQuadIndices.Count; i++)
        {
            //this.UpdateUVsAtIndex(ref this.HeatmapData, ref this.HeatmapMeshes, meshIndex, manipulatedQuadIndices[i]);
            HeatmapData[meshIndex].UpdateUVsAtIndex(manipulatedQuadIndices[i], this.MaxPedestriansOnQuad);
        }
    }


    public void AddOnePedestrianToMeshAndQuad(int meshIndex, int quadIndex)
    {
        this.HeatmapData[meshIndex].IncreaseAmountPedestrianAtIndex(quadIndex);
        //this.UpdateUVsAtIndex(ref this.HeatmapData, ref this.HeatmapMeshes, meshIndex, quadIndex);
        HeatmapData[meshIndex].UpdateUVsAtIndex(quadIndex, this.MaxPedestriansOnQuad);
    }
    public void RemoveOnePedestrianFromMeshAndQuad(int meshIndex, int quadIndex)
    {
        this.HeatmapData[meshIndex].DecreaseAmountPedestrianAtIndex(quadIndex);
        //this.UpdateUVsAtIndex(ref this.HeatmapData, ref this.HeatmapMeshes, floorMeshIndex, quadIndex);
        HeatmapData[meshIndex].UpdateUVsAtIndex(quadIndex, this.MaxPedestriansOnQuad);
    }

    public bool GetAffectedIndeces(Vector3 newPosition, out int meshIndex, out int quadIndex) 
    {
        for (int i = 0; i < HeatmapData.Count; i++)
        {
            if (this.HeatmapData[i].MeshIsOnSameHeight(newPosition)) //if true, pedestrian ist most probably within this mesh (height fits, x and z ranges need to be tested though)
            {
                var resultingIndex = this.HeatmapData[i].GetQuadIndexFromCoords(newPosition);
                if (resultingIndex != -1)
                {
                    quadIndex = resultingIndex;
                    meshIndex = i;
                    return true;
                }
            }
        }
        quadIndex = -1;
        meshIndex = -1;
        return false;
    }

    public void clearHeatmaps()
    {
        foreach(var entry in HeatmapData)
        {
            entry.resetUVs();
        }
    }


}
