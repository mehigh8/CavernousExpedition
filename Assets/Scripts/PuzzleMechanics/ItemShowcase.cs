using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemShowcase : PressurePlate
{
    [Header("Item Showcase Config")]
    public Vector3 baseRotation;
    public Vector3 gemOffset;
    public RawImage[] hints;

    private bool alwaysPressed = false;
    private bool animationStarted = false;

    private void Start()
    {
        isPressed = false;
    }

    public override void Update()
    {
        base.Update();

        if (alwaysPressed)
            isPressed = true;

        if (isPressed)
        {
            alwaysPressed = true;

            if (animationStarted == false)
            {
                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.useGravity = false;
                    rb.velocity = Vector3.zero;
                }
                
                Collider col = item.GetComponent<Collider>();
                if (col)
                    col.enabled = false;

                item.transform.position = transform.position + Vector3.up * boxOffset + gemOffset;
                item.transform.rotation = Quaternion.Euler(baseRotation);
                Animator itemAnimator = item.GetComponent<Animator>();
                itemAnimator.enabled = true;
                itemAnimator.Play("GemRotation");
            }

            animationStarted = true;
        }
    }

    public void SetTexture(Texture texture)
    {
        foreach (RawImage hint in hints)
            hint.texture = texture;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * boxOffset, boxSize);
    }
}
