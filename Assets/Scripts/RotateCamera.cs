using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    [Header("Config")]
    public float rotationSpeed;
    public GameObject cam;

    void FixedUpdate()
    {
        cam.transform.Rotate(new Vector3(0, rotationSpeed, 0));
    }
}
