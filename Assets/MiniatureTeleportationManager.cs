using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MiniatureTeleportationManager : MonoBehaviour
{
    public Transform dummyPlayerPosition = null;
    [SerializeField] ViewModeManager viewModeManager = null;

    public void SwitchMode() {
        viewModeManager.CurrentViewMode = ViewModeManager.ViewMode.Fullsize;
    }
}
