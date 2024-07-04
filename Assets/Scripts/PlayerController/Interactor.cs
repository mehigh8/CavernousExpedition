using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public interface IInteractable
{
    public string interactionPrompt { get; }
    public bool isInteractable { get; set; }
    public void Interact(Interactor interactor);
}

public class Interactor : MonoBehaviour
{
    [Header("Config")]
    public Transform interactorTransform;
    public float interactRange;
    public KeyCode interactKey;
    [Header("References")]
    public TMP_Text interactionText;
    [Space]
    public bool canInteract;
    public bool releasedButton;

    void Start()
    {
        canInteract = true;
        releasedButton = true;
    }

    void Update()
    {
        if (Physics.Raycast(interactorTransform.position, interactorTransform.forward, out RaycastHit hit, interactRange))
        {
            if (hit.collider.gameObject.TryGetComponent(out IInteractable interactable))
            {
                if (interactable.isInteractable)
                {
                    interactionText.text = interactable.interactionPrompt;
                    if (Input.GetKeyDown(interactKey) && canInteract && !GameManager.GetInstance().pauseManager.isPaused)
                    {
                        releasedButton = false;
                        interactable.Interact(this);
                    }
                }
            }
            else
            {
                interactionText.text = "";
            }
        }
        else
        {
            if (canInteract)
                interactionText.text = "";
        }

        if (Input.GetKeyUp(interactKey))
            releasedButton = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(interactorTransform.position, interactorTransform.forward * interactRange);
    }
}
