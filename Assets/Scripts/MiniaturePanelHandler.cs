using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniaturePanelHandler : MonoBehaviour
{
    [SerializeField] private MiniatureViewControl miniatureViewControl = null;
    [SerializeField] private GameObject pedestalCylinder = null;
    [SerializeField] private Transform miniaturePlayerAnchor = null;
    [SerializeField] private Transform miniaturePedestal = null;
    [SerializeField] private Transform miniatureScaleRotate = null;
    [SerializeField] private Transform miniatureMove = null;
    [SerializeField] private Transform playerRig = null;
    [SerializeField] private Toggle toggleLockedHeight = null;
    [SerializeField] private Toggle togglePedestalVisible = null;
    [SerializeField] private Toggle toggleOnPedestal = null;
    [SerializeField] private Button buttonResetPedestal = null;
    private Vector3 pedestalDefaultPos;
    private Vector3 pedestalPreviousPos;
    private bool pedestalHeightLock;
    private bool pedestalVisible;

    private void Awake()
    {
        pedestalDefaultPos = miniaturePedestal.localPosition;
    }

    public void HandlePedestalVisible() {
        pedestalCylinder.SetActive(togglePedestalVisible.isOn);
    }

    public void HandleLockedHeight() {
        miniatureViewControl.IsHeightLocked = toggleLockedHeight.isOn;
    }

    public void HandleResetBuilding() {
        miniatureScaleRotate.localScale = Vector3.one;
        miniatureScaleRotate.localRotation = Quaternion.identity;
        miniatureMove.localPosition = Vector3.zero;
    }

    public void HandleResetPedestal() {
        togglePedestalVisible.isOn = true;
        miniaturePedestal.localPosition = pedestalDefaultPos;
    }

    public void HandleRecenterPlayer() {
        playerRig.position = miniaturePlayerAnchor.position;
        playerRig.rotation = miniaturePlayerAnchor.rotation;
    }

    public void HandleOnPedestal() {
        if (!toggleOnPedestal.isOn) // turning pedestal mode off
        {
            buttonResetPedestal.interactable = false;
            pedestalHeightLock = toggleLockedHeight.isOn;
            pedestalVisible = togglePedestalVisible.isOn;
            togglePedestalVisible.isOn = false;
            toggleLockedHeight.isOn = true;
            togglePedestalVisible.interactable = false;
            toggleLockedHeight.interactable = false;
            pedestalPreviousPos = miniaturePedestal.localPosition;
            miniaturePedestal.localPosition = Vector3.zero;
        }
        else // turning pedestal mode on
        {
            buttonResetPedestal.interactable = true;
            togglePedestalVisible.interactable = true;
            toggleLockedHeight.interactable = true;
            togglePedestalVisible.isOn = pedestalVisible;
            toggleLockedHeight.isOn = pedestalHeightLock;
            miniaturePedestal.localPosition = pedestalPreviousPos;
        }
    }
}
