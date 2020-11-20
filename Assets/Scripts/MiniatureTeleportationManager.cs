using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniatureTeleportationManager : MonoBehaviour
{
    public Transform dummyPlayerPosition = null;
    [SerializeField] ViewModeManager viewModeManager = null;

    public void SwitchMode() {
        viewModeManager.CurrentViewMode = ViewModeManager.ViewMode.Fullsize;
    }
}
