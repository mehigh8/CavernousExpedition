
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour, IInteractable
{
    public string interactionPrompt { get => "Hit crystal"; }

    public bool isInteractable { get; set; }

    [Header("Config")]
    public float pitchBase = 0.5f;
    public CrystalsResonancePuzzle parentPuzzle = null;
    public AudioSource audioSource = null;
    [SerializeField] private float pitch;
    private AudioClip crystalSound;

    void Start()
    {
        parentPuzzle = GetComponentInParent<CrystalsResonancePuzzle>();
        audioSource = GetComponentInParent<AudioSource>();
    }

    void Update()
    {
        if (parentPuzzle)
            isInteractable = !parentPuzzle.isCompleted;
    }

    public void Configure(float pitch, AudioClip crystalSound)
    {
        this.pitch = pitch + pitchBase;
        this.crystalSound = crystalSound;   
    }

    public void Interact(Interactor interactor)
    {
        if (parentPuzzle == null || parentPuzzle.isCompleted)
            return;

        if (audioSource)
        {
            audioSource.volume = PlayerPrefs.GetFloat("sfx");
            audioSource.clip = crystalSound;
            audioSource.pitch = pitch;
            audioSource.Play();
        }

        parentPuzzle.CrystalHit(pitch - 0.5f);
    }
}
