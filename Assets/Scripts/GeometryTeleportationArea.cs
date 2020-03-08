using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GeometryTeleportationArea : BaseTeleportationInteractable
{
    private MiniatureTeleportationManager teleportationManager = null;
    public void setTeleportationManager(MiniatureTeleportationManager mtp) {
        teleportationManager = mtp;
    }

    protected override bool GenerateTeleportRequest(XRBaseInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
    {
        teleportationManager.dummyPlayerPosition.position = raycastHit.point;
        teleportationManager.SwitchMode();
        teleportRequest.destinationPosition = teleportationManager.dummyPlayerPosition.position;
        teleportRequest.destinationUpVector = transform.up; // use the area transform for data.
        teleportRequest.destinationForwardVector = transform.forward;
        teleportRequest.destinationRotation = transform.rotation;
        return true;
    }
}
