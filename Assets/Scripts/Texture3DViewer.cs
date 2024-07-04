using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Texture3DViewer : MonoBehaviour
{
    [Range(0, 1)]
    public float sliceDepth;
    Material material;

    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        material.SetFloat("sliceDepth", sliceDepth);
        material.SetTexture("DisplayTexture", FindObjectOfType<TerrainGenerator>().mapTexture);
    }
}
