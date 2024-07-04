using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightController : MonoBehaviour
{
    [Header("Config")]
    public float minIntensity;
    public float maxIntensity;
    public float intensityDecreaseRate;
    public float batteryIncreaseValue;
    [Header("References")]
    public Light lightSource;
    public Slider lightBar;

    [SerializeField] private float intensityLevel = 1f;

    private void Update()
    {
        lightBar.value = intensityLevel;
    }

    private void FixedUpdate()
    {
        if (GameManager.GetInstance().pauseManager.isPaused)
            return;

        intensityLevel -= intensityDecreaseRate;
        if (intensityLevel < 0f)
            intensityLevel = 0f;

        lightSource.range = minIntensity + (maxIntensity - minIntensity) * intensityLevel;
    }

    public void UseBattery()
    {
        intensityLevel += batteryIncreaseValue;
        if (intensityLevel > 1f)
            intensityLevel = 1f;
    }

}
