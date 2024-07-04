using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    [Header("Config")]
    public float terrainHeight;
    public float timeToOpen;

    public void OpenGate()
    {
        StartCoroutine(OpenGateCoroutine());
    }

    private IEnumerator OpenGateCoroutine()
    {
        float startTime = Time.realtimeSinceStartup;
        float currentTime = startTime;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position - new Vector3(0, terrainHeight, 0);

        while (currentTime - startTime < timeToOpen)
        {
            currentTime = Time.realtimeSinceStartup;
            transform.position = Vector3.Lerp(startPosition, endPosition, (currentTime - startTime) / timeToOpen);
            yield return null;
        }

        Destroy(gameObject);
    }
}
