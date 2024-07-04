using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [Header("Config")]
    public float viewRange;
    [Header("References")]
    public ComputeShader minimapShader;
    public Transform player;

    private TerrainGenerator terrainGenerator = null;
    private Vector2Int mapSize;

    private RawImage minimapImage;
    private RenderTexture minimapTexture;

    private int[,] map; // Original map
    private int[,] seen; // How much the player saw

    private ComputeBuffer mapBuffer;
    private ComputeBuffer seenBuffer;

    void Start()
    {
        GameObject tg = GameObject.Find("TerrainGenerator");

        if (tg == null)
        {
            Debug.LogError("Can't find TerrainGenerator object");
            return;
        }
        
        terrainGenerator = tg.GetComponent<TerrainGenerator>();
        mapSize = terrainGenerator.mapSize;
        map = terrainGenerator.mapGenerator.GetMap();
        seen = new int[mapSize.x, mapSize.y];

        minimapImage = GetComponent<RawImage>();

        minimapTexture = new RenderTexture(mapSize.x, mapSize.y, 0);
        minimapTexture.filterMode = FilterMode.Point;
        minimapTexture.wrapMode = TextureWrapMode.Clamp;
        minimapTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        minimapTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat;
        minimapTexture.enableRandomWrite = true;

        minimapImage.texture = minimapTexture;
    }

    void Update()
    {
        if (terrainGenerator)
        {
            Vector2 mapPlayerPosition = terrainGenerator.WorldToMap(player.position);

            for (int i = 0; i < mapSize.x; i++)
                for (int j = 0; j < mapSize.y; j++)
                    if (Vector2.Distance(new Vector2(i, j), mapPlayerPosition) < viewRange)
                        seen[i, j] = 1;

            mapBuffer = new ComputeBuffer(map.Length, sizeof(int));
            mapBuffer.SetData(map);
            seenBuffer = new ComputeBuffer(seen.Length, sizeof(int));
            seenBuffer.SetData(seen);

            minimapShader.SetTexture(0, "minimapTexture", minimapTexture);
            minimapShader.SetBuffer(0, "map", mapBuffer);
            minimapShader.SetBuffer(0, "seen", seenBuffer);
            minimapShader.SetInts("mapSize", mapSize.x, mapSize.y);
            minimapShader.SetFloats("player", mapPlayerPosition.x, mapPlayerPosition.y);
            minimapShader.Dispatch(0, Mathf.CeilToInt(mapSize.x / 8), Mathf.CeilToInt(mapSize.y / 8), 1);

            mapBuffer.Release();
            seenBuffer.Release();
        }
    }
}
