using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianAssembler
{
    Dictionary<int, List<PedestrianPosition>> pedestrianPositions = new Dictionary<int, List<PedestrianPosition>>();
    public float internalTotalTime = 0.0f;

    public void addPedestrianPosition(PedestrianPosition position)
    {
        List<PedestrianPosition> currentPosList;
        if (pedestrianPositions.TryGetValue(position.getID(), out currentPosList))
        {
            currentPosList.Add(position);
        }
        else
        {
            currentPosList = new List<PedestrianPosition>();
            currentPosList.Add(position);
            pedestrianPositions.Add(position.getID(), currentPosList);
        }
        if (position.getTime() > internalTotalTime) internalTotalTime = position.getTime();
    }

    public List<PedestrianEntity> createPedestrians()
    {
        List<PedestrianEntity> entityList = new List<PedestrianEntity>(pedestrianPositions.Count);
        foreach (var pedestrianEntry in pedestrianPositions)
        {
            PedestrianEntity entity = new PedestrianEntity(pedestrianEntry.Key, pedestrianEntry.Value);
            entityList.Add(entity);
        }
        return entityList;
    }
}
