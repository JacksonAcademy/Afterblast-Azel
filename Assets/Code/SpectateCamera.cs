using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectateCamera : MonoBehaviour
{
    public float sensitivity, movementSpeed;
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;

    Vector2 rotation = Vector2.zero;
    const string xAxis = "Mouse X";
    const string yAxis = "Mouse Y";
    const string moveXAxis = "Horizontal";
    const string moveZAxis = "Vertical";
    const string moveYAxis = "YAxis";
    public Camera cam;

    public bool lockCursor = true;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }
    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockCursor = !lockCursor;


        }

        if (!cam.enabled)
            return;
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
        rotation.x += Input.GetAxis(xAxis) * sensitivity;
        rotation.y += Input.GetAxis(yAxis) * sensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
        transform.localRotation = xQuat * yQuat; //Quaternions seem to rotate more consistently than EulerAngles. Sensitivity seemed to change slightly at certain degrees using Euler. transform.localEulerAngles = new Vector3(-rotation.y, rotation.x, 0);
        float x = Input.GetAxis(moveXAxis);
        float z = Input.GetAxis(moveZAxis);
        float y = Input.GetAxis(moveYAxis);

        Vector3 move = transform.right * x + transform.forward * z + (y * Vector3.up);
        if(!Input.GetKey(KeyCode.LeftShift))
            transform.Translate(move * movementSpeed * Time.deltaTime, Space.World);
        else
            transform.Translate(move * movementSpeed *2* Time.deltaTime, Space.World);
    }
}
