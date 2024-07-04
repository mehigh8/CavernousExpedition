using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatePositionChecker : MonoBehaviour
{

    [Header("Config")]
    public Vector3 boxSize;
    public float boxOffset;
    public LayerMask groundLayer;


    public bool CheckPosition()
    {
        Collider[] cols = Physics.OverlapBox(transform.position + transform.up * boxOffset, boxSize / 2, transform.rotation, groundLayer);
        return cols.Length == 0;
    }
}
