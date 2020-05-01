using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateLoader : MonoBehaviour
{

    [SerializeField] private SettingsArmMenu settingsArmMenu = null;
    [SerializeField] private SettingsTeleportation settingsTeleportation = null;

    void Awake()
    {
        settingsArmMenu.Setup();
        settingsTeleportation.Setup();
    }
}
