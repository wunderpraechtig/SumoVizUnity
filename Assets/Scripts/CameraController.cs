using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float horizontalSpeed = 2.0f;
    float verticalSpeed = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: prevent a mouse rotation along z axis

        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = -1 * verticalSpeed * Input.GetAxis("Mouse Y");
        transform.Rotate(v, h, 0);
        Vector3 input = Quaternion.Euler(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, 0) * new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        transform.position += input;

    }
}
