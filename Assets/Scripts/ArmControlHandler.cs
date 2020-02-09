using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArmControlHandler : MonoBehaviour, GameStateObserver
{
    [SerializeField] private GameObject buttonPlayInactive = null;
    [SerializeField] private GameObject buttonPlayActive = null;
    [SerializeField] private GameObject textTime = null;
    [SerializeField] private GameObject sliderTime = null;
    [SerializeField] private GameObject sliderDensityThreshold = null;
    [SerializeField] private GameObject textDescriptionDensityThreshold = null;
    [SerializeField] private GameObject textDensityThreshold = null;
    [SerializeField] private GameObject radioHighlightSpeed = null;
    [SerializeField] private GameObject radioHighlightDensity = null;
    [SerializeField] private GameObject checkboxShowTrajectories = null;
    [SerializeField] private GameObject mainPanel = null;
    private OVRCameraRig cameraRig;
    private TwinHandedInputSelector handedInputSelector;
    [SerializeField] private float cameraToDisplayAngle = 30.0f;
    [SerializeField] private float displayToCameraAngle = 30.0f;
    public OVRInput.Controller requiredController = OVRInput.Controller.None;
    private GameState gameState;

    public float angleDisplayToCamera = 0.0f;
    public float angleCameraToDisplay = 0.0f;


    public Transform HandAnchorTransform;
    private Vector3 initialScale;

    private void Awake()
    {
        gameState = GameObject.Find("GameState").GetComponent<GameState>();
        cameraRig = FindObjectOfType<OVRCameraRig>();
        handedInputSelector = FindObjectOfType<TwinHandedInputSelector>();
        mainPanel.SetActive(false);
        EnableDensityThresholdFields(false);
        initialScale = transform.localScale;
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

    private void Update()
    {
        Vector3 displayToCameraHeading = (cameraRig.centerEyeAnchor.transform.position - transform.position).normalized;
        Vector3 displaySightVector = -transform.forward;
        angleDisplayToCamera = Vector3.Dot(displaySightVector, displayToCameraHeading);

        Vector3 cameraToDisplayHeading = -displayToCameraHeading;
        Vector3 cameraSightVector = cameraRig.centerEyeAnchor.forward;
        angleCameraToDisplay = Vector3.Dot(cameraSightVector, cameraToDisplayHeading);

        if ((1.0f - angleCameraToDisplay) * 360.0f <= cameraToDisplayAngle && (1.0f - angleDisplayToCamera) * 360.0f <= displayToCameraAngle)
        {
            mainPanel.SetActive(true);
            if (requiredController != OVRInput.Controller.None)
                handedInputSelector.SetAndLockActiveController(requiredController);
        }
        else
        {
            mainPanel.SetActive(false);
            handedInputSelector.UnlockActiveController();
        }
    }

    private void LateUpdate()
    {
        transform.position = HandAnchorTransform.position;
        transform.rotation = HandAnchorTransform.rotation;
        transform.localScale = Vector3.Scale(initialScale, HandAnchorTransform.lossyScale);
    }

    public void HandleEvent(GameState.GameStateEvent theEvent) {
        if ((theEvent & GameState.GameStateEvent.PlayPause) != 0) {
            buttonPlayActive.SetActive(gameState.GetIsPlaying());
            buttonPlayInactive.SetActive(!gameState.GetIsPlaying());
        }
        if ((theEvent & GameState.GameStateEvent.ShowTrajectories) != 0) {
            checkboxShowTrajectories.GetComponent<Toggle>().isOn = gameState.GetShowTrajectories();
        }
        if ((theEvent & (GameState.GameStateEvent.HighlightDensity | GameState.GameStateEvent.HighlightSpeed)) != 0)
        {
            TileColoringMode coloringMode = gameState.GetPawnColoringMode();
            bool newDensityState = false;
            bool newSpeedState = false;
            if (coloringMode == TileColoringMode.TileColoringDensity) newDensityState = true;
            else if (coloringMode == TileColoringMode.TileColoringSpeed) newSpeedState = true;
            radioHighlightDensity.GetComponent<Toggle>().isOn = newDensityState;
            radioHighlightSpeed.GetComponent<Toggle>().isOn = newSpeedState;
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
        buttonPlayActive.SetActive(!isPlaying);
        buttonPlayInactive.SetActive(isPlaying);
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

    public void HandleRadioHighlightSpeed()
    {
        if (radioHighlightSpeed.GetComponent<Toggle>().isOn)
        {
            radioHighlightDensity.GetComponent<Toggle>().isOn = false;
            gameState.SetPawnColoringMode(TileColoringMode.TileColoringSpeed, this);
        }
        else {
            gameState.SetPawnColoringMode(TileColoringMode.TileColoringNone, this);
        }
    }

    public void HandleRadioHighlightDensity()
    {
        if (radioHighlightDensity.GetComponent<Toggle>().isOn)
        {
            radioHighlightSpeed.GetComponent<Toggle>().isOn = false;
            EnableDensityThresholdFields(true);
            gameState.SetPawnColoringMode(TileColoringMode.TileColoringDensity, this);
        }
        else {
            EnableDensityThresholdFields(false);
            gameState.SetPawnColoringMode(TileColoringMode.TileColoringNone, this);
        }
    }

    public void HandleCheckboxShowTrajectories()
    {
        gameState.SetShowTrajectories(checkboxShowTrajectories.GetComponent<Toggle>().isOn, this);
    }

    public void HandleSliderDensityThreshold()
    {
        float sliderValue = sliderDensityThreshold.GetComponent<Slider>().value;
        string newText = sliderValue.ToString("0.0") + "/m²";
        textDensityThreshold.GetComponent<TextMeshProUGUI>().text = newText;
        gameState.SetDensityThreshold(sliderValue, this);
    }

    private void EnableDensityThresholdFields(bool value)
    {
        textDensityThreshold.SetActive(value);
        textDescriptionDensityThreshold.SetActive(value);
        sliderDensityThreshold.SetActive(value);
    }
}
