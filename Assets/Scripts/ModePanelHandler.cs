using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ModePanelHandler : MonoBehaviour
{
    [SerializeField] private ViewModeManager viewModeManager = null;
    [SerializeField] private GameObject panelMiniatureControl = null;
    [SerializeField] private GameObject panelFullsizeControl = null;
    [SerializeField] private Button buttonMiniatureInactive = null;
    [SerializeField] private Button buttonFullsizeInactive = null;

    private void Awake()
    {
        ViewModeManager vmm = FindObjectOfType<ViewModeManager>();
        vmm.viewModeEvent += HandleViewModeChanged;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (viewModeManager.CurrentViewMode == ViewModeManager.ViewMode.Miniature)
        {
            HandleViewModeChanged(ViewModeManager.ViewMode.Miniature);
        }
        else
        {
            HandleViewModeChanged(ViewModeManager.ViewMode.Fullsize);
        }
    }

    public void EnableMiniatureMode() {
        viewModeManager.CurrentViewMode = ViewModeManager.ViewMode.Miniature;
    }

    public void EnableFullsizeMode() {
        viewModeManager.CurrentViewMode = ViewModeManager.ViewMode.Fullsize;
    }

    private void HandleViewModeChanged(ViewModeManager.ViewMode viewMode) {
        bool miniatureMode = (viewMode == ViewModeManager.ViewMode.Miniature);
        panelMiniatureControl.SetActive(miniatureMode);
        panelFullsizeControl.SetActive(!miniatureMode);

        buttonMiniatureInactive.gameObject.SetActive(!miniatureMode);
        buttonFullsizeInactive.gameObject.SetActive(miniatureMode);
    }
}
