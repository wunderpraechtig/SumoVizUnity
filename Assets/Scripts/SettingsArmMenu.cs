using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsArmMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArmPanelController playbackControl = null;
    [SerializeField] private ArmPanelController modeControl = null;
    [SerializeField] Transform handAnchorFL = null;
    [SerializeField] Transform handAnchorFR = null;
    [SerializeField] Transform handAnchorBL = null;
    [SerializeField] Transform handAnchorBR = null;

    [SerializeField] private Toggle togglePlaybackEnabled = null;
    [SerializeField] private Toggle radioPlaybackFL = null;
    [SerializeField] private Toggle radioPlaybackFR = null;
    [SerializeField] private Toggle radioPlaybackBL = null;
    [SerializeField] private Toggle radioPlaybackBR = null;

    [SerializeField] private Toggle toggleModeEnabled = null;
    [SerializeField] private Toggle radioModeFL = null;
    [SerializeField] private Toggle radioModeFR = null;
    [SerializeField] private Toggle radioModeBL = null;
    [SerializeField] private Toggle radioModeBR = null;

    [SerializeField] private TextMeshProUGUI textAngleHandToCam = null;
    [SerializeField] private TextMeshProUGUI textAngleCamToHand = null;
    [SerializeField] private TextMeshProUGUI textAngleZRotMax = null;
    [SerializeField] private Slider sliderCamToHand = null;
    [SerializeField] private Slider sliderHandToCam = null;
    [SerializeField] private Slider sliderZRotMax = null;
    

    public enum ArmPanelLocation {
        FrontLeft = 0,
        FrontRight = 1,
        BackLeft = 2,
        BackRight = 3
    };

    /// <summary>
    /// Loads the related playersettings from PlayerPrefs onto the UI and applies them to affected objects.
    /// </summary>
    public void Setup() {
        LoadPlaybackEnabled();

        LoadPlaybackLocation();

        LoadModeEnabled();

        LoadModeLocation();

        LoadAngleCamToHand();

        LoadAngleHandToCam();

        LoadAngleZRotMax();
    }

    private void LoadPlaybackEnabled()
    {
        if (PlayerPrefs.HasKey("playbackEnabled"))
            togglePlaybackEnabled.SetIsOnWithoutNotify(Convert.ToBoolean(PlayerPrefs.GetInt("playbackEnabled")));
        else
        {
            togglePlaybackEnabled.SetIsOnWithoutNotify(playbackControl.enabled);
            PlayerPrefs.SetInt("playbackEnabled", Convert.ToInt32(playbackControl.enabled));
        }
        playbackControl.gameObject.SetActive(togglePlaybackEnabled.isOn);
    }

    private void LoadModeEnabled()
    {
        if (PlayerPrefs.HasKey("modeEnabled"))
            toggleModeEnabled.SetIsOnWithoutNotify(Convert.ToBoolean(PlayerPrefs.GetInt("modeEnabled")));
        else
        {
            toggleModeEnabled.SetIsOnWithoutNotify(modeControl.enabled);
            PlayerPrefs.SetInt("modeEnabled", Convert.ToInt32(modeControl.enabled));
        }
        modeControl.gameObject.SetActive(toggleModeEnabled.isOn);
    }

    private void LoadPlaybackLocation()
    {
        ArmPanelLocation loc = ArmPanelLocation.BackLeft;
        if (PlayerPrefs.HasKey("playbackLocation"))
            loc = (ArmPanelLocation)PlayerPrefs.GetInt("playbackLocation");
        if (loc == ArmPanelLocation.FrontLeft)
        { 
            HandlePlaybackLocationRadioFL();
            radioPlaybackFL.SetIsOnWithoutNotify(true);
        }
        if (loc == ArmPanelLocation.FrontRight)
        {
            HandlePlaybackLocationRadioFR();
            radioPlaybackFR.SetIsOnWithoutNotify(true);
        }
        if (loc == ArmPanelLocation.BackLeft)
        {
            HandlePlaybackLocationRadioBL();
            radioPlaybackBL.SetIsOnWithoutNotify(true);
        }
        if (loc == ArmPanelLocation.BackRight)
        {
            HandlePlaybackLocationRadioBR();
            radioPlaybackBR.SetIsOnWithoutNotify(true);
        }
            
    }

    private void LoadModeLocation()
    {
        ArmPanelLocation loc = ArmPanelLocation.BackRight;
        if (PlayerPrefs.HasKey("modeLocation"))
            loc = (ArmPanelLocation)PlayerPrefs.GetInt("modeLocation");
        if (loc == ArmPanelLocation.FrontLeft)
        {
            HandleModeLocationRadioFL();
            radioModeFL.SetIsOnWithoutNotify(true);
        }
        if (loc == ArmPanelLocation.FrontRight)
        {
            HandleModeLocationRadioFR();
            radioModeFR.SetIsOnWithoutNotify(true);
        }
        if (loc == ArmPanelLocation.BackLeft)
        {
            HandleModeLocationRadioBL();
            radioModeBL.SetIsOnWithoutNotify(true);
        }
        if (loc == ArmPanelLocation.BackRight)
        {
            HandleModeLocationRadioBR();
            radioModeBR.SetIsOnWithoutNotify(true);
        }

    }

    private void LoadAngleCamToHand()
    {
        if (PlayerPrefs.HasKey("armAngleCamToHand"))
            SetAngleCamToHand(PlayerPrefs.GetFloat("armAngleCamToHand"));
        else
        {
            SetAngleCamToHand(playbackControl.maxAngleCamToDisplay);
            sliderCamToHand.SetValueWithoutNotify(playbackControl.maxAngleCamToDisplay);
        }
    }

    private void LoadAngleHandToCam()
    {
        if (PlayerPrefs.HasKey("armAngleHandToCam"))
            SetAngleHandToCam(PlayerPrefs.GetFloat("armAngleHandToCam"));
        else
        {
            SetAngleHandToCam(playbackControl.maxAngleDisplayToCam);
            sliderHandToCam.SetValueWithoutNotify(playbackControl.maxAngleDisplayToCam);
        }
    }

    private void LoadAngleZRotMax()
    {
        if (PlayerPrefs.HasKey("armAngleZRotMax"))
            SetAngleZRotMax(PlayerPrefs.GetFloat("armAngleZRotMax"));
        else
        {
            SetAngleZRotMax(playbackControl.maxZRotationDifference);
            sliderZRotMax.SetValueWithoutNotify(playbackControl.maxZRotationDifference);
        }
    }

    public void HandlePlaybackToggle() {
        PlayerPrefs.SetInt("playbackEnabled", Convert.ToInt32(togglePlaybackEnabled.isOn));
        playbackControl.gameObject.SetActive(togglePlaybackEnabled.isOn);
    }

    public void HandleModeToggle()
    {
        PlayerPrefs.SetInt("modeEnabled", Convert.ToInt32(toggleModeEnabled.isOn));
        modeControl.gameObject.SetActive(toggleModeEnabled.isOn);
    }

    public void HandlePlaybackLocationRadioFL()
    {
        changePlaybackLocation(ArmPanelLocation.FrontLeft, handAnchorFL);
    }

    public void HandlePlaybackLocationRadioFR()
    {
        changePlaybackLocation(ArmPanelLocation.FrontRight, handAnchorFR);
    }

    public void HandlePlaybackLocationRadioBL()
    {
        changePlaybackLocation(ArmPanelLocation.BackLeft, handAnchorBL);
    }

    public void HandlePlaybackLocationRadioBR()
    {
        changePlaybackLocation(ArmPanelLocation.BackRight, handAnchorBR);
    }

    public void HandleModeLocationRadioFL()
    {
        changeModeLocation(ArmPanelLocation.FrontLeft, handAnchorFL);
    }

    public void HandleModeLocationRadioFR()
    {
        changeModeLocation(ArmPanelLocation.FrontRight, handAnchorFR);
    }

    public void HandleModeLocationRadioBL()
    {
        changeModeLocation(ArmPanelLocation.BackLeft, handAnchorBL);
    }

    public void HandleModeLocationRadioBR()
    {
        changeModeLocation(ArmPanelLocation.BackRight, handAnchorBR);
    }

    private void changePlaybackLocation(ArmPanelLocation loc, Transform anchor) {
        PlayerPrefs.SetInt("playbackLocation", (int)loc);
        playbackControl.handAnchorTransform = anchor;
    }

    private void changeModeLocation(ArmPanelLocation loc, Transform anchor)
    {
        PlayerPrefs.SetInt("modeLocation", (int)loc);
        modeControl.handAnchorTransform = anchor;
    }

    private string ConvertAngleToString(float value)
    {
        return value.ToString("0.0") + "°";
    }

    private void SetAngleCamToHand(float value)
    {
        PlayerPrefs.SetFloat("armAngleCamToHand", value);
        textAngleCamToHand.text = ConvertAngleToString(value);
        if (sliderCamToHand.value != value)
            sliderCamToHand.SetValueWithoutNotify(value);
        playbackControl.maxAngleCamToDisplay = value;
        modeControl.maxAngleCamToDisplay = value;
    }

    public void HandleAngleCamToHand()
    {
        SetAngleCamToHand(sliderCamToHand.value);
    }

    private void SetAngleHandToCam(float value)
    {
        PlayerPrefs.SetFloat("armAngleHandToCam", value);
        textAngleHandToCam.text = ConvertAngleToString(value);
        if (sliderHandToCam.value != value)
            sliderHandToCam.SetValueWithoutNotify(value);
        playbackControl.maxAngleDisplayToCam = value;
        modeControl.maxAngleDisplayToCam = value;
    }

    public void HandleAngleHandToCam()
    {
        SetAngleHandToCam(sliderHandToCam.value);
    }

    private void SetAngleZRotMax(float value)
    {
        PlayerPrefs.SetFloat("armAngleZRotMax", value);
        textAngleZRotMax.text = ConvertAngleToString(value);
        if (sliderZRotMax.value != value)
            sliderZRotMax.SetValueWithoutNotify(value);
        playbackControl.maxZRotationDifference = value;
        modeControl.maxZRotationDifference = value;
    }

    public void HandleAngleZRotMax()
    {
        SetAngleZRotMax(sliderZRotMax.value);
    }
}
