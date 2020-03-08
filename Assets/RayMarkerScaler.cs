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
    public float minimumScale = 0.0f;
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
        if (marker && interactable.gameObject.layer == 11) // if it's hovering over floor geometry
        {
            Vector3 scale = interactable.transform.lossyScale;
            float max = scale.x;
            if (max < scale.y) max = scale.y;
            if (max < scale.z) max = scale.z;
            if (max < Mathf.Abs(minimumScale)) max = minimumScale;
            if (max > Mathf.Abs(maximumScale)) max = maximumScale;
            scale = new Vector3(max, max, max);
            marker.transform.localScale = scale;
            lineVisual.lineWidth = defaultLineWidth * max;
        }
    }

    public void HandleHoverExit(XRBaseInteractable interactable)
    {
        //if (marker && interactable.gameObject.layer == 11) // if it's hovering over floor geometry
        //{
        //    lastExitFrame = Time.frameCount;
        //}
    }
}
