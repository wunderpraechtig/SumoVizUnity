using UnityEngine;
using System;
using System.Collections;

using System.Collections.Generic;
using System.IO;
using Vectrosity;

public class PlaybackControl : MonoBehaviour
{

    private GameState gameState;
    private PedestrianLoader pl;
    GeometryLoader gl;
    public int tiles = 0;
    public bool drawLine;

    // Getters and setters for all previously public attributes that now get managed by the GameState
    public bool playing {
        get { return gameState.IsPlaying; }
        set { gameState.IsPlaying = value; }
    }
    public decimal current_time {
        get { return gameState.CurrentTime; }
        set { gameState.CurrentTime = value; }
    }
    public decimal total_time {
        get { return gameState.TotalTime; }
        set { gameState.TotalTime = value; }
    }
    public TileColoringMode tileColoringMode {
        get { return gameState.PawnColoringMode; }
        set { gameState.PawnColoringMode = value; }
    }
    public bool trajectoriesShown {
        get { return gameState.IsShowingTrajectories; }
        set { gameState.IsShowingTrajectories = value; }
    }
    public float threshold {
        get { return gameState.DensityThreshold; }
        set { gameState.DensityThreshold = value; }
    }

    public struct Label
    {
        public Rect rect;
        public string label;

        public Label(Rect r, string s)
        {
            rect = r;
            label = s;
        }
    }

    void Awake()
    {
        pl = GameObject.Find("PedestrianLoader").GetComponent<PedestrianLoader>();
        gl = GameObject.Find("GeometryLoader").GetComponent<GeometryLoader>();
        gameState = FindObjectOfType<GameState>();
        gameState.trajectoryModeEvent += OnTrajectoryModeChanged;
    }

    private void OnTrajectoryModeChanged(bool isShowingTrajectories)
    {
        if (isShowingTrajectories)
        {
            foreach (GameObject p in pl.pedestirans)
            {
                p.GetComponent<Pedestrian>().showTrajectory();
            }
        }
        else
        {
            foreach (GameObject p in pl.pedestirans)
            {
                p.GetComponent<Pedestrian>().hideTrajectory();
            }
        }
    }

    /* OnGUI
    void OnGUI()
    {
        bool newPlaying = GUI.Toggle(new Rect(30, 25, 100, 30), playing, " PLAY");
        if (newPlaying != playing)
            playing = newPlaying;

        decimal new_current_time = (decimal)GUI.HorizontalSlider(new Rect(100, 30, 400, 30), (float)current_time, 0.0f, (float)total_time);
        if (new_current_time != current_time)
            current_time = new_current_time;

        //string btnText = "show trajectories";
        //if (trajectoriesShown) btnText = "hide trajectories";
        //if (GUI.Button(new Rect(510, 20, 120, 30), btnText))
        //{
        //}

        string btnText = "add line";
        if (drawLine) GUI.color = Color.red;
        if (lineIsDrawn) btnText = "remove line";

        if (GUI.Button(new Rect(640, 20, 80, 30), btnText))
        {
            if (lineIsDrawn)
            {
                gl.groundplane.removeLine();
                drawLine = false;
                lineIsDrawn = false;
            }
            else
            {
                drawLine = !drawLine;
                if (!drawLine)
                {
                    gl.groundplane.removeLine();
                }
            }
        }
        GUI.color = Color.white;

        //if (tiles == 0) btnText = "colors by speed";
        //if (tiles == 1) btnText = "colors by density";
        //if (tiles == 2) btnText = "hide colors";

        //if (GUI.Button(new Rect(730, 20, 120, 30), btnText))
        //{
        //    tiles = (tiles + 1) % 3;

        //    InfoText it = GameObject.Find("InfoText").GetComponent<InfoText>();
        //    if (it.diagram) it.removeDiagram();

        //    if (tiles == 0) tileColoringMode = TileColoringMode.TileColoringNone;
        //    if (tiles == 1) tileColoringMode = TileColoringMode.TileColoringSpeed;
        //    if (tiles == 2) tileColoringMode = TileColoringMode.TileColoringDensity;
        //}

        //if (tileColoringMode == TileColoringMode.TileColoringDensity)
        //{
        //    threshold = GUI.HorizontalSlider(new Rect(730, 55, 120, 30), threshold, 0.0f, 6.0f);
        //    GUI.Label(new Rect(730, 70, 120, 30), "Threshold: " + System.Math.Round(threshold, 2) + "/m²");
        //}
        

    }*/

    public void lineDrawn()
    {
        drawLine = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (playing)
        {
            try
            {
                current_time = (current_time + (decimal)Time.deltaTime) % total_time;
            }
            catch (DivideByZeroException)
            {
                current_time = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playing = !playing;
        }

    }
}
