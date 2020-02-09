using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlaybackUIHandler : MonoBehaviour, GameStateObserver
{
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
    private GameState gameState;
    private TwinHandedInputSelector handedInputSelector;

    private void Awake()
    {
        gameState = GameObject.Find("GameState").GetComponent<GameState>();
        EnableDensityThresholdFields(false);
        gameState.Subscribe(this,
            GameState.GameStateEvent.CurrentTimeChanged |
            GameState.GameStateEvent.TotalTimeChanged |
            GameState.GameStateEvent.PlayPause |
            GameState.GameStateEvent.DensityChange |
            GameState.GameStateEvent.ShowTrajectories |
            GameState.GameStateEvent.HighlightDensity |
            GameState.GameStateEvent.HighlightSpeed
            );
    }

    private void EnableDensityThresholdFields(bool value)
    {
        textDensityThreshold.gameObject.SetActive(value);
        textDescriptionDensityThreshold.gameObject.SetActive(value);
        sliderDensityThreshold.gameObject.SetActive(value);
    }

    public void HandleEvent(GameState.GameStateEvent theEvent)
    {
        if ((theEvent & GameState.GameStateEvent.PlayPause) != 0)
        {
            buttonPlayActive.gameObject.SetActive(gameState.GetIsPlaying());
            buttonPlayInactive.gameObject.SetActive(!gameState.GetIsPlaying());
        }
        if ((theEvent & GameState.GameStateEvent.ShowTrajectories) != 0)
        {
            toggleShowTrajectories.GetComponent<Toggle>().isOn = gameState.GetShowTrajectories();
        }
        if ((theEvent & (GameState.GameStateEvent.HighlightDensity | GameState.GameStateEvent.HighlightSpeed)) != 0)
        {
            TileColoringMode coloringMode = gameState.GetPawnColoringMode();
            bool newDensityState = false;
            bool newSpeedState = false;
            if (coloringMode == TileColoringMode.TileColoringDensity) newDensityState = true;
            else if (coloringMode == TileColoringMode.TileColoringSpeed) newSpeedState = true;
            toggleHighlightDensity.GetComponent<Toggle>().isOn = newDensityState;
            toggleHighlightSpeed.GetComponent<Toggle>().isOn = newSpeedState;
            EnableDensityThresholdFields(newDensityState);
        }
        if ((theEvent & GameState.GameStateEvent.DensityChange) != 0)
        {
            Slider densitySlider = sliderDensityThreshold.GetComponent<Slider>();
            densitySlider.value = gameState.DensityThreshold();
            string newDensityText = gameState.DensityThreshold().ToString("0.0") + "/m²";
            textDensityThreshold.GetComponent<TextMeshProUGUI>().text = newDensityText;
        }
        if ((theEvent & (GameState.GameStateEvent.CurrentTimeChanged | GameState.GameStateEvent.TotalTimeChanged)) != 0)
        {
            Slider timeSlider = sliderTime.GetComponent<Slider>();
            timeSlider.maxValue = (float)gameState.GetTotalTime();
            timeSlider.value = (float)gameState.GetCurrentTime();
            string newTimeText = gameState.GetCurrentTime().ToString();
            textTime.GetComponent<TextMeshProUGUI>().text = newTimeText;
        }
        Debug.Log("Debug::ArmControlHandler::HandleEvent: 0b" + System.Convert.ToString((int)theEvent, 2));
    }

    public void HandlePlayPressed()
    {
        bool isPlaying = gameState.GetIsPlaying();
        buttonPlayActive.gameObject.SetActive(!isPlaying);
        buttonPlayInactive.gameObject.SetActive(isPlaying);
        gameState.SetPlaying(!isPlaying, this);
    }

    public void HandleSliderTime()
    {
        float sliderValue = sliderTime.GetComponent<Slider>().value;
        gameState.SetCurrentTime((decimal)sliderValue, this);
        string newTimeText = sliderValue.ToString();
        textTime.GetComponent<TextMeshProUGUI>().text = newTimeText;
    }

    public void HandleButtonSkipToStart()
    {
        Slider timeSlider = sliderTime.GetComponent<Slider>();
        timeSlider.value = 0;
        gameState.SetCurrentTime(0, this);
        string newTimeText = timeSlider.value.ToString();
        textTime.GetComponent<TextMeshProUGUI>().text = newTimeText;
    }

    public void HandleToggleHighlightSpeed()
    {
        if (toggleHighlightSpeed.GetComponent<Toggle>().isOn)
        {
            toggleHighlightDensity.GetComponent<Toggle>().isOn = false;
            gameState.SetPawnColoringMode(TileColoringMode.TileColoringSpeed, this);
        }
        else
        {
            gameState.SetPawnColoringMode(TileColoringMode.TileColoringNone, this);
        }
    }

    public void HandleToggleHighlightDensity()
    {
        if (toggleHighlightDensity.GetComponent<Toggle>().isOn)
        {
            toggleHighlightSpeed.GetComponent<Toggle>().isOn = false;
            EnableDensityThresholdFields(true);
            gameState.SetPawnColoringMode(TileColoringMode.TileColoringDensity, this);
        }
        else
        {
            EnableDensityThresholdFields(false);
            gameState.SetPawnColoringMode(TileColoringMode.TileColoringNone, this);
        }
    }

    public void HandleToggleShowTrajectories()
    {
        gameState.SetShowTrajectories(toggleShowTrajectories.GetComponent<Toggle>().isOn, this);
    }

    public void HandleSliderDensityThreshold()
    {
        float sliderValue = sliderDensityThreshold.GetComponent<Slider>().value;
        string newText = sliderValue.ToString("0.0") + "/m²";
        textDensityThreshold.GetComponent<TextMeshProUGUI>().text = newText;
        gameState.SetDensityThreshold(sliderValue, this);
    }
}
