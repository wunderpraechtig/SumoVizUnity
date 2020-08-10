using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandDeviceManager : MonoBehaviour
{
    public struct ManagedButton
    {
        bool downThisFrame;
        bool downLastFrame;

        public void setCurrentState(bool newState)
        {
            downLastFrame = downThisFrame;
            downThisFrame = newState;
        }
        public bool isDown()
        {
            return downThisFrame;
        }
        public bool isPressed()
        {
            return (!downLastFrame & downThisFrame);
        }
        public bool isReleased()
        {
            return (downLastFrame & !downThisFrame);
        }
    }

    public struct ManagedAxis1D
    {
        float value;

        public void setValue(float newVal)
        {
            value = newVal;
        }

        public float getValue()
        {
            return value;
        }
        // Whether the value is above a given threshold
        public bool getState(float threshold)
        {
            if (value >= threshold)
                return true;
            return false;
        }
    }

    public struct ManagedAxis2D
    {
        Vector2 value;

        public void setValue(Vector2 newVal)
        {
            value = newVal;
        }
        public Vector2 getValue()
        {
            return value;
        }
    }

    public List<InputDevice> leftHandDevices = new List<InputDevice>();
    public List<InputDevice> rightHandDevices = new List<InputDevice>();
    
    void FixedUpdate()
    {
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);
    }

    public void updateButtonFeature(bool left, InputFeatureUsage<bool> feature, ref ManagedButton button)
    {
        List<InputDevice> devices = left ? leftHandDevices : rightHandDevices;
        bool state = false;
        bool tempState = false;
        foreach (var device in devices)
        {
            // Try to get the state of this device and if successful merge it with the total state, biased towards true
            if (device.TryGetFeatureValue(feature, out tempState))
                state = state | tempState;
        }
        button.setCurrentState(state);
    }

    public void updateAxis1DFeature(bool left, InputFeatureUsage<float> feature, ref ManagedAxis1D axis)
    {
        List<InputDevice> devices = left ? leftHandDevices : rightHandDevices;
        float state = 0.0f;
        float tempState = 0.0f;
        foreach (var device in devices)
        {
            // Here we use the highest axis result we can find, assuming the controller in use is the only one returning a non-zero value
            if (device.TryGetFeatureValue(feature, out tempState))
                if (state < tempState) state = tempState;
        }
        axis.setValue(state);
    }

    public void updateAxis2DFeature(bool left, InputFeatureUsage<Vector2> feature, ref ManagedAxis2D axis)
    {
        List<InputDevice> devices = left ? leftHandDevices : rightHandDevices;
        Vector2 state = Vector2.zero;
        Vector2 tempState = Vector2.zero;
        foreach (var device in devices)
        {
            // Here we use the highest axis result we can find, assuming the controller in use is the only one returning a non-zero value
            if (device.TryGetFeatureValue(feature, out tempState))
            {
                if (state.x < tempState.x) state.x = tempState.x;
                if (state.y < tempState.y) state.y = tempState.y;
            }
        }
        axis.setValue(state);
    }
}
