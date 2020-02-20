using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniatureViewMode : MonoBehaviour
{
    [SerializeField] private GameObject miniatureNormalizer = null;
    [SerializeField] private GameObject simulationObjects = null;
    [SerializeField] private Transform miniaturePlayerAnchor = null;

    private GameObject playerController;


    void Awake()
    {
        playerController = GameObject.Find("OVRPlayerController");
    }

    public void EnableMiniatureMode()
    {
        simulationObjects.transform.SetParent(miniatureNormalizer.transform, false);
        ResetPlayerPosition();
    }

    public void ResetPlayerPosition() {
        playerController.transform.localPosition = miniaturePlayerAnchor.position;
        playerController.transform.localRotation = miniaturePlayerAnchor.rotation;
        playerController.transform.localScale = miniaturePlayerAnchor.localScale;
    }


}
