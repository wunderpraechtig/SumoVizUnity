using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewModeManager : MonoBehaviour
{
    private MiniatureViewMode miniatureViewMode;

    private void Awake()
    {
        miniatureViewMode = FindObjectOfType<MiniatureViewMode>();
    }

    private void Start()
    {
        EnableMiniatureMode();
    }

    public void EnableMiniatureMode()
    {
        Physics.IgnoreLayerCollision(9, 11);
        Physics.IgnoreLayerCollision(9, 12);
        Physics.IgnoreLayerCollision(9, 13);
        miniatureViewMode.EnableMiniatureMode();
        miniatureViewMode.gameObject.SetActive(true);
    }

    public void DisableMiniatureMode()
    {
        Physics.IgnoreLayerCollision(9, 11, false);
        Physics.IgnoreLayerCollision(9, 12, false);
        Physics.IgnoreLayerCollision(9, 13, false);
        miniatureViewMode.gameObject.SetActive(false);
    }
}
