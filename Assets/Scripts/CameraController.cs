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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked) //looking around with mouse allowed
        {

            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            float v = -1 * verticalSpeed * Input.GetAxis("Mouse Y");
            //transform.Rotate(v, h, 0); //rotates along z as well - wrong!
            transform.Rotate(new Vector3(0, h, 0), Space.World); //Left-right
            transform.Rotate(new Vector3(v, 0, 0)); //up-down

        }
        Vector3 input = Quaternion.Euler(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, 0) * new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        transform.position += input;

        if (Input.GetKeyDown(KeyCode.Escape)) //ESC unlocks mouse
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetKeyDown(KeyCode.Space)) //SPACE - to toggle to change from locked to none and vice versa
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
