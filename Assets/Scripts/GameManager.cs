using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Puzzles")]
    public List<GameObject> puzzles = new List<GameObject>();
    [Header("Others")]
    public LayerMask groundLayer;
    public GameObject playerPackage;
    public GameObject gatePrefab;
    public GameObject applePrefab;
    public GameObject waterPrefab;
    public GameObject batteryPrefab;
    [Header("References")]
    public CameraController cameraController;
    public PlayerController playerController;
    public LightController lightController;
    public PauseManager pauseManager;


    private MapGenerator.Region startingRegion;
    private TerrainGenerator terrainGenerator;
    private List<Puzzle> generatedPuzzles = new List<Puzzle>();

    private bool initialized = false;

    [HideInInspector] public List<Vector2> usedPoints = new List<Vector2>();

    // Singleton
    private static GameManager instance;

    public bool finishedGame = false;

    void OnEnable()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        List<MapGenerator.Region> processedRegions = new List<MapGenerator.Region>();
        List<MapGenerator.Tunnel> processedTunnels = new List<MapGenerator.Tunnel>();
        Queue<MapGenerator.Region> regionQueue = new Queue<MapGenerator.Region>();
        regionQueue.Enqueue(startingRegion);

        while (regionQueue.Count > 0)
        {
            MapGenerator.Region region = regionQueue.Dequeue();
            processedRegions.Add(region);

            GameObject chosenPuzzle = Helpers.GetRandomElement(puzzles);

            // Spawn puzzle
            while (true)
            {
                Vector2 mapPuzzlePosition = Helpers.GetRandomElement(region.points);
                Vector3 spawnPosition = terrainGenerator.MapToWorld(mapPuzzlePosition);
                usedPoints.Add(mapPuzzlePosition);
                RaycastHit raycastHit;
                if (Physics.Raycast(spawnPosition, Vector3.down, out raycastHit, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
                {
                    Vector3 rotationBasePoint = raycastHit.point + raycastHit.normal;
                    Vector3 bestRotationPointToLookAt = Vector3.zero;
                    float bestDistance = 0f;

                    for (int i = 0; i < Helpers.neswX.Length; i++)
                    {
                        if (Physics.Raycast(rotationBasePoint, new Vector3(Helpers.neswX[i], 0, Helpers.neswY[i]), out RaycastHit rotationHit, float.MaxValue, Physics.AllLayers))
                        {
                            float distance = Vector3.Distance(rotationHit.point, rotationBasePoint);
                            if (distance > bestDistance)
                            {
                                bestDistance = distance;
                                bestRotationPointToLookAt = rotationHit.point;
                            }
                        }
                    }

                    GameObject puzzleObj = Instantiate(chosenPuzzle, raycastHit.point, Quaternion.identity);
                    puzzleObj.transform.LookAt(bestRotationPointToLookAt - raycastHit.normal);

                    Puzzle puzzle = puzzleObj.GetComponent<Puzzle>();
                    puzzleObj.transform.position += puzzle.spawnOffset;
                    if (!puzzle.IsPlacedCorrectly())
                    {
                        Destroy(puzzleObj);
                        continue;
                    }

                    puzzle.region = region;
                    puzzle.prevRegions = Helpers.DeepCopyList(processedRegions);
                    puzzle.gates = new List<Gate>();

                    // Spawn gates
                    foreach (MapGenerator.Tunnel tunnel in region.tunnels)
                    {
                        if (processedTunnels.Contains(tunnel))
                            continue;

                        processedTunnels.Add(tunnel);

                        GameObject gateObj = Instantiate(gatePrefab, terrainGenerator.MapToWorld(tunnel.GetTunnelCenter()), Quaternion.identity);
                        gateObj.transform.LookAt(terrainGenerator.MapToWorld(tunnel.start));
                        gateObj.transform.localScale = new Vector3(terrainGenerator.worldSizeMultiplier * 6, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, 3);

                        Gate gateComponent = gateObj.GetComponent<Gate>();
                        gateComponent.terrainHeight = terrainGenerator.height * terrainGenerator.worldSizeMultiplier;

                        puzzle.gates.Add(gateComponent);
                    }

                    // Spawn apple
                    while (true)
                    {
                        Vector2 chosenPoint = Helpers.GetRandomElement(region.points);
                        if (!usedPoints.Contains(chosenPoint))
                        {
                            usedPoints.Add(chosenPoint);
                            if (Physics.Raycast(terrainGenerator.MapToWorld(chosenPoint), Vector3.down, out RaycastHit hitInfo, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
                            {
                                Instantiate(applePrefab, hitInfo.point + Vector3.up, Helpers.RandomQuaternion(), transform);
                                break;
                            }
                        }
                    }

                    // Spawn water bottle
                    while (true)
                    {
                        Vector2 chosenPoint = Helpers.GetRandomElement(region.points);
                        if (!usedPoints.Contains(chosenPoint))
                        {
                            usedPoints.Add(chosenPoint);
                            if (Physics.Raycast(terrainGenerator.MapToWorld(chosenPoint), Vector3.down, out RaycastHit hitInfo, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
                            {
                                Instantiate(waterPrefab, hitInfo.point + Vector3.up, Helpers.RandomQuaternion(), transform);
                                break;
                            }
                        }
                    }

                    // Spawn battery
                    while (true)
                    {
                        Vector2 chosenPoint = Helpers.GetRandomElement(region.points);
                        if (!usedPoints.Contains(chosenPoint))
                        {
                            usedPoints.Add(chosenPoint);
                            if (Physics.Raycast(terrainGenerator.MapToWorld(chosenPoint), Vector3.down, out RaycastHit hitInfo, terrainGenerator.height * terrainGenerator.worldSizeMultiplier, groundLayer))
                            {
                                Instantiate(batteryPrefab, hitInfo.point + Vector3.up, Helpers.RandomQuaternion(), transform);
                                break;
                            }
                        }
                    }

                    generatedPuzzles.Add(puzzle);
                    break;
                }
            }

            foreach (MapGenerator.Region connectedRegion in region.connectedRegions)
                if (!processedRegions.Contains(connectedRegion))
                    regionQueue.Enqueue(connectedRegion);
        }
    }

    void Start()
    {
        foreach (Puzzle puzzle in generatedPuzzles)
            puzzle.enabled = true;
    }

    private void Update()
    {
        finishedGame = generatedPuzzles.TrueForAll(puzzle => puzzle.isCompleted);
    }

    public void InitGameManager(MapGenerator.Region startingRegion, TerrainGenerator terrainGenerator)
    {
        if (initialized)
            return;

        initialized = true;
        this.startingRegion = startingRegion;
        this.terrainGenerator = terrainGenerator;

        bool validPoint = false;
        while (!validPoint) {
            Vector2 playerPosition = Helpers.GetRandomElement(startingRegion.points);
            usedPoints.Add(playerPosition);

            if (Physics.Raycast(terrainGenerator.MapToWorld(playerPosition), Vector3.down, out RaycastHit hit))
            {
                validPoint = true;
                GameObject player = Instantiate(playerPackage, hit.point + Vector3.up * 3, Quaternion.identity);
                cameraController = player.GetComponentInChildren<CameraController>();
                playerController = player.GetComponentInChildren<PlayerController>();
                lightController = player.GetComponentInChildren<LightController>();
                pauseManager = player.GetComponentInChildren<PauseManager>();
            }
            
        }
    }

    public void CollectedItem(string type)
    {
        if (type.Contains("Apple"))
            playerController.Consume(PlayerController.Food.APPLE);

        if (type.Contains("Bottle"))
            playerController.Consume(PlayerController.Food.WATER);

        if (type.Contains("Baterry"))
            lightController.UseBattery();
    }

    public static GameManager GetInstance()
    {
        return instance;
    }
}
