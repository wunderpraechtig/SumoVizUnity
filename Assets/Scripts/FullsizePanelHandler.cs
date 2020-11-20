using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullsizePanelHandler : MonoBehaviour
{
    [SerializeField] private Transform playerRig = null;
    [SerializeField] private Transform fullsizePlayerAnchor = null;

    public void HandleRecenterPlayer()
    {
        playerRig.position = fullsizePlayerAnchor.position;
        playerRig.rotation = fullsizePlayerAnchor.rotation;
    }
}
