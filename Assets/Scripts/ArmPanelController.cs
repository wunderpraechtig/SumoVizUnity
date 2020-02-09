using System.Collections; using System.Collections.Generic; using UnityEngine;  public class ArmPanelController : MonoBehaviour {     private OVRCameraRig cameraRig;     private TwinHandedInputSelector handedInputSelector;     private Vector3 initialScale;     [SerializeField] private float maxAngleCamToDisplay = 40.0f;     [SerializeField] private float maxAngleDisplayToCam = 40.0f;     [SerializeField] private float maxZRotationDifference = 20.0f;     [SerializeField] private GameObject mainPanel = null;     public OVRInput.Controller requiredController = OVRInput.Controller.None;     public Transform handAnchorTransform;          public float angleDisplayToCamera = 0.0f;     public float angleCameraToDisplay = 0.0f;     public float camZRot = 0.0f;     public float controllerZRot = 0.0f;      private void Awake()     {         cameraRig = FindObjectOfType<OVRCameraRig>();         handedInputSelector = FindObjectOfType<TwinHandedInputSelector>();         mainPanel.SetActive(false);         initialScale = transform.localScale;     }      private void Update()     {
        // Filter angle differences before doing view direction calculation
        bool showPanel = false;         camZRot = cameraRig.centerEyeAnchor.rotation.z;         controllerZRot = handAnchorTransform.rotation.z;         if (Mathf.Abs(camZRot - controllerZRot) < maxZRotationDifference/180.0f)
        {
            Vector3 displayToCameraHeading = (cameraRig.centerEyeAnchor.transform.position - transform.position).normalized;
            Vector3 displaySightVector = -transform.forward;
            angleDisplayToCamera = Vector3.Dot(displaySightVector, displayToCameraHeading);

            Vector3 cameraToDisplayHeading = -displayToCameraHeading;
            Vector3 cameraSightVector = cameraRig.centerEyeAnchor.forward;
            angleCameraToDisplay = Vector3.Dot(cameraSightVector, cameraToDisplayHeading);

            if ((1.0f - angleCameraToDisplay) <= maxAngleCamToDisplay/180.0f && (1.0f - angleDisplayToCamera) <= maxAngleDisplayToCam/ 180.0f)
            {
                showPanel = true;
            }
        }         if (showPanel)
        {
            mainPanel.SetActive(true);
            if (requiredController != OVRInput.Controller.None)
                handedInputSelector.SetAndLockActiveController(requiredController);
        }
        else {
            mainPanel.SetActive(false);
            handedInputSelector.UnlockActiveController();
        }     }      private void LateUpdate()     {         transform.position = handAnchorTransform.position;         transform.rotation = handAnchorTransform.rotation;         transform.localScale = Vector3.Scale(initialScale, handAnchorTransform.lossyScale);     } } 