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
    public int[] amountPedestriansPerQuad;
    private float PedestrianHeightTolerance;


    public HeatmapData(Vector3 startingPoint, Vector3 widthVector, Vector3 lengthVector, Vector3 farRight, Vector3 nearLeft, float width, float length, int amountsQuadWidth, int amountQuadsHeight)
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
        int totalAmountQuads = AmountQuadsWidth * AmountQuadsHeight;
        this.amountPedestriansPerQuad = new int[totalAmountQuads];
        this.PedestrianHeightTolerance = 0.001f;
        for (int i = 0; i < totalAmountQuads; i++)
        {
            amountPedestriansPerQuad[i] = 0;
        }
    }

    public void IncreaseAmountPedestrianAtIndex(int index)
    {
        this.amountPedestriansPerQuad[index] = this.amountPedestriansPerQuad[index] + 1;
    }

    public void DecreaseAmountPedestrianAtIndex(int index)
    {
        this.amountPedestriansPerQuad[index] = this.amountPedestriansPerQuad[index] - 1;
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

    public int GetQuadIndexFromCoords(float xCoord, float zCoord, float quadSize) //TODO: rename since it also changes the list!, TODO: exception
    {
        float farthestZCoord;
        farthestZCoord = StartingPoint.z >= FarRight.z ? StartingPoint.z : FarRight.z;
        if (xCoord < StartingPoint.x || zCoord > farthestZCoord) //if it is not within these, its definitely not in the plane of the stair
        {
            //return null; //in this case, the x and/or z Coord lies outside of the mesh. That actually shouldnt happen ..
            return -1; //TODO: take care of this! happens in osloer
        }
        //Ray ray = new Ray();


        float distanceToLeftEdge = xCoord - StartingPoint.x;
        int xOffset = (int)(distanceToLeftEdge / quadSize); //cast to int!

        float distanceToTopEdge = StartingPoint.z - zCoord;
        int zOffset = (int)(distanceToTopEdge / quadSize); //cast to int!


        int resultingIndex = ((AmountQuadsWidth * zOffset) + xOffset);



        return resultingIndex; // per quad we have 6 vertices - therefore we need to multiply the result with 6!
    }

    public int GetQuadIndexFromCoords(Vector3 position, float quadSize) //TODO: rename since it also changes the list!, TODO: exception
    {
        float farthestZCoord = StartingPoint.z >= FarRight.z ? StartingPoint.z : FarRight.z;
        float leftMostXCoord = StartingPoint.x <= NearLeft.x ? StartingPoint.x : NearLeft.x;
        if (position.x < leftMostXCoord || position.z > farthestZCoord) //if it is not within these, its definitely not in the plane of the stair
        {
            //return null; //in this case, the x and/or z Coord lies outside of the mesh. That actually shouldnt happen ..
            return -1; //TODO: take care of this! happens in osloer
        }

        //important first step: position needs to be put relative to starting point!!!

        Vector3 movedPosition = position - StartingPoint;

        Vector3 projectionWidth = Vector3.Project(movedPosition, WidthVector);
        Vector3 projectionLength = Vector3.Project(movedPosition, LengthVector);

        if (projectionWidth.magnitude > Width || projectionLength.magnitude > Length) //in case the pedestrian is outside of mesh
        {
            return -1;
        }

        int xOffset = (int)(projectionWidth.magnitude / quadSize); //cast to int!
        int zOffset = (int)(projectionLength.magnitude / quadSize); //cast to int!

        int resultingIndex = ((AmountQuadsWidth * zOffset) + xOffset);


        //float distanceToLeftEdgeOld = position.x - StartingPoint.x;

        //int xOffsetOld = (int)(distanceToLeftEdge / quadSize); //cast to int!

        //float distanceToTopEdgeOld = StartingPoint.z - position.z;
        //int zOffsetOld = (int)(distanceToTopEdge / quadSize); //cast to int!


        //int resultingIndexOld = ((AmountQuadsWidth * zOffset) + xOffset);



        return resultingIndex; // per quad we have 6 vertices - therefore we need to multiply the result with 6!
    }
    public int GetQuadIndexFromCoordsStairs(Vector3 position, float quadSize) //TODO: rename since it also changes the list!, TODO: exception
    {


        float farthestZCoord;
        farthestZCoord = StartingPoint.z >= FarRight.z ? StartingPoint.z : FarRight.z;
        if (position.x < StartingPoint.x || position.z > farthestZCoord) //if it is not within these, its definitely not in the plane of the stair
        {
            //return null; //in this case, the x and/or z Coord lies outside of the mesh. That actually shouldnt happen ..
            return -1; //TODO: take care of this! happens in osloer
        }


        Vector3 projectionWidth = Vector3.Project(position, WidthVector);
        Vector3 projectionLength = Vector3.Project(position, LengthVector);

        if(projectionWidth.magnitude > Width || projectionLength.magnitude > Length)
        {
            //we are outside of this mesh
        }

        return 0;
    }

}
