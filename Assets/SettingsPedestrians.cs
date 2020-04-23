using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPedestrians : MonoBehaviour
{

    [SerializeField] private Toggle radioModelVSimple = null;
    [SerializeField] private Toggle radioModelVComplex = null;
    [SerializeField] private PedestrianLoader pedestrianLoader = null;
    [SerializeField] private GameObject prefabVSimple = null;
    [SerializeField] private GameObject prefabVComplex = null;

    public void Setup()
    {
        LoadModel();
    }

    public enum ModelType
    {
        VerySimple = 0,
        VeryComplex = 1
    };

    private void LoadModel()
    {
        ModelType model = ModelType.VerySimple;
        if (PlayerPrefs.HasKey("pedestrianModel"))
            model = (ModelType)PlayerPrefs.GetInt("pedestrianModel");
        if (model == ModelType.VerySimple)
        {
            HandleRadioModelVSimple();
            radioModelVSimple.SetIsOnWithoutNotify(true);
        }
        if (model == ModelType.VeryComplex)
        {
            HandleRadioModelVComplex();
            radioModelVComplex.SetIsOnWithoutNotify(true);
        }

    }

    public void HandleRadioModelVSimple()
    {
        PlayerPrefs.SetInt("pedestrianModel", (int)ModelType.VerySimple);
        pedestrianLoader.pedestrianPrefab = prefabVSimple.name;
    }

    public void HandleRadioModelVComplex()
    {
        PlayerPrefs.SetInt("pedestrianModel", (int)ModelType.VeryComplex);
        pedestrianLoader.pedestrianPrefab = prefabVComplex.name;
    }

    
}
