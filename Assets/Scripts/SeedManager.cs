using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedManager : MonoBehaviour
{
    public string seed;

    // Singleton
    private static SeedManager instance;

    private void OnEnable()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        seed = Random.Range(1, int.MaxValue).ToString();
    }

    public static SeedManager GetInstance()
    {
        return instance;
    }
}
