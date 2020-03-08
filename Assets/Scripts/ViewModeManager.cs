using System;
using UnityEngine;

public class ViewModeManager : MonoBehaviour
{
    [System.Serializable]
    public enum ViewMode { Miniature = 0, Fullsize };

    [SerializeField] private MiniatureViewMode miniatureViewMode = null;
    [SerializeField] private FullsizeViewMode fullsizeViewMode = null;

    public event Action<ViewMode> viewModeEvent;
    ViewMode currentViewMode = ViewMode.Miniature;
    public ViewMode CurrentViewMode {
        get { return currentViewMode; }
        set {
            if (currentViewMode != value) {
                currentViewMode = value;
                viewModeEvent.Invoke(currentViewMode);
                if (currentViewMode == ViewMode.Miniature)
                    enableMiniatureViewMode();
                else
                    enableFullsizeViewMode();
            }
        }
    }
    

    private void Awake()
    {
        LoadViewMode();
    }

    private void Start()
    {
        if (CurrentViewMode == ViewMode.Miniature)
        {
            enableMiniatureViewMode();
        }
        else
        {
            enableFullsizeViewMode();
        }
    }

    private void LoadViewMode()
    {
        if (PlayerPrefs.HasKey("PlayState"))
            CurrentViewMode = (ViewMode)PlayerPrefs.GetInt("PlayState");
        else
        {
            PlayerPrefs.SetInt("PlayState", (int)CurrentViewMode);
        }
    }

    public void enableMiniatureViewMode()
    {
        fullsizeViewMode.SaveState();
        miniatureViewMode.Enable();
        fullsizeViewMode.gameObject.SetActive(false);
        miniatureViewMode.gameObject.SetActive(true);
    }

    public void enableFullsizeViewMode()
    {
        miniatureViewMode.SaveState();
        fullsizeViewMode.Enable();
        miniatureViewMode.gameObject.SetActive(false);
        fullsizeViewMode.gameObject.SetActive(true);
    }
}
