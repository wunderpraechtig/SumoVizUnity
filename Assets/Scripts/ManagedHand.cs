using UnityEngine;
using UnityEngine.XR;

public class ManagedHand : MonoBehaviour
{
    [SerializeField] private HandDeviceManager handDeviceManager = null;
    [SerializeField] private bool leftHand = true;

    public HandDeviceManager.ManagedButton buttonPrimary;
    public HandDeviceManager.ManagedButton buttonSecondary;
    public HandDeviceManager.ManagedButton buttonTrigger;
    public HandDeviceManager.ManagedAxis1D axisGrip;
    public HandDeviceManager.ManagedAxis1D axisTrigger;
    public HandDeviceManager.ManagedAxis2D axisPrimary2D;
    public HandDeviceManager.ManagedButton axisPrimary2DClick;
    public HandDeviceManager.ManagedButton axisPrimary2DTouch;

    void Update()
    {
        handDeviceManager.updateButtonFeature(leftHand, CommonUsages.primaryButton, ref buttonPrimary);
        handDeviceManager.updateButtonFeature(leftHand, CommonUsages.secondaryButton, ref buttonSecondary);
        handDeviceManager.updateButtonFeature(leftHand, CommonUsages.triggerButton, ref buttonTrigger);

        handDeviceManager.updateAxis1DFeature(leftHand, CommonUsages.grip, ref axisGrip);
        handDeviceManager.updateAxis1DFeature(leftHand, CommonUsages.trigger, ref axisTrigger);
        handDeviceManager.updateAxis2DFeature(leftHand, CommonUsages.primary2DAxis, ref axisPrimary2D);
        handDeviceManager.updateButtonFeature(leftHand, CommonUsages.primary2DAxisClick, ref axisPrimary2DClick);
        handDeviceManager.updateButtonFeature(leftHand, CommonUsages.primary2DAxisTouch, ref axisPrimary2DTouch);
    }
}
