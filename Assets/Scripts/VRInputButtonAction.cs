using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRInputButtonAction : MonoBehaviour
{
    public OVRInput.Button button = OVRInput.Button.None;

    [System.Serializable]
    public class InputPressedEvent: UnityEvent { }

    [System.Serializable]
    public class InputReleasedEvent : UnityEvent { }

    [System.Flags]
    public enum Mode {
        Pressed     = 0x1,
        Released    = 0x2
    };

    [SerializeField]
    private Mode mode = Mode.Pressed;

    [SerializeField]
    private InputPressedEvent inputPressedEvent = new InputPressedEvent();

    [SerializeField]
    private InputReleasedEvent inputReleasedEvent = new InputReleasedEvent();
    

    // Update is called once per frame
    void Update()
    {
        if ((mode & Mode.Pressed) != 0)
        {
            if (OVRInput.GetDown(button))
                inputPressedEvent.Invoke();
        }
        if ((mode & Mode.Released) != 0)
        {
            if (OVRInput.GetDown(button))
                inputPressedEvent.Invoke();
        }

    }
}
