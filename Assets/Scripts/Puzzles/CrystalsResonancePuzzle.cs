using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class CrystalsResonancePuzzle : Puzzle
{
    [Header("Config")]
    public int minCrystalCount;
    public int maxCrystalCount;
    public Vector3 crystalSpawnOffset;
    public float maxCrystalScale;
    [Header("References")]
    public TMP_Text[] keynotes;
    public List<GameObject> crystalPrefabs;
    public AudioClip crystalSound;

    [Space]
    public int crystalCount;

    public List<float> correctPitches;
    private int correctHitsCount = 0;

    public override void InitPuzzle()
    {
        crystalCount = Random.Range(minCrystalCount, maxCrystalCount + 1);

        crystalPrefabs = Helpers.ShuffleList(crystalPrefabs);

        List<float> pitches = new List<float>();
        float pitchIncrement = 1f / crystalCount;
        for (int i = 0; i < crystalCount; i++)
            pitches.Add(pitchIncrement * i);

        correctPitches = Helpers.ShuffleList(pitches);
        pitches = Helpers.ShuffleList(correctPitches);

        for (int i = 0; i < crystalCount; i++)
        {
            bool validPlace = false;
            while (!validPlace)
            {
                Vector2 chosenPlace = Helpers.GetRandomElement(region.points);
                if (!GameManager.GetInstance().usedPoints.Contains(chosenPlace))
                {
                    GameManager.GetInstance().usedPoints.Add(chosenPlace);
                    if (Physics.Raycast(terrainGenerator.MapToWorld(chosenPlace), Vector3.down, out RaycastHit hit, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
                    {
                        validPlace = true;

                        GameObject instantiatedCrystal = Instantiate(crystalPrefabs[i], hit.point + crystalSpawnOffset, Quaternion.identity, transform);
                        instantiatedCrystal.transform.Rotate(new Vector3(0, Random.Range(-90f, 90f), 0));
                        float scale = Random.Range(1f, maxCrystalScale);
                        instantiatedCrystal.transform.localScale = new Vector3(scale, scale, scale);

                        Crystal[] crystalScripts = instantiatedCrystal.GetComponentsInChildren<Crystal>();
                        foreach (Crystal crystalScript in crystalScripts)
                            crystalScript.Configure(pitches[i], crystalSound);
                    }
                }
            }
        }

        keynotes = GetComponentsInChildren<TMP_Text>();

        for (int i = crystalCount; i < maxCrystalCount; i++)
            keynotes[i].gameObject.SetActive(false);

        for (int i = 0; i < crystalCount; i++)
        {
            keynotes[i].transform.position += new Vector3(0, correctPitches[i], 0);
            keynotes[i].color = Color.red;
        }
    }

    public void CrystalHit(float pitch, bool recall = true)
    {
        if (Mathf.Abs(correctPitches[correctHitsCount] - pitch) < 0.0001f)
        {
            keynotes[correctHitsCount].color = Color.green;
            correctHitsCount++;
            
            if (correctHitsCount == crystalCount)
            {
                isCompleted = true;
                PuzzleCompleted();
            }
        }
        else
        {
            correctHitsCount = 0;
            foreach (TMP_Text keynote in keynotes)
                keynote.color = Color.red;

            if (recall)
                CrystalHit(pitch, false);
        }
    }
}
