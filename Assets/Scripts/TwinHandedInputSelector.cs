using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TwinHandedInputSelector : MonoBehaviour
{
    private OVRInputModule inputModule;

    [SerializeField]
    private Transform laserAnchorL = null;
    [SerializeField]
    private Transform laserAnchorR = null;
    [SerializeField]
    private bool lockedController = false;
    [SerializeField]
    private GameObject lockingObject = null;

    public OVRInput.Button controllerUiButtonsL = OVRInput.Button.Three;
    public OVRInput.Button controllerUiButtonsR = OVRInput.Button.One;

    void Awake()
    {
        inputModule = FindObjectOfType<OVRInputModule>();
    }

    private void Start()
    {
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
            inputModule.rayTransform = laserAnchorL;
            inputModule.joyPadClickButton = controllerUiButtonsL;
        }
        else
        {
            inputModule.rayTransform = laserAnchorR;
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

    public bool SetAndLockActiveController(OVRInput.Controller c, GameObject obj)
    {
        if (!lockedController)
        {
            lockedController = true;
            lockingObject = obj;
            SetActiveController(c);
            return true;
        }
        return false;
    }

    public void UnlockActiveController(GameObject obj)
    {
        if(obj = lockingObject)
            lockedController = false;
    }
}
