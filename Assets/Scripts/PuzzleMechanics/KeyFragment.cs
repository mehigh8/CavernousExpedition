using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyFragment : MonoBehaviour, IInteractable
{
    public string interactionPrompt { get => "Pick up key fragment"; }
    public bool isInteractable { get; set; }
    [Header("Config")]
    public KeyAssemblingPuzzle parentPuzzle = null;
    public int index;

    private MeshRenderer[] meshRenderers = null;
    private Collider col;
    private Rigidbody rb;

    void Start()
    {
        parentPuzzle = GetComponentInParent<KeyAssemblingPuzzle>();
        if (meshRenderers == null)
            meshRenderers = GetComponentsInChildren<MeshRenderer>();

        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        isInteractable = true;
    }

    public void Interact(Interactor interactor)
    {
        if (parentPuzzle == null)
            return;

        parentPuzzle.pickedKeyFragments.Add(gameObject);
        
        Color color = meshRenderers[0].material.color;
        color.a = 0f;
        SetColor(color);

        if (col)
        {
            Destroy(col);
            col = null;
        }

        if (rb)
        {
            Destroy(rb);
            rb = null;
        }
    }

    public void SetColor(Color color)
    {
        if (meshRenderers == null)
            meshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer meshRenderer in meshRenderers)
            meshRenderer.material.color = color;
    }
}
