using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniatureViewControl : MonoBehaviour
{
    public enum HandFunction { Move = 0, Rotate };

    private HandFunction functionLeft = HandFunction.Move;
    private HandFunction functionRight = HandFunction.Rotate;
    private HandManager handManager = null;
    public bool IsHeightLocked { get; set; } = true;

    [SerializeField] private Transform leftController = null;
    [SerializeField] private Transform rightController = null;
    [SerializeField] private Transform miniatureRotate = null;
    [SerializeField] private Transform miniatureMoveScale = null;
    [SerializeField] private Transform miniaturePedestal = null;
    [SerializeField] private bool isGrabbingLeft = false;
    [SerializeField] private bool isGrabbingRight = false;
    [SerializeField] private float grabThreshold = 0.9f;

    [SerializeField] private Vector3 grabPointMove;
    [SerializeField] private Vector3 grabPointRotateGlobal;
    [SerializeField] private Vector3 grabPointScaleA;
    [SerializeField] private Vector3 grabPointScaleB;

    [SerializeField] private Vector3 savedMiniaturePosGlobal;
    [SerializeField] private Vector3 savedMiniaturePos;
    [SerializeField] private Vector3 savedPedestalPos;
    [SerializeField] private Quaternion savedMiniatureRot;
    [SerializeField] private Vector3 savedMiniatureScale;

    [SerializeField] private float rotAngle;
    [SerializeField] private Vector3 rotInitialVec;
    [SerializeField] private Vector3 rotNewVec;

    private void Awake()
    {
        handManager = FindObjectOfType<HandManager>();
    }

    void Update()
    {
        if (handManager.Left.axisGrip.getState(grabThreshold))
        {
            if (!isGrabbingLeft)
            { // when not already grabbing
                if (isGrabbingRight)
                {
                    grabPointScaleA = leftController.localPosition;
                    grabPointScaleB = rightController.localPosition;
                }
                if (functionLeft == HandFunction.Move)
                {
                    grabPointMove = leftController.localPosition;
                    grabPointRotateGlobal = rightController.position;
                }
                else
                {
                    grabPointMove = rightController.localPosition;
                    grabPointRotateGlobal = leftController.position;
                }
                saveMiniatureTransforms();
                isGrabbingLeft = true;
            }
        }

        else
        {
            if (isGrabbingLeft && isGrabbingRight)
            {
                if (functionLeft == HandFunction.Move)
                {
                    grabPointMove = leftController.localPosition;
                    grabPointRotateGlobal = rightController.position;
                }
                else
                {
                    grabPointMove = rightController.localPosition;
                    grabPointRotateGlobal = leftController.position;
                }
            }
            isGrabbingLeft = false;
        }

        if (handManager.Right.axisGrip.getState(grabThreshold))
        {
            if (!isGrabbingRight)
            { // when not already grabbing
                if (isGrabbingLeft)
                {
                    grabPointScaleA = leftController.localPosition;
                    grabPointScaleB = rightController.localPosition;
                }
                if (functionRight == HandFunction.Move)
                {
                    grabPointMove = rightController.localPosition;
                    grabPointRotateGlobal = leftController.position;
                }
                else
                {
                    grabPointMove = leftController.localPosition;
                    grabPointRotateGlobal = rightController.position;
                }
                saveMiniatureTransforms();
                isGrabbingRight = true;
            }
        }
        else
        {
            if (isGrabbingLeft && isGrabbingRight)
            {
                if (functionRight == HandFunction.Move)
                {
                    grabPointMove = rightController.localPosition;
                    grabPointRotateGlobal = leftController.position;
                }
                else
                {
                    grabPointMove = leftController.localPosition;
                    grabPointRotateGlobal = rightController.position;
                }
            }
            isGrabbingRight = false;
        }
        HandleInputs();
    }

    private void HandleInputs()
    {
        // Case rotation or move
        if (isGrabbingLeft != isGrabbingRight)
        {
            HandFunction currentFunction;
            Transform currentHandAnchor;
            if (isGrabbingLeft)
            {
                currentFunction = functionLeft;
                currentHandAnchor = leftController;
            }
            else
            {
                currentFunction = functionRight;
                currentHandAnchor = rightController;
            }

            if (currentFunction == HandFunction.Move)
            {
                Vector3 diff = (currentHandAnchor.localPosition - grabPointMove);
                float heightDiff = diff.y;
                diff.y = 0;
                miniatureMoveScale.localPosition = Quaternion.Inverse(miniatureRotate.rotation) * (savedMiniaturePos + diff);

                if (!IsHeightLocked)
                {
                    Vector3 pedestalPos = savedPedestalPos;
                    pedestalPos.y += heightDiff;
                    miniaturePedestal.localPosition = pedestalPos;
                }
            }
            else
            {
                rotInitialVec = grabPointRotateGlobal - miniaturePedestal.position;
                rotNewVec = currentHandAnchor.position - miniaturePedestal.position;
                rotInitialVec.y = 0;
                rotNewVec.y = 0;
                rotAngle = Vector3.SignedAngle(rotInitialVec, rotNewVec, Vector3.up);
                miniatureRotate.localRotation = savedMiniatureRot * Quaternion.Euler(Vector3.up * rotAngle);
            }

        }

        // Case scaling
        if (isGrabbingLeft && isGrabbingRight)
        {
            float initialDistance = Vector3.Distance(grabPointScaleA, grabPointScaleB);
            float newDistance = Vector3.Distance(leftController.localPosition, rightController.localPosition);
            Vector3 newScale = savedMiniatureScale * (newDistance / initialDistance);
            miniatureMoveScale.localScale = newScale;
        }
    }

    void saveMiniatureTransforms()
    {
        savedMiniaturePosGlobal = miniatureMoveScale.position;
        savedMiniaturePos = miniatureRotate.localRotation * miniatureMoveScale.localPosition;
        savedPedestalPos = miniaturePedestal.localPosition;
        savedMiniatureRot = miniatureRotate.localRotation;
        savedMiniatureScale = miniatureMoveScale.localScale;
    }
}
