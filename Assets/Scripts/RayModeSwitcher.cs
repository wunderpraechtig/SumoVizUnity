using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(RayMarkerScaler))]
public class RayModeSwitcher : MonoBehaviour
{
    public bool enableTeleport = true;
    private HandManager handManager;
    private List<GameObject> markers = new List<GameObject>();

    public enum Hand { Left = 0, Right };
    [SerializeField] private XRInteractorLineVisual defaultLineVisual = null;
    [SerializeField] private XRInteractorLineVisual teleportLineVisual = null;
    [SerializeField] private Hand hand = Hand.Left;
    [SerializeField] private GameObject teleportMarker = null;
    private RayMarkerScaler markerScaler;

    private void Awake()
    {
        markerScaler = gameObject.GetComponent<RayMarkerScaler>();
        handManager = FindObjectOfType<HandManager>();
    }

    private void Start()
    {
        toggleTeleportRay(false);
    }

    private void Update()
    {
        bool buttonPressed = false;
        bool buttonReleased = false;
        if (hand == Hand.Left)
        {
            buttonPressed = handManager.Left.axisPrimary2DTouch.isPressed();
            buttonReleased = handManager.Left.axisPrimary2DTouch.isReleased();
        }
        else
        {
            buttonPressed = handManager.Right.axisPrimary2DTouch.isPressed();
            buttonReleased = handManager.Right.axisPrimary2DTouch.isReleased();
        }

        if (buttonPressed)
        {
            if(enableTeleport)
                toggleTeleportRay(true);
        }

        if (buttonReleased)
        {
            toggleTeleportRay(false);
        }
    }

    private void toggleTeleportRay(bool enable)
    {
        teleportMarker.SetActive(enable);
        // move marker out of view so when it appears again, it's not at the old position for one frame
        if (!enable)
        {
            teleportMarker.transform.localPosition = new Vector3(0, -1, 0);
            markerScaler.ResetMarker();
        }
        defaultLineVisual.enabled = !enable;
        teleportLineVisual.enabled = enable;
    }

}
