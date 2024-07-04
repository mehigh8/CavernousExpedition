using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoButton : MonoBehaviour, IInteractable
{
    public string interactionPrompt { get => "Show info about puzzle"; }

    public bool isInteractable { get; set; }

    [Header("Config")]
    public string puzzleTitle;
    [TextArea(3, 5)]
    public string puzzleDescription;
    public Animator animator;
    public AudioSource audioSource;

    private void Start()
    {
        isInteractable = true;
    }

    public void Interact(Interactor interactor)
    {
        GameManager.GetInstance().pauseManager.ShowInfo(puzzleTitle, puzzleDescription);
        animator.Play("ButtonPressedAnimation");
        audioSource.volume = PlayerPrefs.GetFloat("sfx");
        audioSource.Play();
    }
}
