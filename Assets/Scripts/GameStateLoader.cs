using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateLoader : MonoBehaviour
{

    [SerializeField] private SettingsArmMenu settingsArmMenu = null;
    
    void Awake()
    {
        settingsArmMenu.Setup();
    }
}
