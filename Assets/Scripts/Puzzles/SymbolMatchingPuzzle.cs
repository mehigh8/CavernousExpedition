using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class SymbolMatchingPuzzle : Puzzle
{
    public static Color[] symbolColors = { Color.red, Color.blue, Color.yellow, Color.green, Color.cyan, Color.magenta };
    [Header("Config")]
    public int minSymbolCount;
    public int maxSymbolCount;
    public float hintLowHeightOffset;
    public float hintHighHeightOffset;
    [Header("References")]
    public SymbolMatchingButton[] buttons;
    public GameObject hint;

    [HideInInspector] public List<Color> leftColumnColors;
    [HideInInspector] public List<Color> rightColumnColors;

    [Space]
    [SerializeField] private int symbolCount;
    private List<string> symbols;

    public override void InitPuzzle()
    {
        symbolCount = Random.Range(minSymbolCount, maxSymbolCount + 1) * 2;
        symbols = Helpers.ShuffleList(Helpers.letters.ToList());

        List<Vector3> usedPlaces = new List<Vector3>();
        
        for (int i = 0; i < symbolCount; i += 2) 
        {
            usedPlaces.Add(PlaceHint(hint, Random.value < 0.5f ? symbols[i] + " - " + symbols[i + 1] : symbols[i + 1] + " - " + symbols[i], new Vector3(0, Random.Range(-hintLowHeightOffset, hintHighHeightOffset), 0), usedPlaces, groundLayer));
        }

        buttons = GetComponentsInChildren<SymbolMatchingButton>();

        for (int i = symbolCount; i < maxSymbolCount * 2; i++)
        {
            buttons[i].enabled = false;
            buttons[i].gameObject.SetActive(false);
        }

        List<string> leftColumn = new List<string>();
        List<string> rightColumn = new List<string>();

        for (int i = 0; i < symbolCount; i++)
        {
            if (i % 2 == 0)
                leftColumn.Add(symbols[i]);
            else
                rightColumn.Add(symbols[i]);
        }

        leftColumn = Helpers.ShuffleList(leftColumn);
        rightColumn = Helpers.ShuffleList(rightColumn);

        for (int i = 0; i < symbolCount; i++)
        {
            if (i % 2 == 0)
                buttons[i].SetLetter(leftColumn[i / 2]);
            else
                buttons[i].SetLetter(rightColumn[i / 2]);
        }
    }

    public void CheckSymbols()
    {
        for (int i = 0; i < symbolCount; i += 2)
        {
            Color color1 = Color.white;
            Color color2 = Color.white;

            for (int j = 0; j < symbolCount; j += 2)
                if (buttons[j].letter.Equals(symbols[i]))
                    color1 = buttons[j].currentColor;

            for (int j = 1; j < symbolCount; j += 2)
                if (buttons[j].letter.Equals(symbols[i + 1]))
                    color2 = buttons[j].currentColor;

            if (color1 != color2 || color1.Equals(Color.white) || color2.Equals(Color.white))
                return;
        }

        isCompleted = true;
        PuzzleCompleted();
    }
}
