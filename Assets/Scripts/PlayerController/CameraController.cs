using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Config")]
    public float minSensitivity;
    public float maxSensitivity;
    [Header("References")]
    public Transform orientation;
    public Transform cameraPosition;

    private float xRotation;
    private float yRotation;

    [SerializeField] private float xSensitivity = 400;
    [SerializeField] private float ySensitivity = 400;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        transform.position = cameraPosition.position;

        if (GameManager.GetInstance().pauseManager.isPaused)
            return;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * xSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * ySensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void ChangeSensitivity(float sensitivity)
    {
        xSensitivity = minSensitivity + (maxSensitivity - minSensitivity) * sensitivity;
        ySensitivity = minSensitivity + (maxSensitivity - minSensitivity) * sensitivity;
    }
}
