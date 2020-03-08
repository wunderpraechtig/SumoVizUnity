using UnityEngine;
using UnityEngine.XR;

public class HandManager : MonoBehaviour
{
    [SerializeField] private HandDeviceManager hdm = null;

    // Left Hand
    public HandDeviceManager.ManagedButton buttonLeftPrimary;
    public HandDeviceManager.ManagedButton buttonLeftSecondary;
    public HandDeviceManager.ManagedButton buttonLeftTrigger;
    public HandDeviceManager.ManagedAxis1D axisLeftGrip;
    public HandDeviceManager.ManagedAxis1D axisLeftTrigger;
    public HandDeviceManager.ManagedAxis2D axisLeftPrimary2D;
    public HandDeviceManager.ManagedButton axisLeftPrimary2DClick;
    public HandDeviceManager.ManagedButton axisLeftPrimary2DTouch;


    // Right Hand
    public HandDeviceManager.ManagedButton buttonRightPrimary;
    public HandDeviceManager.ManagedButton buttonRightSecondary;
    public HandDeviceManager.ManagedButton buttonRightTrigger;
    public HandDeviceManager.ManagedAxis1D axisRightGrip;
    public HandDeviceManager.ManagedAxis1D axisRightTrigger;
    public HandDeviceManager.ManagedAxis2D axisRightPrimary2D;
    public HandDeviceManager.ManagedButton axisRightPrimary2DClick;
    public HandDeviceManager.ManagedButton axisRightPrimary2DTouch;


    void Update()
    {
        // Left
        hdm.updateButtonFeature(hdm.leftHandDevices, CommonUsages.primaryButton, ref buttonLeftPrimary);
        hdm.updateButtonFeature(hdm.leftHandDevices, CommonUsages.secondaryButton, ref buttonLeftSecondary);
        hdm.updateButtonFeature(hdm.leftHandDevices, CommonUsages.triggerButton, ref buttonLeftTrigger);
        

        hdm.updateAxis1DFeature(hdm.leftHandDevices, CommonUsages.grip, ref axisLeftGrip);
        hdm.updateAxis1DFeature(hdm.leftHandDevices, CommonUsages.trigger, ref axisLeftTrigger);
        hdm.updateAxis2DFeature(hdm.leftHandDevices, CommonUsages.primary2DAxis, ref axisLeftPrimary2D);
        hdm.updateButtonFeature(hdm.leftHandDevices, CommonUsages.primary2DAxisClick, ref axisLeftPrimary2DClick);
        hdm.updateButtonFeature(hdm.leftHandDevices, CommonUsages.primary2DAxisTouch, ref axisLeftPrimary2DTouch);

        // Right
        hdm.updateButtonFeature(hdm.rightHandDevices, CommonUsages.primaryButton, ref buttonRightPrimary);
        hdm.updateButtonFeature(hdm.rightHandDevices, CommonUsages.secondaryButton, ref buttonRightSecondary);
        hdm.updateButtonFeature(hdm.rightHandDevices, CommonUsages.triggerButton, ref buttonRightTrigger);

        hdm.updateAxis1DFeature(hdm.rightHandDevices, CommonUsages.grip, ref axisRightGrip);
        hdm.updateAxis1DFeature(hdm.rightHandDevices, CommonUsages.trigger, ref axisRightTrigger);
        hdm.updateAxis2DFeature(hdm.rightHandDevices, CommonUsages.primary2DAxis, ref axisRightPrimary2D);
        hdm.updateButtonFeature(hdm.rightHandDevices, CommonUsages.primary2DAxisClick, ref axisRightPrimary2DClick);
        hdm.updateButtonFeature(hdm.rightHandDevices, CommonUsages.primary2DAxisTouch, ref axisRightPrimary2DTouch);
    }


}
