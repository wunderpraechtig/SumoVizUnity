using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapHandler : MonoBehaviour
{
    private List<Mesh> FloorHeatmapMeshes;
    private List<HeatmapData> FloorHeatmapData;
    private List<Mesh> StairsHeatmapMeshes;
    private List<HeatmapData> StairsHeatmapData;
    //private float PedestrianHeightTolerance;

    public float QuadSize;
    public float MaxPedestriansOnQuad;

    HeatmapHandler()
    {
        FloorHeatmapMeshes = new List<Mesh>();
        FloorHeatmapData = new List<HeatmapData>();
        StairsHeatmapMeshes = new List<Mesh>();
        StairsHeatmapData = new List<HeatmapData>();
        //this.PedestrianHeightTolerance = 0.01f;
    }
    public void AddToFloorHeatmapMeshes(ref Mesh singleMesh)
    {
        this.FloorHeatmapMeshes.Add(singleMesh);
    }
    public void AddToStairHeatmapMeshes(ref Mesh singleMesh)
    {
        this.StairsHeatmapMeshes.Add(singleMesh);
    }

    public void AddToFloorsHeatmapData(HeatmapData data)
    {
        this.FloorHeatmapData.Add(data);

    }

    public void AddToStairsHeatmapData(HeatmapData data)
    {
        this.StairsHeatmapData.Add(data);

    }

    private void UpdateUVsAtIndex(ref List<HeatmapData> heatmapData, ref List<Mesh> meshes, int indexMesh, int quadIndex) //update UV entsprechend pedestriancount erhöhen!
    {
        float amountPedestrians = heatmapData[indexMesh].amountPedestriansPerQuad[quadIndex];
        var currentUVs = meshes[indexMesh].uv;
        int vertexStartIndex = quadIndex * 6;
        //float xUV = 1f / 64f * (amountPedestrians / MaxPedestriansOnQuad) + 1f / 128f;
        float xUV = 1f * (amountPedestrians / MaxPedestriansOnQuad) + 1f / 128f;

        //if (xUV != 0 && xUV < 1f / 128f + 1f / 64f)
        //{
        //    xUV = 1f / 128f + 1f / 64f;
        //} 
        //if(amountPedestrians >= MaxPedestriansOnQuad)
        //{
        //    int debug = 1;
        //}

        for (int i = 0; i < 6; i++)
        {
            //float result = (amountPedestrians / MaxPedestriansOnQuad);
            currentUVs[vertexStartIndex++] = new Vector2(xUV, 0); //texture is 64*1. you should always take the middle of the pixel!

        }
        meshes[indexMesh].uv = currentUVs;
    }

    public void RemoveOnePedestrianFromFloorMesh(int floorMeshIndex, int quadIndex)
    {
        this.FloorHeatmapData[floorMeshIndex].DecreaseAmountPedestrianAtIndex(quadIndex);
        this.UpdateUVsAtIndex(ref this.FloorHeatmapData, ref this.FloorHeatmapMeshes, floorMeshIndex, quadIndex);
    }

    public void AddOnePedestrianToFloorMesh(int floorMeshIndex, int quadIndex)
    {
        this.FloorHeatmapData[floorMeshIndex].IncreaseAmountPedestrianAtIndex(quadIndex);
        this.UpdateUVsAtIndex(ref this.FloorHeatmapData, ref this.FloorHeatmapMeshes, floorMeshIndex, quadIndex);
    }
    public void RemoveOnePedestrianFromStairMesh(int stairMeshIndex, int quadIndex)
    {
        this.StairsHeatmapData[stairMeshIndex].DecreaseAmountPedestrianAtIndex(quadIndex);
        this.UpdateUVsAtIndex(ref this.StairsHeatmapData, ref this.StairsHeatmapMeshes, stairMeshIndex, quadIndex);
    }

    public void AddOnePedestrianToStairMesh(int stairMeshIndex, int quadIndex)
    {
        this.StairsHeatmapData[stairMeshIndex].IncreaseAmountPedestrianAtIndex(quadIndex);
        this.UpdateUVsAtIndex(ref this.StairsHeatmapData, ref this.StairsHeatmapMeshes, stairMeshIndex, quadIndex);
    }

    public bool GetFloorMeshAndQuadIndeces(Vector3 position, out int meshIndex, out int quadIndex)
    {
        //var resutingMeshIndexNullable = AffectedMeshIndex(position.y);
        var resutingMeshIndexNullable = AffectedFloorMeshIndexByHeight(position);
        if (resutingMeshIndexNullable.HasValue)
        {
            meshIndex = resutingMeshIndexNullable.Value;
        }
        else
        {
            meshIndex = -1;
            quadIndex = -1;
            return false; //currently also gets here if pedestrian is on stairs
        }

        var affectedMeshQuadIndex = FloorHeatmapData[meshIndex].GetQuadIndexFromCoords(position, QuadSize);
        if (affectedMeshQuadIndex == -1)
        {
            meshIndex = -1;
            quadIndex = -1;
            return false;
        }
        quadIndex = affectedMeshQuadIndex;
        return true; //successfully updated

    }





    public bool GetAffectedStairIndex(Vector3 newPosition, out int stairIndex, out int quadIndex) //Since multiple stairs can be on the same height we have to do the other checks directly (not only check for candidates)
    {
        //1. potential stair by comparing y value (min max, it has to be in between)
        for (int i = 0; i < StairsHeatmapMeshes.Count; i++)
        {
            if (newPosition.y >= this.StairsHeatmapMeshes[i].bounds.min.y && newPosition.y <= this.StairsHeatmapMeshes[i].bounds.max.y) //fitting stair candidate
            {
                var resultingIndex = this.StairsHeatmapData[i].GetQuadIndexFromCoords(newPosition, 1); //TODO: hardcoded size, fix!
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

    private int? AffectedFloorMeshIndexByHeight(Vector3 pedestrianPosition) //TODO: exception?
    {
        for (int i = 0; i < this.FloorHeatmapMeshes.Count; i++)
        {
            if (this.FloorHeatmapData[i].MeshIsOnSameHeight(pedestrianPosition)) //if true, pedestrian ist most probably within this mesh (height fits, x and z need to be tested though)
            {
                return i;
            }
        }
        return null; //should hopefully never happen //edit: happens, if pedestrian is on stairs...
    }
}
