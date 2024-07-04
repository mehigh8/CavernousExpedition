using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Config")]
    //public float rayLength;
    public float boxOffset;
    public Vector3 boxSize;
    public string expectedItem = "";
    public bool canBePressedByPlayer = false;
    [Space]
    public bool isPressed;
    [HideInInspector] public GameObject item;

    private void Start()
    {
        isPressed = false;
    }

    public virtual void Update()
    {
        //if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hitInfo, rayLength))
        //{
        //    if (canBePressedByPlayer && hitInfo.collider.tag.Equals("Player"))
        //        isPressed = true;
        //    else
        //        isPressed = hitInfo.collider.name.Contains(expectedItem);
        //}
        //else
        //{
        //    isPressed = false;
        //}

        Collider[] colliders = Physics.OverlapBox(transform.position + Vector3.up * boxOffset, boxSize / 2, transform.rotation);
        isPressed = false;
        foreach (Collider collider in colliders)
        {
            if (canBePressedByPlayer && collider.tag.Equals("Player"))
            {
                isPressed = true;
                break;
            }
            else if (collider.name.Contains(expectedItem))
            {
                isPressed = true;
                item = collider.gameObject;
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawRay(transform.position, Vector3.up * rayLength);
        Gizmos.DrawWireCube(transform.position + Vector3.up * boxOffset, boxSize);
    }
}
