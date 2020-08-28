using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRInteractorLineVisual))]
public class RayMarkerScaler : MonoBehaviour
{
    private GameObject marker;
    private XRInteractorLineVisual lineVisual;
    public float minimumScale = 0.01f;
    public float maximumScale = 1.0f;
    private float defaultLineWidth = 0.01f;

    void Awake()
    {
        lineVisual = gameObject.GetComponent<XRInteractorLineVisual>();
        marker = lineVisual.reticle;
        defaultLineWidth = lineVisual.lineWidth;
    }

    public void ResetMarker() {
        marker.transform.localScale = Vector3.one;
        lineVisual.lineWidth = defaultLineWidth;
    }

    public void HandleHoverEnter(XRBaseInteractable interactable) {
        // check for geometry layer
        if (marker && interactable && interactable.gameObject.layer == 11) {
            float scale = interactable.transform.lossyScale.x;
            scale = Mathf.Clamp(scale, minimumScale, maximumScale);
            marker.transform.localScale = new Vector3(scale, scale, scale);
            lineVisual.lineWidth = defaultLineWidth * scale;
        }
    }

    public void HandleHoverExit(XRBaseInteractable interactable){
        // check for geometry layer
        if (marker && interactable && interactable.gameObject.layer == 11) {
            marker.transform.localScale = Vector3.one;
            lineVisual.lineWidth = defaultLineWidth;
        }
    }
}
