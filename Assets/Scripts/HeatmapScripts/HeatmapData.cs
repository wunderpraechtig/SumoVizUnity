using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapData
{


    private Vector3 StartingPoint;
    private float Width, Height;
    private int AmountQuadsWidth, AmountQuadsHeight; // amount of quads with considering the potential edge quads
    public int[] amountPedestriansPerQuad;

    //private int _pedestrianCount;
    //public int PedestrianCount {
    //    get { return _pedestrianCount; }
    //    set { _pedestrianCount = value; } 
    //}

    public HeatmapData(Vector3 startingPoint, float width, float height, int amountsQuadWidth, int amountQuadsHeight)
    {
        //this.PedestrianCount = 0;
        this.StartingPoint = startingPoint;
        this.Width = width;
        this.Height = height;
        this.AmountQuadsWidth = amountsQuadWidth;
        this.AmountQuadsHeight = amountQuadsHeight;
        int totalAmountQuads = AmountQuadsWidth * AmountQuadsHeight;
        this.amountPedestriansPerQuad = new int[totalAmountQuads]; //TODO: does this work?
        for (int i=0; i< totalAmountQuads; i++)
        {
            amountPedestriansPerQuad[i] = 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xCoord"></param>
    /// <param name="zCoord"></param>
    /// <param name="quadSize"></param>
    /// <returns>null if coords are outside of the mesh, otherwise returns the index of the first affected vertex (the 5 next indeces need to be taken as well!)</returns>
    public int getVertexIndexFromCoords(float xCoord, float zCoord, float quadSize) //TODO: rename since it also changes the list!, TODO: exception
    {
        if(xCoord < StartingPoint.x || xCoord > (StartingPoint.x+Width ) || zCoord < (StartingPoint.z-Height) || zCoord > StartingPoint.z)
        {
            //return null; //in this case, the x and/or z Coord lies outside of the mesh. That actually shouldnt happen ..
            int debug = 1;
        }

        float distanceToLeftEdge = xCoord - StartingPoint.x;
        int xOffset = (int) (distanceToLeftEdge / quadSize); //cast to int!

        float distanceToTopEdge = StartingPoint.z - zCoord;
        int zOffset = (int)(distanceToTopEdge / quadSize); //cast to int!

        //int quadsInOneRow = AmountQuadsWidth;

        //if (EdgeQuadsWidthExist)
        //{
        //    quadsInOneRow = AmountQuadsWidth + 1;
        //}

        int resultingIndex = ((AmountQuadsWidth * zOffset) + xOffset);

        this.amountPedestriansPerQuad[resultingIndex] = this.amountPedestriansPerQuad[resultingIndex] + 1;

        return resultingIndex*6; // per quad we have 6 vertices - therefore we need to multiply the result with 6!
    }

    //private void Update()
    //{
    //    //reset pedestriancount every frame to 0 ??
    //    //this.PedestrianCount = 0;
    //}
}
