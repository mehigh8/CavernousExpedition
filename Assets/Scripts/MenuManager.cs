using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField seedInput;
    public Button startButton;
    public Button optionsButton;
    public Button exitButton;
    public Button backButton;
    public RawImage preview;
    public Slider sfxSlider;
    public Slider mouseSlider;
    public GameObject menuUI;
    public GameObject optionsUI;
    [Header("References")]
    public ComputeShader textureViewShader;

    private TerrainGenerator generator;
    private RenderTexture texture;
    private ComputeBuffer buffer;

    private void OnEnable()
    {
        seedInput.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        mouseSlider.onValueChanged.RemoveAllListeners();
        startButton.onClick.RemoveAllListeners();
        optionsButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();

        if (PlayerPrefs.HasKey("sfx"))
            sfxSlider.value = PlayerPrefs.GetFloat("sfx");
        else
        {
            sfxSlider.value = 1f;
            PlayerPrefs.SetFloat("sfx", 1f);
        }

        if (PlayerPrefs.HasKey("mouse"))
            mouseSlider.value = PlayerPrefs.GetFloat("mouse");
        else
        {
            mouseSlider.value = 0.5f;
            PlayerPrefs.SetFloat("mouse", 0.5f);
        }

        seedInput.onValueChanged.AddListener(delegate { EditSeed(); });
        sfxSlider.onValueChanged.AddListener(delegate { ChangeSFX(); });
        mouseSlider.onValueChanged.AddListener(delegate { ChangeSensitivity(); });
        startButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(OpenOptions);
        exitButton.onClick.AddListener(ExitGame);
        backButton.onClick.AddListener(Back);
    }

    void Start()
    {
        generator = GameObject.Find("TerrainGenerator").GetComponent<TerrainGenerator>();
        EditSeed();
    }

    public void EditSeed()
    {
        SeedManager seedManager = SeedManager.GetInstance();

        if (seedInput.text.Equals(""))
            seedManager.seed = Random.Range(1, int.MaxValue).ToString();
        else
            seedManager.seed = seedInput.text;

        MapGenerator mapGenerator = new MapGenerator(seedManager.seed, generator.mapSize, generator.caveSizeThreshold, generator.tunnelSize);
        mapGenerator.GenerateMap(generator.generationSteps, generator.fillPercentage, generator.neighborThreshold);

        texture = new RenderTexture(generator.mapSize.x, generator.mapSize.y, 0);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat;
        texture.enableRandomWrite = true;
        texture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;

        textureViewShader.SetTexture(0, "tex", texture);

        int[,] map = mapGenerator.GetMap();
        Vector2Int mapSize = generator.mapSize;

        buffer = new ComputeBuffer(mapSize.x * mapSize.y, sizeof(int));
        buffer.SetData(map);

        textureViewShader.SetBuffer(0, "map", buffer);
        textureViewShader.SetInts("mapSize", mapSize.x, mapSize.y);
        textureViewShader.Dispatch(0, Mathf.CeilToInt(mapSize.x / 8f), Mathf.CeilToInt(mapSize.y / 8f), 1);

        preview.texture = texture;

        buffer.Release();
    }

    public void ChangeSFX()
    {
        PlayerPrefs.SetFloat("sfx", sfxSlider.value);
    }

    public void ChangeSensitivity()
    {
        PlayerPrefs.SetFloat("mouse", mouseSlider.value);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenOptions()
    {
        menuUI.SetActive(false);
        optionsUI.SetActive(true);
    }

    public void Back()
    {
        menuUI.SetActive(true);
        optionsUI.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
