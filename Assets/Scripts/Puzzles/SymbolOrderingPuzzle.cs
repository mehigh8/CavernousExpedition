using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SymbolOrderingPuzzle : Puzzle
{
    [Header("Config")]
    public int minSymbolCount;
    public int maxSymbolCount;
    public float hintLowHeightOffset;
    public float hintHighHeightOffset;
    [Header("References")]
    public SymbolOrderingButton[] buttons;
    public GameObject hint;

    [HideInInspector] public string[] currentOrder;
    [Space]
    public SymbolOrderingButton selectedButton;
    public bool isSelected = false;

    [SerializeField] private int symbolCount;
    private List<string> symbols;

    public override void InitPuzzle()
    {
        symbolCount = Random.Range(minSymbolCount, maxSymbolCount + 1);
        symbols = Helpers.ShuffleList(Helpers.letters.ToList());

        List<Vector3> usedPlaces = new List<Vector3>();

        usedPlaces.Add(PlaceHint(hint, symbols[0] + " is first", new Vector3(0, Random.Range(-hintLowHeightOffset, hintHighHeightOffset)), usedPlaces, groundLayer));
        usedPlaces.Add(PlaceHint(hint, symbols[symbolCount - 1] + " is last", new Vector3(0, Random.Range(-hintLowHeightOffset, hintHighHeightOffset)), usedPlaces, groundLayer));

        for (int i = 1; i < symbolCount - 2; i++)
        {
            usedPlaces.Add(PlaceHint(hint, Random.value < 0.5f ? symbols[i] + " is before " + symbols[i + 1] : symbols[i + 1] + " is after " + symbols[i], new Vector3(0, Random.Range(-hintLowHeightOffset, hintHighHeightOffset)), usedPlaces, groundLayer));
        }

        buttons = GetComponentsInChildren<SymbolOrderingButton>();

        for (int i = symbolCount; i < maxSymbolCount; i++)
        {
            buttons[i].enabled = false;
            buttons[i].gameObject.SetActive(false);
        }

        currentOrder = new string[symbolCount];
        for (int i = 0; i < symbolCount; i++)
            currentOrder[i] = symbols[i];

        bool isDifferent = false;
        do
        {
            currentOrder = Helpers.ShuffleList(currentOrder.ToList()).ToArray();
            for (int i = 0; i < symbolCount; i++)
                if (!currentOrder[i].Equals(symbols[i]))
                {
                    isDifferent = true;
                    break;
                }
        } while (!isDifferent);

        for (int i = 0; i < symbolCount; i++)
            buttons[i].SetLetter(currentOrder[i], i);
    }

    public void CheckSymbols()
    {
        for (int i = 0; i < symbolCount; i++)
            if (!currentOrder[i].Equals(symbols[i]))
                return;

        isCompleted = true;
        PuzzleCompleted();
    }
}
