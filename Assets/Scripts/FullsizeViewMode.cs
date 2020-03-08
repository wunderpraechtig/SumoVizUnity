using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullsizeViewMode : MonoBehaviour
{
    [SerializeField] private Transform fullsizeAnchor = null;
    [SerializeField] private Transform simulationObjects = null;
    [SerializeField] private Transform playerController = null;
    private PlayerTransform lastTransform;
    
    void Awake()
    {
        lastTransform.position = Vector3.zero;
        lastTransform.rotation = playerController.rotation;
    }

    public void Enable()
    {
        Physics.IgnoreLayerCollision(9, 11, false);
        Physics.IgnoreLayerCollision(9, 12, false);
        Physics.IgnoreLayerCollision(9, 13, false);
        simulationObjects.SetParent(fullsizeAnchor, false);
        playerController.localPosition = lastTransform.position;
        playerController.localRotation = lastTransform.rotation;
    }

    public void SaveState()
    {
        
        lastTransform.position = playerController.localPosition;
        lastTransform.rotation = playerController.localRotation;
    }
}
