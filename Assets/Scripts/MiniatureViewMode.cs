using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniatureViewMode : MonoBehaviour
{
    [SerializeField] private Transform miniatureNormalizer = null;
    [SerializeField] private Transform simulationObjects = null;
    [SerializeField] private Transform miniaturePlayerAnchor = null;

    [SerializeField] private Transform playerController = null;

    private PlayerTransform lastTransform;

    public void Awake() {
        lastTransform.position = miniaturePlayerAnchor.position;
        lastTransform.rotation = miniaturePlayerAnchor.rotation;
    }

    public void Enable()
    {
        Physics.IgnoreLayerCollision(9, 11);
        Physics.IgnoreLayerCollision(9, 12);
        Physics.IgnoreLayerCollision(9, 13);
        simulationObjects.SetParent(miniatureNormalizer, false);
        playerController.localPosition = lastTransform.position;
        playerController.localRotation = lastTransform.rotation;
    }

    public void SaveState()
    {
        lastTransform.position = playerController.localPosition;
        lastTransform.rotation = playerController.localRotation;
    }
}
