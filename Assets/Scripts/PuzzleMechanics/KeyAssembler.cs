using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyAssembler : MonoBehaviour, IInteractable
{
    public string interactionPrompt { get => prompts[currentPrompt]; }
    public bool isInteractable { get; set; }
    [Header("Config")]
    public string[] prompts;
    public GameObject[] placements;
    public KeyAssemblingPuzzle parentPuzzle = null;

    [Space]
    [SerializeField] private bool[] currentFragments = new bool[3];
    private int currentPrompt;

    void Start()
    {
        parentPuzzle = GetComponentInParent<KeyAssemblingPuzzle>();
        currentPrompt = 0;
        isInteractable = true;
    }

    void Update()
    {
        currentPrompt = (currentFragments[0] == false || currentFragments[1] == false || currentFragments[2] == false ? 0 : 1);
    }

    public void Interact(Interactor interactor)
    {
        if (parentPuzzle == null)
            return;

        if (currentFragments[0] == false || currentFragments[1] == false || currentFragments[2] == false)
        {
            foreach (GameObject keyFragment in parentPuzzle.pickedKeyFragments)
            {
                KeyFragment keyFragmentScript = keyFragment.GetComponent<KeyFragment>();
                currentFragments[keyFragmentScript.index] = true;

                keyFragment.transform.position = placements[keyFragmentScript.index].transform.position;
                keyFragment.transform.rotation = placements[keyFragmentScript.index].transform.rotation;
                keyFragment.transform.parent = gameObject.transform;
                keyFragmentScript.SetColor(parentPuzzle.keyColor);
            }

            parentPuzzle.pickedKeyFragments.Clear();
        }
        else
        {
            parentPuzzle.pickedKey = true;
            Destroy(gameObject);
        }
    }
}
