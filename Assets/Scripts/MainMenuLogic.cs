using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLogic : MonoBehaviour
{
    public Transform userPositionTransform;
    public float distanceFromUser = 2.0f;
    public GameObject InitialMainMenu;
    public Button buttonLoadSimulation;
    private FileLoader fileLoader = null;
    private bool filebrowserOpen = false;
    [SerializeField] private FileBrowser fileBrowser = null;

    private void Awake()
    {
        fileLoader = FindObjectOfType<FileLoader>();
        FileBrowser.setInstance(fileBrowser);
    }

    private void Start()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Xml Files", ".xml"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter(".xml");
        gameObject.SetActive(false);
        ResetMenus();
    }

    public void ToggleMainMenu() {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            ResetMenus();
        }
        else {
            Quaternion rotation = userPositionTransform.rotation;
            rotation.x = 0;
            rotation.z = 0;
            Vector3 relativePosition = rotation * Vector3.forward * distanceFromUser;
            gameObject.transform.position = relativePosition + userPositionTransform.position;
            gameObject.transform.rotation = rotation;
            gameObject.SetActive(!gameObject.activeSelf);
        }
        
    }

    /// <summary>
    /// Disables all child objects
    /// </summary>
    private void ResetMenus() {
        if(filebrowserOpen) return;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        InitialMainMenu.SetActive(true);
    }

    public void HandleLoadSimulation() {
        buttonLoadSimulation.interactable = false;
        StartCoroutine(LoadSimulation());
    }

    public void HandleQuit() {
        Application.Quit();
    }

    IEnumerator LoadSimulation()
    {
        //fileLoader.LoadWithEditorDialog();

        string pathGeometry = "";
        string pathTrajectories = "";
        bool successGeometry = false;
        bool successTrajectories = false;

        yield return FileBrowser.WaitForLoadDialog(false, null, "Load Geometry", "Load");

        successGeometry = FileBrowser.Success;
        pathGeometry = FileBrowser.Result;
        Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);

        yield return FileBrowser.WaitForLoadDialog(false, null, "Load Trajectories", "Load");

        successTrajectories = FileBrowser.Success;
        pathTrajectories = FileBrowser.Result;
        Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);

        StartCoroutine(fileLoader.ClearCurrentSimulation());

        if (!successGeometry)
            pathGeometry = "";

        if (!successTrajectories)
            pathTrajectories = "";

        yield return fileLoader.LoadSimulation(pathGeometry, pathTrajectories);

        buttonLoadSimulation.interactable = true;

        yield return null;
    }
}
