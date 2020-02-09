using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCharacterCameraConstraint : MonoBehaviour
{
    public Transform centerEyeAnchor;
    private CharacterController characterController;

    void Awake()
    {
        characterController = this.GetComponent<CharacterController>();    
    }
    
    void Update()
    {
        characterController.transform.position.Set(0, 0, centerEyeAnchor.position.magnitude);
    }
}
