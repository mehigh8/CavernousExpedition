using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SymbolMatchingButton : MonoBehaviour, IInteractable
{
    public string interactionPrompt { get => "Press button to change color"; }
    public bool isInteractable { get; set; }
    [Header("Config")]
    public TMP_Text text;
    public MeshRenderer meshRenderer;
    public SymbolMatchingPuzzle parentPuzzle = null;
    public Color currentColor;
    public string letter;
    public Animator animator;
    public AudioSource audioSource;

    public bool isOnLeft = true;

    private int currentColorIndex = 0;

    private void Start()
    {
        parentPuzzle = GetComponentInParent<SymbolMatchingPuzzle>();
        currentColor = Color.white;
        isInteractable = true;
    }

    private void Update()
    {
        if (parentPuzzle)
            isInteractable = !parentPuzzle.isCompleted;
    }

    public void SetLetter(string letter)
    {
        text.text = letter;
        this.letter = letter;
    }

    public void Interact(Interactor interactor)
    {
        if (parentPuzzle == null || parentPuzzle.isCompleted)
            return;

        animator.Play("ButtonPressedAnimation");
        audioSource.volume = PlayerPrefs.GetFloat("sfx");
        audioSource.Play();

        if (isOnLeft)
            parentPuzzle.leftColumnColors.Remove(currentColor);
        else
            parentPuzzle.rightColumnColors.Remove(currentColor);

        do
        {
            currentColor = SymbolMatchingPuzzle.symbolColors[currentColorIndex];
            currentColorIndex++;

            if (currentColorIndex == SymbolMatchingPuzzle.symbolColors.Length)
                currentColorIndex = 0;
        } while ((isOnLeft && parentPuzzle.leftColumnColors.Contains(currentColor)) || (!isOnLeft && parentPuzzle.rightColumnColors.Contains(currentColor)));
        
        meshRenderer.material.color = currentColor;
        if (isOnLeft)
            parentPuzzle.leftColumnColors.Add(currentColor);
        else
            parentPuzzle.rightColumnColors.Add(currentColor);

        parentPuzzle.CheckSymbols();
    }
}
