using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureViewer : MonoBehaviour
{
    public ComputeShader shader;

    private Material material;
    private TerrainGenerator generator;
    private RenderTexture texture;
    private ComputeBuffer buffer;

    void Start()
    {
        generator = GameObject.Find("TerrainGenerator").GetComponent<TerrainGenerator>();
        material = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            texture = new RenderTexture(generator.mapSize.x, generator.mapSize.y, 0);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32_SFloat;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;

            shader.SetTexture(0, "tex", texture);

            int[,] map = generator.mapGenerator.GetMap();
            Vector2Int mapSize = generator.mapSize;

            buffer = new ComputeBuffer(mapSize.x * mapSize.y, sizeof(int));
            buffer.SetData(map);

            shader.SetBuffer(0, "map", buffer);
            shader.SetInts("mapSize", mapSize.x, mapSize.y);
            shader.Dispatch(0, Mathf.CeilToInt(mapSize.x / 8f), Mathf.CeilToInt(mapSize.y / 8f), 1);

            material.mainTexture = texture;
        }
    }
}
