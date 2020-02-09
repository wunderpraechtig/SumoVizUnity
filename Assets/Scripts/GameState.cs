using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public enum GameStateEvent
    {
        CurrentTimeChanged      = 0x0001,
        TotalTimeChanged        = 0x0002, 
        PlayPause               = 0x0004,
        HighlightSpeed          = 0x0008,
        HighlightDensity        = 0x0010,
        DensityChange           = 0x0020,
        ShowTrajectories        = 0x0040,
        FileLoaded              = 0x0080,
        Any                     = 0xFFFF
    }
    private Dictionary<GameStateObserver, GameStateEvent> observers = new Dictionary<GameStateObserver, GameStateEvent>();
    public void Subscribe(GameStateObserver observer, GameStateEvent eventMask)
    {
        if (observers.ContainsKey(observer))
        {
            observers[observer] = eventMask;
        }
        else
        {
            observers.Add(observer, eventMask);
        }
    }
    public void Unsubscribe(GameStateObserver observer)
    {
        if (observers.ContainsKey(observer))
        {
            observers.Remove(observer);
        }
    }
    private void NotifyObservers(GameStateEvent eventMask, GameStateObserver excludedObserver)
    {
        foreach (var entry in observers)
        {
            if (entry.Key != excludedObserver && (entry.Value & eventMask) != 0)
            {
                entry.Key.HandleEvent(eventMask);
            }
        }
    }

    private PlaybackControl playbackControl;

    private void Awake()
    {
        playbackControl = GameObject.Find("PlaybackControl").GetComponent<PlaybackControl>();
    }

    private void Start()
    {
        NotifyObservers(GameStateEvent.Any, null);
    }

    public bool GetIsPlaying() { return playbackControl.playing; }
    public void SetPlaying(bool value, GameStateObserver observer) {
        if (value != playbackControl.playing) {
            playbackControl.playing = value;
            NotifyObservers(GameStateEvent.PlayPause, observer);
        }
    }

    public decimal GetCurrentTime() { return playbackControl.current_time; }
    public void SetCurrentTime(decimal value, GameStateObserver observer)
    {
        if (value != playbackControl.current_time)
        {
            playbackControl.current_time = value;
            NotifyObservers(GameStateEvent.CurrentTimeChanged, observer);
        }
    }

    public decimal GetTotalTime() { return playbackControl.current_time; }
    public void SetTotalTime(decimal value, GameStateObserver observer)
    {
        if (value != playbackControl.total_time)
        {
            playbackControl.total_time = value;
            NotifyObservers(GameStateEvent.TotalTimeChanged, observer);
        }
    }

    public TileColoringMode GetPawnColoringMode() { return playbackControl.tileColoringMode; }
    public void SetPawnColoringMode(TileColoringMode value, GameStateObserver observer)
    {
        if (value != playbackControl.tileColoringMode)
        {
            playbackControl.tileColoringMode = value;
            NotifyObservers(GameStateEvent.HighlightDensity | GameStateEvent.HighlightSpeed, observer);
        }
    }

    public bool GetShowTrajectories() { return playbackControl.trajectoriesShown; }
    public void SetShowTrajectories(bool value, GameStateObserver observer)
    {
        if (value != playbackControl.trajectoriesShown)
        {
            playbackControl.trajectoriesShown = value;
            NotifyObservers(GameStateEvent.ShowTrajectories, observer);
        }
    }

    public float DensityThreshold() { return playbackControl.threshold; }
    public void SetDensityThreshold(float value, GameStateObserver observer)
    {
        if (value != playbackControl.threshold)
        {
            playbackControl.threshold = value;
            NotifyObservers(GameStateEvent.DensityChange, observer);
        }
    }
}
