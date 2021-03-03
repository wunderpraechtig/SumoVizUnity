using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapHandler : MonoBehaviour
{
    private List<Mesh> FloorHeatmapMeshes;
    private List<HeatmapData> FloorHeatmapData;    
    private List<Mesh> StairsHeatmapMeshes;
    private List<HeatmapData> StairsHeatmapData;
    private float PedestrianHeightTolerance;

    public float QuadSize;
    public float MaxPedestriansOnQuad;

    HeatmapHandler()
    {
        FloorHeatmapMeshes = new List<Mesh>();
        FloorHeatmapData = new List<HeatmapData>();
        this.PedestrianHeightTolerance = 0.01f;
    }


    public void AddToHeatmapData(HeatmapData data)
    {
        this.FloorHeatmapData.Add(data);

    }
    private void UpdateUVsAtIndex(int indexMesh, int quadIndex) //update UV entsprechend pedestriancount erhöhen!
    {
        float amountPedestrians = FloorHeatmapData[indexMesh].amountPedestriansPerQuad[quadIndex];
        var currentUVs = FloorHeatmapMeshes[indexMesh].uv;
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
        FloorHeatmapMeshes[indexMesh].uv = currentUVs;
    }

    public void RemoveOnePedestrian(int meshIndex, int quadIndex)
    {
        this.FloorHeatmapData[meshIndex].DecreaseAmountPedestrianAtIndex(quadIndex);
        this.UpdateUVsAtIndex(meshIndex, quadIndex);
    }

    public void AddOnePedestrian(int meshIndex, int quadIndex)
    {
        this.FloorHeatmapData[meshIndex].IncreaseAmountPedestrianAtIndex(quadIndex);
        this.UpdateUVsAtIndex(meshIndex, quadIndex);
    }

    public bool GetMeshAndQuadIndeces(Vector3 position, out int meshIndex, out int quadIndex)
    {
        var resutingMeshIndexNullable = AffectedMeshIndex(position.y);
        if (resutingMeshIndexNullable.HasValue)
        {
            meshIndex = resutingMeshIndexNullable.Value;
        }
        else
        {
            meshIndex = -1;
            quadIndex = -1;
            return false; //currently gets here if pedestrian is on stairs
        }

        var affectedQuadIndex = FloorHeatmapData[meshIndex].getQuadIndexFromCoords(position.x, position.z, QuadSize);
        if (affectedQuadIndex == -1)
        {
            meshIndex = -1;
            quadIndex = -1;
            return false;
        }
        quadIndex = affectedQuadIndex;
        return true; //successfully updated

    }

    private int? AffectedMeshIndex(float pedestrianHeight) //TODO: exception
    {
        for (int i = 0; i < this.FloorHeatmapMeshes.Count; i++)
        {
            var heightCurrentMesh = this.FloorHeatmapMeshes[i].vertices[0].y;
            if (pedestrianHeight < heightCurrentMesh + PedestrianHeightTolerance && pedestrianHeight > heightCurrentMesh - PedestrianHeightTolerance) //if true, pedestrian ist most probably within this mesh
            {
                return i;
            }
        }
        return null; //should hopefully never happen //happens, if pedestrian is on stairs...
    }

    public void AddToHeatmapMeshes(ref Mesh singleMesh)
    {
        this.FloorHeatmapMeshes.Add(singleMesh);
    }

}
