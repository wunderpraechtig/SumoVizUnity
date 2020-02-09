using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TwinHandedInputSelector : MonoBehaviour
{
    private OVRCameraRig cameraRig;
    private OVRInputModule inputModule;
    private bool lockedController = false;

    public OVRInput.Button controllerUiButtonsL = OVRInput.Button.Three;
    public OVRInput.Button controllerUiButtonsR = OVRInput.Button.One;

    void Start()
    {
        cameraRig = FindObjectOfType<OVRCameraRig>();
        inputModule = FindObjectOfType<OVRInputModule>();

        if (OVRInput.GetActiveController() == OVRInput.Controller.LTouch)
        {
            SetActiveController(OVRInput.Controller.LTouch);
        }
        else
        {
            SetActiveController(OVRInput.Controller.RTouch);
        }
    }

    void Update()
    {
        if (OVRInput.GetDown(controllerUiButtonsL))
        {
            TrySettingActiveController(OVRInput.Controller.LTouch);
        }
        if (OVRInput.GetDown(controllerUiButtonsR))
        {
            TrySettingActiveController(OVRInput.Controller.RTouch);
        }
    }

    void SetActiveController(OVRInput.Controller c)
    {
        if (c == OVRInput.Controller.LTouch)
        {
            inputModule.rayTransform = cameraRig.leftHandAnchor;
            inputModule.joyPadClickButton = controllerUiButtonsL;
        }
        else
        {
            inputModule.rayTransform = cameraRig.rightHandAnchor;
            inputModule.joyPadClickButton = controllerUiButtonsR;
        }
    }

    public bool TrySettingActiveController(OVRInput.Controller c)
    {
        if (!lockedController)
        {
            SetActiveController(c);
            return true;
        }
        return false;
    }

    public bool SetAndLockActiveController(OVRInput.Controller c)
    {
        if (!lockedController)
        {
            lockedController = true;
            SetActiveController(c);
            return true;
        }
        return false;
    }

    public void UnlockActiveController()
    {
        lockedController = false;
    }
}
