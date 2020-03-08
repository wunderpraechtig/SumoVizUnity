using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlaybackUIHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button buttonPlayInactive = null;
    [SerializeField] private Button buttonPlayActive = null;
    [SerializeField] private TextMeshProUGUI textTime = null;
    [SerializeField] private Slider sliderTime = null;
    [SerializeField] private Slider sliderDensityThreshold = null;
    [SerializeField] private TextMeshProUGUI textDescriptionDensityThreshold = null;
    [SerializeField] private TextMeshProUGUI textDensityThreshold = null;
    [SerializeField] private Toggle toggleHighlightSpeed = null;
    [SerializeField] private Toggle toggleHighlightDensity = null;
    [SerializeField] private Toggle toggleShowTrajectories = null;
    private System.TimeSpan timeSpan;
    const string format = @"mm\:ss\.ff";

    private GameState gameState;

    private void Awake()
    {
        gameState = FindObjectOfType<GameState>();
        EnableDensityThresholdFields(false);
        gameState.isPlayingEvent += OnIsPlayingChanged;
        gameState.trajectoryModeEvent += OnTrajectoryModeChanged;
        gameState.coloringModeEvent += OnPawnColoringModeChanged;
        gameState.densityThresholdEvent += OnDensityThresholdChanged;
        gameState.currentTimeEvent += OnCurrentTimeChanged;
        gameState.totalTimeEvent += OnTotalTimeChanged;
    }

    private void UpdateTimeSlider()
    {
        sliderTime.maxValue = gameState.TotalTime;
        sliderTime.SetValueWithoutNotify(gameState.CurrentTime);
        textTime.text = TimeStringFromSeconds(gameState.CurrentTime);
    }

    private void OnTotalTimeChanged(float value)
    {
        UpdateTimeSlider();
    }

    private void OnCurrentTimeChanged(float value)
    {
        UpdateTimeSlider();
    }

    private void OnDensityThresholdChanged(float value)
    {
        sliderDensityThreshold.SetValueWithoutNotify(value);
        string newDensityText = value.ToString("0.0") + "/m²";
        textDensityThreshold.text = newDensityText;
    }

    private void OnPawnColoringModeChanged(TileColoringMode value)
    {
        TileColoringMode coloringMode = value;
        bool newDensityState = false;
        bool newSpeedState = false;
        if (coloringMode == TileColoringMode.TileColoringDensity) newDensityState = true;
        else if (coloringMode == TileColoringMode.TileColoringSpeed) newSpeedState = true;
        toggleHighlightDensity.SetIsOnWithoutNotify(newDensityState);
        toggleHighlightSpeed.SetIsOnWithoutNotify(newSpeedState);
        EnableDensityThresholdFields(newDensityState);
    }

    private void OnTrajectoryModeChanged(bool value)
    {
        toggleShowTrajectories.SetIsOnWithoutNotify(value);
    }

    private void OnIsPlayingChanged(bool isPlaying)
    {
        buttonPlayActive.gameObject.SetActive(isPlaying);
        buttonPlayInactive.gameObject.SetActive(!isPlaying);
    }

    private void EnableDensityThresholdFields(bool value)
    {
        textDensityThreshold.gameObject.SetActive(value);
        textDescriptionDensityThreshold.gameObject.SetActive(value);
        sliderDensityThreshold.gameObject.SetActive(value);
    }
    
    public void HandlePlayPressed()
    {
        buttonPlayActive.gameObject.SetActive(!gameState.IsPlaying);
        buttonPlayInactive.gameObject.SetActive(gameState.IsPlaying);
        gameState.IsPlaying = !gameState.IsPlaying;
    }

    public void HandleSliderTime()
    {
        gameState.CurrentTime = (float)sliderTime.value;
        textTime.text = TimeStringFromSeconds((float)sliderTime.value);
    }

    public void HandleButtonSkipToStart()
    {
        sliderTime.SetValueWithoutNotify(0);
        gameState.CurrentTime = 0;
        textTime.text = TimeStringFromSeconds(0);
    }

    public void HandleToggleHighlightSpeed()
    {
        if (toggleHighlightSpeed.isOn)
        {
            toggleHighlightDensity.SetIsOnWithoutNotify(false);
            gameState.PawnColoringMode = TileColoringMode.TileColoringSpeed;
        }
        else
        {
            gameState.PawnColoringMode = TileColoringMode.TileColoringNone;
        }
    }

    public void HandleToggleHighlightDensity()
    {
        if (toggleHighlightDensity.isOn)
        {
            toggleHighlightSpeed.SetIsOnWithoutNotify(false);
            EnableDensityThresholdFields(true);
            gameState.PawnColoringMode = TileColoringMode.TileColoringDensity;
        }
        else
        {
            EnableDensityThresholdFields(false);
            gameState.PawnColoringMode = TileColoringMode.TileColoringNone;
        }
    }

    public void HandleToggleShowTrajectories()
    {
        gameState.IsShowingTrajectories = toggleShowTrajectories.isOn;
    }

    public void HandleSliderDensityThreshold()
    {
        textDensityThreshold.text = sliderDensityThreshold.value.ToString("0.0") + "/m²";
        gameState.DensityThreshold = sliderDensityThreshold.value;
    }

    private string TimeStringFromSeconds(float time)
    {
        timeSpan = System.TimeSpan.FromSeconds(time);
        return timeSpan.ToString(format);
    }
}
