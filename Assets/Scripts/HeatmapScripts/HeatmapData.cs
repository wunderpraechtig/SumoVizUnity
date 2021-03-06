﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeatmapData
{


    private Vector3 StartingPoint; // smallest x and biggest z value = farleft
    private Vector3 WidthVector;
    private Vector3 LengthVector;
    private Vector3 FarRight; //biggest x and biggest z value
    private Vector3 NearLeft; //smallest x and smallest z value
    private Plane plane;
    private float Width, Length;
    private int AmountQuadsWidth, AmountQuadsHeight; // amount of quads with considering the potential edge quads
    private float PedestrianHeightTolerance;
    private float quadSize;

    public float[] amountPedestriansPerQuad;
    private Mesh HeatmapMesh;



    public HeatmapData(Vector3 startingPoint, Vector3 widthVector, Vector3 lengthVector, Vector3 farRight, Vector3 nearLeft, float width, float length, int amountsQuadWidth, int amountQuadsHeight, float quadSize)
    {
        //this.PedestrianCount = 0;
        this.StartingPoint = startingPoint;
        this.WidthVector = widthVector;
        this.LengthVector = lengthVector;
        this.plane = new Plane(startingPoint, farRight, nearLeft);
        this.FarRight = farRight;
        this.NearLeft = nearLeft;
        this.Width = width;
        this.Length = length;
        this.AmountQuadsWidth = amountsQuadWidth;
        this.AmountQuadsHeight = amountQuadsHeight;
        this.quadSize = quadSize;
        int totalAmountQuads = AmountQuadsWidth * AmountQuadsHeight;
        this.amountPedestriansPerQuad = new float[totalAmountQuads];
        this.PedestrianHeightTolerance = 0.01f;
        for (int i = 0; i < totalAmountQuads; i++)
        {
            amountPedestriansPerQuad[i] = 0;
        }
    }

    public void setHeatmapMesh(Mesh heatmapMesh)
    {
        this.HeatmapMesh = heatmapMesh;
    }

    public List<int> IncreaseAmountPedestrianAtIndexAndSurrounding(int centerQuadIndex, float spreadFactor)
    {
        List<int> quadIndices = new List<int>();
        //int row = centerQuadIndex / AmountQuadsWidth + 1;
        int column = centerQuadIndex % AmountQuadsWidth;

        float currentAmountPedestrians = ++this.amountPedestriansPerQuad[centerQuadIndex];

        try
        {


            //N4: above quad, below quad, right quad, left quad
            //if (row - 1 != 0) //above row exists
            if (centerQuadIndex - AmountQuadsWidth >= 0) //above row exists
            {
                quadIndices.Add(centerQuadIndex - AmountQuadsWidth);
                //this.amountPedestriansPerQuad[centerQuadIndex - AmountQuadsWidth] += currentAmountPedestrians * spreadFactor;
                this.amountPedestriansPerQuad[centerQuadIndex - AmountQuadsWidth]++;
            }
            //if (row + 1 <= AmountQuadsHeight) //below row exists
            if (centerQuadIndex + AmountQuadsWidth < AmountQuadsWidth * AmountQuadsHeight) //below row exists
            {
                quadIndices.Add(centerQuadIndex + AmountQuadsWidth);
                //this.amountPedestriansPerQuad[centerQuadIndex + AmountQuadsWidth] += currentAmountPedestrians * spreadFactor;
                this.amountPedestriansPerQuad[centerQuadIndex + AmountQuadsWidth]++;
            }
            if (column - 1 >= 0) //left column exists
            {
                quadIndices.Add(centerQuadIndex - 1);
                //this.amountPedestriansPerQuad[centerQuadIndex - 1] += currentAmountPedestrians * spreadFactor;
                this.amountPedestriansPerQuad[centerQuadIndex - 1]++;
            }
            if (column + 1 < AmountQuadsWidth) //right column exists
            {
                quadIndices.Add(centerQuadIndex + 1);
                //this.amountPedestriansPerQuad[centerQuadIndex + 1] += currentAmountPedestrians * spreadFactor;
                this.amountPedestriansPerQuad[centerQuadIndex + 1]++;
            }


            return quadIndices;
        }
        catch (IndexOutOfRangeException e)
        {
            var msg = e.Message;
            var data = e.Data;
        }
        return null;
    }

    public List<int> DecreaseAmountPedestrianAtIndexAndSurrounding(int centerQuadIndex, float spreadFactor)
    {
        List<int> quadIndices = new List<int>();
        int column = centerQuadIndex % AmountQuadsWidth;

        float previousAmountPedestrians = this.amountPedestriansPerQuad[centerQuadIndex]--;

        try
        {


            //N4: above quad, below quad, right quad, left quad
            //if (row - 1 != 0) //above row exists
            if (centerQuadIndex - AmountQuadsWidth >= 0) //above row exists
            {
                quadIndices.Add(centerQuadIndex - AmountQuadsWidth);
                //this.amountPedestriansPerQuad[centerQuadIndex - AmountQuadsWidth] -= previousAmountPedestrians * spreadFactor;
                this.amountPedestriansPerQuad[centerQuadIndex - AmountQuadsWidth]--;
            }
            //if (row + 1 <= AmountQuadsHeight) //below row exists
            if (centerQuadIndex + AmountQuadsWidth < AmountQuadsWidth * AmountQuadsHeight) //below row exists
            {
                quadIndices.Add(centerQuadIndex + AmountQuadsWidth);
                //this.amountPedestriansPerQuad[centerQuadIndex + AmountQuadsWidth] -= previousAmountPedestrians * spreadFactor;
                this.amountPedestriansPerQuad[centerQuadIndex + AmountQuadsWidth]--;
            }
            if (column - 1 >= 0) //left column exists
            {
                quadIndices.Add(centerQuadIndex - 1);
                //this.amountPedestriansPerQuad[centerQuadIndex - 1] -= previousAmountPedestrians * spreadFactor;
                this.amountPedestriansPerQuad[centerQuadIndex - 1]--;
            }
            if (column + 1 < AmountQuadsWidth) //right column exists
            {
                quadIndices.Add(centerQuadIndex + 1);
                //this.amountPedestriansPerQuad[centerQuadIndex + 1] -= previousAmountPedestrians * spreadFactor;
                this.amountPedestriansPerQuad[centerQuadIndex + 1]--;
            }


            return quadIndices;
        }
        catch (IndexOutOfRangeException e)
        {
            var msg = e.Message;
            var data = e.Data;
        }
        return null;
    }

    public void IncreaseAmountPedestrianAtIndex(int index)
    {
        this.amountPedestriansPerQuad[index] = this.amountPedestriansPerQuad[index] + 1;
    }

    public void DecreaseAmountPedestrianAtIndex(int index)
    {
        this.amountPedestriansPerQuad[index] = this.amountPedestriansPerQuad[index] - 1;
    }

    public void UpdateUVsAtIndex(int quadIndex, float maxPedestriansOnQuad)
    {
        float amountPedestrians = amountPedestriansPerQuad[quadIndex];
        var currentUVs = HeatmapMesh.uv;
        int vertexStartIndex = quadIndex * 4;
        float xUV;

        xUV = 1f * (amountPedestrians / maxPedestriansOnQuad) + 1f / 128f;

        for (int i = 0; i < 4; i++)
        {
            currentUVs[vertexStartIndex++] = new Vector2(xUV, 0);
        }
        this.HeatmapMesh.uv = currentUVs;
    }

    public bool MeshIsOnSameHeight(Vector3 pedestrianPosition)
    {
        var result = Mathf.Abs(this.plane.GetDistanceToPoint(pedestrianPosition));
        if (result < PedestrianHeightTolerance)
        {
            return true;
        }
        return false;
    }

    public int GetQuadIndexFromCoords(Vector3 position) //TODO: rename since it also changes the list!, TODO: exception
    {
        float farthestZCoord = StartingPoint.z >= FarRight.z ? StartingPoint.z : FarRight.z;
        float leftMostXCoord = StartingPoint.x <= NearLeft.x ? StartingPoint.x : NearLeft.x;
        if (position.x < leftMostXCoord || position.z > farthestZCoord) //if it is not within these, its definitely not in the plane of the stair
        {
            return -1;
        }

        //important first step: position needs to be put relative to starting point!!!

        Vector3 movedPosition = position - StartingPoint;

        Vector3 projectionWidth = Vector3.Project(movedPosition, WidthVector);
        Vector3 projectionLength = Vector3.Project(movedPosition, LengthVector);

        if (projectionWidth.magnitude > Width || projectionLength.magnitude > Length) //in case the pedestrian is outside of mesh
        {
            return -1;
        }

        int xOffset = (int)(projectionWidth.magnitude / this.quadSize); //cast to int!
        int zOffset = (int)(projectionLength.magnitude / this.quadSize); //cast to int!

        int resultingIndex = ((AmountQuadsWidth * zOffset) + xOffset);

        return resultingIndex; // per quad we have 6 vertices - therefore we need to multiply the result with 6!
    }

    public void resetUVs()
    {
        Vector2[] clearUVs = new Vector2[(HeatmapMesh.uv.Length)];
        foreach (var entry in clearUVs)
        {
            var current = entry;
            current.x = 0;
            current.y = 0;
        }
        HeatmapMesh.uv = clearUVs;
    }

}
