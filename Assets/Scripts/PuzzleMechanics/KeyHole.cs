using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHole : MonoBehaviour, IInteractable
{
    public string interactionPrompt { get => "Complete puzzle (requires key)"; }
    public bool isInteractable { get; set; }
    [Header("Config")]
    public GameObject key;
    public Animator keyAnimator;
    public KeyAssemblingPuzzle parentPuzzle = null;

    void Start()
    {
        parentPuzzle = GetComponentInParent<KeyAssemblingPuzzle>();
        isInteractable = true;
    }

    public void Interact(Interactor interactor)
    {
        if (parentPuzzle == null || parentPuzzle.isCompleted)
            return;

        if (parentPuzzle.pickedKey)
        {
            MeshRenderer[] meshRenderers = key.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.material.color = parentPuzzle.keyColor;
            key.SetActive(true);
            keyAnimator.Play("KeyHoleAnimation");

            isInteractable = false;
            parentPuzzle.isCompleted = true;
            parentPuzzle.PuzzleCompleted();
        }
    }
}
