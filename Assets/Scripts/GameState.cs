using CustomEvents;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameState : MonoBehaviour
{
    [SerializeField] private bool                isPlaying = false;
    [SerializeField] private decimal             currentTime;
    [SerializeField] private decimal             totalTime;
    [SerializeField] private TileColoringMode    pawnColoringMode = TileColoringMode.TileColoringNone;
    [SerializeField] private bool                trajectoriesShown;
    [SerializeField] private float               densityThreshold;

    public event Action<bool>               isPlayingEvent;
    public event Action<decimal>            currentTimeEvent;
    public event Action<decimal>            totalTimeEvent;
    public event Action<TileColoringMode>   coloringModeEvent;
    public event Action<bool>               trajectoryModeEvent;
    public event Action<float>              densityThresholdEvent;

    

    private void Start()
    {
        isPlayingEvent.Invoke(isPlaying);
        currentTimeEvent.Invoke(currentTime);
        totalTimeEvent.Invoke(totalTime);
        coloringModeEvent.Invoke(pawnColoringMode);
        trajectoryModeEvent.Invoke(trajectoriesShown);
        densityThresholdEvent.Invoke(densityThreshold);

        Physics.IgnoreLayerCollision(10, 11);
        Physics.IgnoreLayerCollision(10, 12);
        Physics.IgnoreLayerCollision(10, 13);
    }

    public bool IsPlaying {
        get { return isPlaying; }
        set {
            if (value != isPlaying)
            {
                isPlaying = value;
                //isPlayingEvent.Invoke(value);
                isPlayingEvent.Invoke(value);
            }
        }
    }

    public decimal CurrentTime {
        get { return currentTime; }
        set {
            if (value != currentTime)
            {
                currentTime = value;
                currentTimeEvent.Invoke(value);
            }
        }
    }

    public decimal TotalTime {
        get { return totalTime; }
        set {
            if (value != totalTime)
            {
                totalTime = value;
                totalTimeEvent.Invoke(value);
            }
        }
    }

    public TileColoringMode PawnColoringMode {
        get { return pawnColoringMode; }
        set {
            if (value != pawnColoringMode)
            {
                pawnColoringMode = value;
                coloringModeEvent.Invoke(value);
            }
        }
    }

    public bool IsShowingTrajectories {
        get { return trajectoriesShown; }
        set {
            if (value != trajectoriesShown)
            {
                trajectoriesShown = value;
                trajectoryModeEvent.Invoke(value);
            }
        }
    }

    public float DensityThreshold {
        get { return densityThreshold; }
        set {
            if (value != densityThreshold)
            {
                densityThreshold = value;
                densityThresholdEvent.Invoke(value);
            }
        }
    }
}
