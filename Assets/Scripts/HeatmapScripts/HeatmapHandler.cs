using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapHandler : MonoBehaviour
{
    private List<Mesh> HeatmapMeshes;
    private List<HeatmapData> HeatmapData;

    public float FloorQuadSize;
    public float StairQuadSize;
    public float MaxPedestriansOnQuad;

    HeatmapHandler()
    {
        HeatmapMeshes = new List<Mesh>();
        HeatmapData = new List<HeatmapData>();
    }

    public void AddToHeatmapMeshes(ref Mesh singleMesh)
    {
        this.HeatmapMeshes.Add(singleMesh);
    }

    public void AddToHeatmapData(HeatmapData data)
    {
        this.HeatmapData.Add(data);

    }

    private void UpdateUVsAtIndex(ref List<HeatmapData> heatmapData, ref List<Mesh> meshes, int indexMesh, int quadIndex) 
    {
        float amountPedestrians = heatmapData[indexMesh].amountPedestriansPerQuad[quadIndex];
        var currentUVs = meshes[indexMesh].uv;
        int vertexStartIndex = quadIndex * 4;
        float xUV = 1f * (amountPedestrians / MaxPedestriansOnQuad) + 1f / 128f;

        for (int i = 0; i < 4; i++)
        {
            currentUVs[vertexStartIndex++] = new Vector2(xUV, 0); //texture is 64*1. you should always take the middle of the pixel!
        }
        meshes[indexMesh].uv = currentUVs;
    }
    
    public void RemoveOnePedestrianFromMesh(int floorMeshIndex, int quadIndex)
    {
        this.HeatmapData[floorMeshIndex].DecreaseAmountPedestrianAtIndex(quadIndex);
        this.UpdateUVsAtIndex(ref this.HeatmapData, ref this.HeatmapMeshes, floorMeshIndex, quadIndex);
    }

    public void AddOnePedestrianToMesh(int MeshIndex, int quadIndex)
    {
        this.HeatmapData[MeshIndex].IncreaseAmountPedestrianAtIndex(quadIndex);
        this.UpdateUVsAtIndex(ref this.HeatmapData, ref this.HeatmapMeshes, MeshIndex, quadIndex);
    }

    public bool GetAffectedIndeces(Vector3 newPosition, out int stairIndex, out int quadIndex) //Since multiple stairs can be on the same height we have to do the other checks directly (not only check for candidates)
    {
        //1. potential stair by comparing y value (min max, it has to be in between)
        for (int i = 0; i < HeatmapMeshes.Count; i++)
        {
            if (this.HeatmapData[i].MeshIsOnSameHeight(newPosition)) //if true, pedestrian ist most probably within this mesh (height fits, x and z need to be tested though)
            {
                var resultingIndex = this.HeatmapData[i].GetQuadIndexFromCoords(newPosition); //TODO: hardcoded size, fix!
                if (resultingIndex != -1)
                {
                    quadIndex = resultingIndex;
                    stairIndex = i;
                    return true;
                }
            }
        }
        quadIndex = -1;
        stairIndex = -1;
        return false;
    }

}
