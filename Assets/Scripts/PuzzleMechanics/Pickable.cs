using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour, IInteractable
{
    public string interactionPrompt { get => prompts[currentPrompt]; }
    public bool isInteractable { get; set; }
    [Header("Config")]
    public string[] prompts;
    public float carryOffset;


    private bool isCarried;
    private Interactor interactor;
    private Collider col;
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private int currentPrompt;

    void Start()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (col == null || rb == null)
            Debug.LogError("Pickable object must have collider and rigidbody");

        currentPrompt = 0;
        isInteractable = true;
    }

    void Update()
    {
        if (isCarried)
        {
            currentPrompt = 1;
            interactor.interactionText.text = interactionPrompt;

            transform.position = interactor.interactorTransform.position + interactor.interactorTransform.forward * carryOffset;
            if (meshRenderer)
            {
                Color meshColor = meshRenderer.material.color;
                meshRenderer.material.color = new Color(meshColor.r, meshColor.g, meshColor.b, 0.5f);
            }

            if (Input.GetKeyDown(interactor.interactKey) && interactor.releasedButton && !GameManager.GetInstance().pauseManager.isPaused)
            {
                if (Physics.Raycast(interactor.interactorTransform.position, interactor.interactorTransform.forward, out RaycastHit hit, interactor.interactRange))
                {
                    if (col) col.enabled = true;

                    if (rb) {
                        rb.velocity = Vector3.zero;
                        rb.useGravity = true;
                        rb.freezeRotation = false;
                    }

                    if (meshRenderer)
                    {
                        Color meshColor = meshRenderer.material.color;
                        meshRenderer.material.color = new Color(meshColor.r, meshColor.g, meshColor.b, 1f);
                    }

                    isCarried = false;
                    interactor.canInteract = true;
                    transform.position = hit.point + Vector3.up + hit.normal;
                }
            }
        }
        else
        {
            currentPrompt = 0;
        }
    }

    public void Interact(Interactor interactor)
    {
        interactor.canInteract = false;
        
        if (col) col.enabled = false;
        if (rb)
        {
            rb.useGravity = false;
            rb.freezeRotation = true;
        }

        this.interactor = interactor;
        isCarried = true;
    }
}
