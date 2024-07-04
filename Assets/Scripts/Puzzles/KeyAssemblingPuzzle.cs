using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyAssemblingPuzzle : Puzzle
{
    [Header("Config")]
    public float keyScale = 0.2f;
    [Header("References")]
    public GameObject[] keyFragmentsPrefabs;

    [HideInInspector] public List<GameObject> pickedKeyFragments;
    [HideInInspector] public Color keyColor;
    [Space]
    public bool pickedKey = false;

    public override void InitPuzzle()
    {
        keyColor = Random.ColorHSV();
        keyColor.a = 1f;

        for (int i = 0; i < 3; i++)
        {
            bool validPoint = false;
            while (!validPoint)
            {
                Vector2 chosenPoint = Helpers.GetRandomElement(region.points);
                if (!GameManager.GetInstance().usedPoints.Contains(chosenPoint))
                {
                    GameManager.GetInstance().usedPoints.Add(chosenPoint);
                    if (Physics.Raycast(terrainGenerator.MapToWorld(chosenPoint), Vector3.down, out RaycastHit hit, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
                    {
                        validPoint = true;

                        GameObject keyFragment = Instantiate(keyFragmentsPrefabs[i], hit.point + Vector3.up, Helpers.RandomQuaternion(), transform);
                        keyFragment.transform.localScale = new Vector3(keyScale, keyScale, keyScale);
                        keyFragment.GetComponent<KeyFragment>().SetColor(keyColor);
                    }
                }
            }
        }
    }
}
