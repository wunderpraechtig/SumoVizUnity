using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLogic : MonoBehaviour
{
    public Transform userPositionTransform;
    public float distanceFromUser = 2.0f;
    public GameObject InitialMainMenu;
    private SettingsArmMenu armMenu = null;

    private void Awake()
    {
        armMenu = gameObject.GetComponentInChildren<SettingsArmMenu>();
        if (armMenu) armMenu.Setup();
        gameObject.SetActive(false);
        ResetMenus();
    }

    public void ToggleMainMenu() {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            ResetMenus();
        }
        else {
            Quaternion rotation = userPositionTransform.rotation;
            rotation.x = 0;
            rotation.z = 0;
            Vector3 relativePosition = rotation * Vector3.forward * distanceFromUser;
            gameObject.transform.position = relativePosition + userPositionTransform.position;
            gameObject.transform.rotation = rotation;
            gameObject.SetActive(!gameObject.activeSelf);
        }
        
    }

    /// <summary>
    /// Disables all child objects
    /// </summary>
    private void ResetMenus() {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        InitialMainMenu.SetActive(true);
    }
}
