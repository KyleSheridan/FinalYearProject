using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    private bool hasControl = false;

    public MapGenerator generator;

    public GameObject menu;

    public float speed = 5f;

    public float mouseSensitivity = 100f;
    float xRotation = 0f;
    float yRotation = 0f;

    public Vector3 move;

    void Update()
    {
        if(!hasControl) { return; }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitTest();
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        yRotation += mouseX;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        move = Vector3.right * x + Vector3.forward * y;

        move.Normalize();

        transform.Translate(move * speed * Time.deltaTime);
    }

    public void GiveControl()
    {
        hasControl = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ExitTest()
    {
        transform.position = Vector3.zero;

        hasControl = false;
        Cursor.lockState = CursorLockMode.None;

        generator.ClearMap();
        menu.SetActive(true);
    }
}
