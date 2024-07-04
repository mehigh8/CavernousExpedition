using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public struct VertexInfo
    {
        public Vector3 position;
        public Vector3 normal;
    }

    [Header("Map config")]
    public Vector2Int mapSize;
    public int generationSteps;
    public int fillPercentage;
    public int neighborThreshold;
    public int caveSizeThreshold;
    public float tunnelSize;
    [Header("World config")]
    public int height;
    public float worldSizeMultiplier;
    [Header("Noise")]
    public float noiseFloorHeight;
    public float noiseCeilingHeight;
    [Header("Material Config")]
    public Material terrainMaterial;
    public float floorHeightDelimiter;
    public float ceilHeightDelimiter;
    public float noiseTextureScale;
    [Header("Random config")]
    public string seed;
    [Header("References")]
    public ComputeShader map3DGenerator;
    public ComputeShader marchingCubes;
    public GameManager gameManager;
    [Space]
    public bool inMenu = false;
    [HideInInspector] public RenderTexture mapTexture;

    [HideInInspector] public MapGenerator mapGenerator;

    private float[,] floorNoise;
    private float[,] ceilingNoise;

    private ComputeBuffer floorNoiseBuffer;
    private ComputeBuffer ceilingNoiseBuffer;
    private ComputeBuffer map2DBuffer;
    // The triangle buffer is a list of vertices
    private ComputeBuffer triangleBuffer;
    // Buffer used to get the count of the triangleBuffer
    private ComputeBuffer countBuffer;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private Dictionary<VertexInfo, int> vertexDictionary;
    private List<Vector3> meshVertices;
    private List<Vector3> meshNormals;
    private List<int> meshTriangles;

    private int maxVertexCount;
    private int vertexCount;
    private VertexInfo[] vertexArray;
    private int triangleCount;

    void Awake()
    {
        seed = SeedManager.GetInstance().seed;

        GenerateMap();
        CreateMesh();
        if (!inMenu)
        {
            gameManager.InitGameManager(mapGenerator.GetCaves()[0], this);
            gameManager.enabled = true;
        }
        else
        {
            Camera.main.transform.position = MapToWorld(Helpers.GetRandomElement(mapGenerator.GetCaves()[0].points));
        }
    }

    void Update()
    {
        //Debug.Log((int)(1f / Time.unscaledDeltaTime));
        terrainMaterial.SetFloat("minHeight", -height / 2 * worldSizeMultiplier + floorHeightDelimiter * worldSizeMultiplier);
        terrainMaterial.SetFloat("maxHeight", height / 2 * worldSizeMultiplier - ceilHeightDelimiter * worldSizeMultiplier);
        terrainMaterial.SetFloat("textureScale", noiseTextureScale);
    }

    void GenerateMap()
    {
        mapGenerator = new MapGenerator(seed, mapSize, caveSizeThreshold, tunnelSize);
        mapGenerator.GenerateMap(generationSteps, fillPercentage, neighborThreshold);

        floorNoise = mapGenerator.GenerateNoiseMap(seed.GetHashCode() % 256, 10f);
        ceilingNoise = mapGenerator.GenerateNoiseMap(seed.GetHashCode().ToString().GetHashCode() % 256, 10f);

        mapTexture = new RenderTexture(mapSize.x, height, 0);
        mapTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
        mapTexture.volumeDepth = mapSize.y;
        mapTexture.enableRandomWrite = true;
        mapTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        mapTexture.wrapMode = TextureWrapMode.Repeat;
        mapTexture.filterMode = FilterMode.Bilinear;
        mapTexture.name = "mapTexture";

        mapTexture.Create();

        map2DBuffer = new ComputeBuffer(mapSize.x * mapSize.y, sizeof(int));
        map2DBuffer.SetData(mapGenerator.GetMap());

        floorNoiseBuffer = new ComputeBuffer(mapSize.x * mapSize.y, sizeof(float));
        floorNoiseBuffer.SetData(floorNoise);
        ceilingNoiseBuffer = new ComputeBuffer(mapSize.x * mapSize.y, sizeof(float));
        ceilingNoiseBuffer.SetData(ceilingNoise);

        map3DGenerator.SetBuffer(0, "map2d", map2DBuffer);
        map3DGenerator.SetInts("mapSize", mapSize.x, mapSize.y);
        map3DGenerator.SetBuffer(0, "floorNoise", floorNoiseBuffer);
        map3DGenerator.SetBuffer(0, "ceilingNoise", ceilingNoiseBuffer);
        map3DGenerator.SetTexture(0, "map3d", mapTexture);
        map3DGenerator.SetInt("height", height);
        map3DGenerator.SetFloats("worldSize", mapSize.x * worldSizeMultiplier, height * worldSizeMultiplier, mapSize.y * worldSizeMultiplier);
        map3DGenerator.SetFloat("noiseCeilingHeight", noiseCeilingHeight);
        map3DGenerator.SetFloat("noiseFloorHeight", noiseFloorHeight);

        map3DGenerator.Dispatch(0, Mathf.CeilToInt(mapSize.x / 8f), Mathf.CeilToInt(height / 8f), Mathf.CeilToInt(mapSize.y / 8f));

        map2DBuffer.Release();
        floorNoiseBuffer.Release();
        ceilingNoiseBuffer.Release();
    }

    void CreateMesh()
    {
        vertexDictionary = new Dictionary<VertexInfo, int>();
        meshVertices = new List<Vector3>();
        meshNormals = new List<Vector3>();
        meshTriangles = new List<int>();

        // Each voxel (cube) has a maximum of 12 vertices (one on each edge).
        maxVertexCount = (mapSize.x - 1) * (mapSize.y - 1) * (height - 1) * 12;

        Mesh mesh = new Mesh();
        // mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        meshRenderer.material = terrainMaterial;
        meshFilter.mesh = mesh;

        triangleBuffer = new ComputeBuffer(maxVertexCount * 3, sizeof(float) * 6, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);

        marchingCubes.SetInts("mapSize", mapSize.x, mapSize.y);
        marchingCubes.SetFloats("worldSize", mapSize.x * worldSizeMultiplier, height * worldSizeMultiplier, mapSize.y * worldSizeMultiplier);
        marchingCubes.SetInt("height", height);
        marchingCubes.SetBuffer(0, "triangles", triangleBuffer);
        marchingCubes.SetTexture(0, "mapTexture", mapTexture);

        marchingCubes.Dispatch(0, Mathf.CeilToInt(mapSize.x / 8f), Mathf.CeilToInt(height / 8f), Mathf.CeilToInt(mapSize.y / 8f));

        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        // To get the count of the triangle buffer an array must be used
        int[] countArray = new int[1];
        countBuffer.SetData(countArray);
        ComputeBuffer.CopyCount(triangleBuffer, countBuffer, 0);
        countBuffer.GetData(countArray);

        // The count value is in terms of triangles. Each triangle has 3 vertices.
        vertexCount = countArray[0] * 3;
        vertexArray = new VertexInfo[vertexCount];

        triangleBuffer.GetData(vertexArray, 0, 0, vertexCount);

        triangleCount = 0;
        foreach (VertexInfo vertex in vertexArray)
        {
            int existingIndex;
            if (vertexDictionary.TryGetValue(vertex, out existingIndex))
            {
                meshTriangles.Add(existingIndex);
            }
            else
            {
                vertexDictionary.Add(vertex, triangleCount);
                meshVertices.Add(vertex.position);
                meshNormals.Add(vertex.normal);
                meshTriangles.Add(triangleCount);
                triangleCount++;
            }
        }

        meshCollider.sharedMesh = null;

        mesh.Clear();
        mesh.SetVertices(meshVertices);
        mesh.SetTriangles(meshTriangles, 0, true);
        //mesh.RecalculateNormals();
        mesh.SetNormals(meshNormals);

        meshCollider.sharedMesh = mesh;

        countBuffer.Release();
        triangleBuffer.Release();
    }

    public Vector3 MapToWorld(Vector2 mapPoint)
    {
        return new Vector3((mapPoint.x / mapSize.x - 0.5f) * worldSizeMultiplier * mapSize.x, 0, (mapPoint.y / mapSize.y - 0.5f) * worldSizeMultiplier * mapSize.y);
    }

    public Vector2 WorldToMap(Vector3 worldPoint)
    {
        return new Vector2((worldPoint.x / mapSize.x / worldSizeMultiplier + 0.5f) * mapSize.x, (worldPoint.z / mapSize.y / worldSizeMultiplier + 0.5f) * mapSize.y);
    }
}
