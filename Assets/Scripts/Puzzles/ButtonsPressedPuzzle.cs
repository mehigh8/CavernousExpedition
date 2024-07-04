using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonsPressedPuzzle : Puzzle
{
    [Header("Config")]
    public int minNumberOfButtons;
    public int maxNumberOfButtons;
    public Color indicatorOffColor;
    public Color indicatorOnColor;
    [Header("References")]
    public GameObject pressurePlatePrefab;
    public List<GameObject> pickablePrefabs;
    public MeshRenderer[] indicators;

    private List<PressurePlate> pressurePlates = new List<PressurePlate>();
    [SerializeField] private int numberOfButtons;

    void Update()
    {
        if (!isCompleted)
        {
            isCompleted = ButtonsPressed();
            if (isCompleted)
                PuzzleCompleted();
        }
    }

    private bool ButtonsPressed()
    {
        bool result = true;
        foreach (PressurePlate pressurePlate in pressurePlates)
            if (pressurePlate.isPressed == false)
                result = false;

        for (int i = 0; i < numberOfButtons; i++)
            if (pressurePlates[i].isPressed)
                indicators[i].material.color = indicatorOnColor;
            else
                indicators[i].material.color = indicatorOffColor;

        return result;
    }

    override public void InitPuzzle()
    {
        numberOfButtons = Random.Range(minNumberOfButtons, maxNumberOfButtons + 1);

        for (int i = 0; i < numberOfButtons; i++)
        {
            Vector2 chosenPoint = Helpers.GetRandomElement(region.points);
            while (GameManager.GetInstance().usedPoints.Contains(chosenPoint))
                chosenPoint = Helpers.GetRandomElement(region.points);

            GameManager.GetInstance().usedPoints.Add(chosenPoint);

            if (Physics.Raycast(terrainGenerator.MapToWorld(chosenPoint), Vector3.down, out RaycastHit hit, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
            {
                GameObject pressurePlate = Instantiate(pressurePlatePrefab, hit.point, Quaternion.identity, transform);
                if (pressurePlate.GetComponent<PlatePositionChecker>().CheckPosition() == false)
                {
                    Destroy(pressurePlate);
                    i--;
                    continue;
                }
                pressurePlates.Add(pressurePlate.GetComponent<PressurePlate>());
            }
        }

        for (int i = numberOfButtons; i < maxNumberOfButtons; i++)
            indicators[i].gameObject.SetActive(false);

        for (int i = 0; i < numberOfButtons - 1; i++)
        {
            Vector2Int chosenPoint = Helpers.GetRandomElement(region.points);
            if (Physics.Raycast(terrainGenerator.MapToWorld(chosenPoint), Vector3.down, out RaycastHit hit, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
                Instantiate(Helpers.GetRandomElement(pickablePrefabs), hit.point + Vector3.up, Helpers.RandomQuaternion());
        }
    }
}
