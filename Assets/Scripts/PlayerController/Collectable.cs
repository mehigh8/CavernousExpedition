using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour, IInteractable
{
    public string interactionPrompt { get => prompt; }

    public bool isInteractable { get; set; }

    [Header("Config")]
    public string prompt;

    void Start()
    {
        isInteractable = true;
    }

    public void Interact(Interactor interactor)
    {
        GameManager.GetInstance().CollectedItem(gameObject.name);
        Destroy(gameObject);
    }
}
