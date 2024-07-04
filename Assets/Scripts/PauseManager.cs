using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    public GameObject pauseBackground;
    public GameObject infoUI;
    public GameObject pauseUI;
    public GameObject endGameUI;
    [Header("Info")]
    public TMP_Text infoTitle;
    public TMP_Text infoText;
    public Button closeButton;
    [Header("Pause")]
    public Button resumeButton;
    public Button restartButton;
    public Button menuButton;
    public Slider sfxSlider;
    public Slider mouseSlider;
    [Header("End game")]
    public Button endMenuButton;

    [Space]
    public bool isPaused = false;

    private void OnEnable()
    {
        closeButton.onClick.RemoveAllListeners();
        resumeButton.onClick.RemoveAllListeners();
        restartButton.onClick.RemoveAllListeners();
        menuButton.onClick.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        mouseSlider.onValueChanged.RemoveAllListeners();
        endMenuButton.onClick.RemoveAllListeners();

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

        closeButton.onClick.AddListener(CloseAll);
        resumeButton.onClick.AddListener(CloseAll);
        restartButton.onClick.AddListener(Restart);
        menuButton.onClick.AddListener(Menu);
        sfxSlider.onValueChanged.AddListener(delegate { ChangeSFX(); });
        mouseSlider.onValueChanged.AddListener(delegate { ChangeSensitivity(); });
        endMenuButton.onClick.AddListener(Menu);
    }

    private void Start()
    {
        ChangeSensitivity();
    }

    void Update()
    {
        if (GameManager.GetInstance().finishedGame)
        {
            CloseAll();
            FinishGame();
            return;
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                ShowPause();
            else
                CloseAll();
        }
    }

    public void ShowInfo(string title, string text)
    {
        isPaused = true;

        infoTitle.text = title;
        infoText.text = text;

        pauseBackground.SetActive(true);
        infoUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowPause()
    {
        isPaused = true;

        pauseBackground.SetActive(true);
        pauseUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseAll()
    {
        isPaused = false;

        pauseBackground.SetActive(false);
        infoUI.SetActive(false);
        pauseUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void FinishGame()
    {
        isPaused = true;

        pauseBackground.SetActive(true);
        endGameUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Menu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void ChangeSFX()
    {
        PlayerPrefs.SetFloat("sfx", sfxSlider.value);
    }

    public void ChangeSensitivity()
    {
        PlayerPrefs.SetFloat("mouse", mouseSlider.value);
        GameManager.GetInstance().cameraController.ChangeSensitivity(mouseSlider.value);
    }
}
