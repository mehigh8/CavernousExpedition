using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SymbolOrderingButton : MonoBehaviour, IInteractable
{
    public string interactionPrompt { get => prompts[currentPrompt]; }
    public bool isInteractable { get; set; }

    [Header("Config")]
    public string[] prompts;
    public TMP_Text text;
    public string letter;
    public SymbolOrderingPuzzle parentPuzzle = null;
    public Animator animator;
    public AudioSource audioSource;

    public int index;
    private int currentPrompt;

    void Start()
    {
        parentPuzzle = GetComponentInParent<SymbolOrderingPuzzle>();
        isInteractable = true;
        currentPrompt = 0;
    }

    private void Update()
    {
        if (parentPuzzle)
        {
            isInteractable = !parentPuzzle.isCompleted;
            currentPrompt = parentPuzzle.isSelected ? 1 : 0;
        }
    }

    public void SetLetter(string letter, int index)
    {
        text.text = letter;
        this.letter = letter;

        this.index = index;
    }

    public void Interact(Interactor interactor)
    {
        if (parentPuzzle == null || parentPuzzle.isCompleted)
            return;

        audioSource.volume = PlayerPrefs.GetFloat("sfx");
        audioSource.Play();

        if (!parentPuzzle.isSelected)
        {
            animator.Play("ButtonStayPressed");
            parentPuzzle.selectedButton = this;
            parentPuzzle.isSelected = true;
        }
        else
        {
            if (parentPuzzle.selectedButton.index == index)
                return;

            animator.Play("ButtonPressedAnimation");
            parentPuzzle.selectedButton.animator.Play("ButtonUnpress");

            Helpers.SwapArrayElements(parentPuzzle.currentOrder, parentPuzzle.selectedButton.index, index);
            SetLetter(parentPuzzle.currentOrder[index], index);
            parentPuzzle.selectedButton.SetLetter(parentPuzzle.currentOrder[parentPuzzle.selectedButton.index], parentPuzzle.selectedButton.index);

            parentPuzzle.isSelected = false;
            parentPuzzle.CheckSymbols();
        }
    }

}
