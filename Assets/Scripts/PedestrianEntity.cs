using System.Collections.Generic;
using UnityEngine;

public struct PedestrianEntity
{
    public int id;
    public List<PedestrianPosition> positions;
    public int lastTimeIndex;
    public float timeEarliest;
    public float timeLatest;
    public PedestrianPoolObject poolObject;

    public PedestrianEntity(int _id, List<PedestrianPosition> _positions) {
        id = _id;
        positions = _positions;
        lastTimeIndex = 0;
        timeEarliest = positions[0].getTime();
        timeLatest = positions[positions.Count - 1].getTime();
        poolObject = null;
    }

    public int getIndexOfNewTime(float time)
    {
        if (positions.Count < 1) return -1;
        float lastTime = positions[lastTimeIndex].getTime();
        if (time < timeEarliest) return -1;
        if (time > timeLatest) return -1;
        if (lastTime < time)
        {
            for (int i = lastTimeIndex; i < positions.Count; ++i)
            {
                if (positions[i].getTime() > time)
                {
                    lastTimeIndex = i - 1;
                    return i - 1;
                }
            }
        }
        else
        {
            for (int i = lastTimeIndex; i >= 0; --i)
            {
                if (positions[i].getTime() <= time)
                {
                    lastTimeIndex = i;
                    return i;
                }
            }
        }
        return -1;
    }
}
