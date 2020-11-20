using System.Collections; using System.Collections.Generic; using UnityEngine;  public class ArmPanelController : MonoBehaviour {     [SerializeField] private GameObject mainCamera = null;     private Vector3 initialScale;     private Canvas theCanvas = null;     public float maxAngleDisplayToCam = 30.0f;     public float maxAngleCamToDisplay = 60.0f;     public float maxZRotationDifference = 30.0f;     public Transform handAnchorTransform;

    [Header("Debug")] // Visible variables for a better overview in the inspector     [SerializeField] private float angleDisplayToCamera = 0.0f;     [SerializeField] private float angleCameraToDisplay = 0.0f;     [SerializeField] private float camZRot = 0.0f;     [SerializeField] private float controllerZRot = 0.0f;     [SerializeField] private float relativeZRot = 0.0f;      private void Awake()     {         //handedInputSelector = FindObjectOfType<TwinHandedInputSelector>();         initialScale = transform.localScale;         theCanvas = gameObject.GetComponent<Canvas>();         theCanvas.enabled = false;     }      private void Update()     {
        // Filter angle differences before doing view direction calculation
        bool showPanel = false;         camZRot = mainCamera.transform.eulerAngles.z;         controllerZRot = handAnchorTransform.eulerAngles.z;         relativeZRot = Mathf.Abs((camZRot - controllerZRot));         if (relativeZRot > 180.0f)             relativeZRot = -(relativeZRot - 360.0f);

        Vector3 displayToCameraHeading = (mainCamera.transform.position - transform.position).normalized;
        Vector3 displaySightVector = -transform.forward;
        angleDisplayToCamera = Vector3.Angle(displaySightVector, displayToCameraHeading);

        Vector3 cameraToDisplayHeading = -displayToCameraHeading;
        Vector3 cameraSightVector = mainCamera.transform.forward;
        angleCameraToDisplay = Vector3.Angle(cameraSightVector, cameraToDisplayHeading);

        if (relativeZRot < maxZRotationDifference)
        {
            if (angleCameraToDisplay <= maxAngleCamToDisplay && angleDisplayToCamera <= maxAngleDisplayToCam)
            {
                showPanel = true;
            }
        }

        theCanvas.enabled = showPanel;     }      private void LateUpdate()     {         transform.position = handAnchorTransform.position;         transform.rotation = handAnchorTransform.rotation;         transform.localScale = Vector3.Scale(initialScale, handAnchorTransform.lossyScale);     } } 