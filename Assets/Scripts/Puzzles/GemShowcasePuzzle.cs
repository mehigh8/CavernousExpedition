using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemShowcasePuzzle : Puzzle
{
    [System.Serializable]
    public struct GemInfo
    {
        public GameObject prefab;
        public Texture image;
    }

    [Header("Config")]
    public int minGemCount;
    public int maxGemCount;
    public Vector3 pillarSpawnOffset;
    public int maxGemSpawns;
    [Header("References")]
    public List<GemInfo> gems;
    public GameObject pillarPrefab;

    [SerializeField] private int gemCount;

    private List<ItemShowcase> itemShowcases = new List<ItemShowcase>();

    void Update()
    {
        if (!isCompleted)
        {
            isCompleted = ItemsPlaced();
            if (isCompleted)
                PuzzleCompleted();
        }
    }

    public override void InitPuzzle()
    {
        gemCount = Random.Range(minGemCount, maxGemCount + 1);

        for (int i = 0; i < gemCount; i++)
        {
            GemInfo gemInfo = Helpers.GetRandomElement(gems);
            GameObject gem = gemInfo.prefab;
            Color gemColor = Random.ColorHSV();
            gemColor.a = 1f;

            bool validPlace = false;
            while (!validPlace)
            {
                Vector2 pillarPlace = Helpers.GetRandomElement(region.points);
                if (!GameManager.GetInstance().usedPoints.Contains(pillarPlace))
                {
                    if (Physics.Raycast(terrainGenerator.MapToWorld(pillarPlace), Vector3.down, out RaycastHit hit, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
                    {
                        validPlace = true;
                        GameManager.GetInstance().usedPoints.Add(pillarPlace);

                        GameObject pillar = Instantiate(pillarPrefab, hit.point + pillarSpawnOffset, Quaternion.identity, transform);
                        ItemShowcase itemShowcase = pillar.GetComponent<ItemShowcase>();
                        itemShowcase.expectedItem = gem.name;
                        itemShowcase.SetTexture(gemInfo.image);

                        itemShowcases.Add(itemShowcase);
                    }
                }
            }

            int spawns = Random.Range(1, maxGemSpawns + 1);

            for (int j = 0; j < spawns; j++) {
                validPlace = false;
                while (!validPlace)
                {
                    MapGenerator.Region chosenRegion = Helpers.GetRandomElement(prevRegions);
                    Vector2 gemPlace = Helpers.GetRandomElement(chosenRegion.points);

                    if (!GameManager.GetInstance().usedPoints.Contains(gemPlace))
                    {
                        if (Physics.Raycast(terrainGenerator.MapToWorld(gemPlace), Vector3.down, out RaycastHit hit, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
                        {
                            validPlace = true;
                            GameManager.GetInstance().usedPoints.Add(gemPlace);

                            GameObject spawnedGem = Instantiate(gem, hit.point + Vector3.up, Helpers.RandomQuaternion());
                            spawnedGem.GetComponent<MeshRenderer>().material.color = gemColor;
                        }
                    }
                }
            }
        }
    }

    public bool ItemsPlaced()
    {
        bool result = true;
        foreach (ItemShowcase itemShowcase in itemShowcases)
            if (itemShowcase.isPressed == false)
                result = false;

        return result;
    }
}
