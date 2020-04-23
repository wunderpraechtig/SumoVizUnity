using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsTeleportation : MonoBehaviour
{
    [SerializeField] private RayModeSwitcher rmsLeft = null;
    [SerializeField] private RayModeSwitcher rmsRight = null;

    [SerializeField] private Toggle radioControllerLeft = null;
    [SerializeField] private Toggle radioControllerRight = null;
    [SerializeField] private Toggle radioControllerBoth = null;

    public enum Controller
    {
        Left = 0,
        Right = 1,
        Both = 2
    };

    public void Setup()
    {
        LoadControllerSetup();
    }

    private void LoadControllerSetup()
    {
        Controller model = Controller.Both;
        if (PlayerPrefs.HasKey("teleportationControllerChoice"))
            model = (Controller)PlayerPrefs.GetInt("teleportationControllerChoice");
        if (model == Controller.Left)
        {
            HandleRadioControllerLeft();
            radioControllerLeft.SetIsOnWithoutNotify(true);
        }
        if (model == Controller.Right)
        {
            HandleRadioControllerRight();
            radioControllerRight.SetIsOnWithoutNotify(true);
        }
        if (model == Controller.Both)
        {
            HandleRadioControllerBoth();
            radioControllerBoth.SetIsOnWithoutNotify(true);
        }
    }

    public void HandleRadioControllerBoth()
    {
        PlayerPrefs.SetInt("teleportationControllerChoice", (int)Controller.Both);
        rmsLeft.enableTeleport = true;
        rmsRight.enableTeleport = true;
    }

    public void HandleRadioControllerRight()
    {
        PlayerPrefs.SetInt("teleportationControllerChoice", (int)Controller.Right);
        rmsLeft.enableTeleport = false;
        rmsRight.enableTeleport = true;
    }

    public void HandleRadioControllerLeft()
    {
        PlayerPrefs.SetInt("teleportationControllerChoice", (int)Controller.Left);
        rmsLeft.enableTeleport = true;
        rmsRight.enableTeleport = false;
    }
}
