using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Puzzle : MonoBehaviour
{
    [System.Serializable]
    public struct PuzzlePositionChecker
    {
        public GameObject obj;
        public float radius;
    }

    [Header("Generic Puzzle Config")]
    public List<Gate> gates;
    public MapGenerator.Region region;
    public TerrainGenerator terrainGenerator = null;
    public Vector3 spawnOffset;
    public PuzzlePositionChecker[] positionCheckers;
    public LayerMask groundLayer;
    public MeshRenderer completionIndicator;
    public List<MapGenerator.Region> prevRegions;
    public AudioSource audioSource;
    [Space]
    public bool isCompleted = false;


    private void OnEnable()
    {
        terrainGenerator = FindAnyObjectByType<TerrainGenerator>();
        InitPuzzle();
    }

    public abstract void InitPuzzle();

    public void PuzzleCompleted()
    {
        completionIndicator.material.color = Color.green;
        audioSource.volume = PlayerPrefs.GetFloat("sfx");
        audioSource.Play();
        foreach (Gate gate in gates)
            gate.OpenGate();
    }

    public Vector3 PlaceHint(GameObject hint, string hintText, Vector3 offset, List<Vector3> usedPlaces, LayerMask groundLayer)
    {
        bool placedHint = false;
        do
        {
            Vector2Int mapPoint = Helpers.GetRandomElement(region.points);
            Vector3 worldPoint = terrainGenerator.MapToWorld(mapPoint);

            RaycastHit[] hits = new RaycastHit[4];

            for (int i = 0; i < Helpers.neswX.Length; i++)
            {
                Physics.Raycast(worldPoint, new Vector3(Helpers.neswX[i], 0, Helpers.neswY[i]), out hits[i], float.MaxValue, Physics.AllLayers);
            }

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null) continue;

                Vector2 mapHintPoint = terrainGenerator.WorldToMap(hit.point);
                Vector2Int mapHintPointInt = new Vector2Int((int)mapHintPoint.x, (int)mapHintPoint.y);
                if (!region.points.Contains(mapHintPointInt)) continue;

                if (Helpers.LayerInLayerMask(hit.collider.gameObject.layer, groundLayer) && !usedPlaces.Contains(hit.point))
                {
                    GameObject spawnedHint = Instantiate(hint, hit.point, Quaternion.identity, transform);
                    spawnedHint.transform.LookAt(hit.point + hit.normal);
                    spawnedHint.transform.Rotate(new Vector3(90, 0, 0));
                    spawnedHint.transform.position += spawnedHint.transform.up * 0.1f;
                    spawnedHint.transform.position += offset;
                    if (spawnedHint.GetComponent<PlatePositionChecker>().CheckPosition() == false)
                    {
                        Destroy(spawnedHint);
                        continue;
                    }

                    TMP_Text spawnedHintText = spawnedHint.GetComponentInChildren<TMP_Text>();
                    spawnedHintText.text = hintText;

                    placedHint = true;

                    return hit.point;
                }
            }
        } while (placedHint == false);

        return Vector3.zero;
    }

    public bool IsPlacedCorrectly()
    {
        for (int i = 0; i < positionCheckers.Length; i++)
        {
            Collider[] col = Physics.OverlapSphere(positionCheckers[i].obj.transform.position, positionCheckers[i].radius, groundLayer);
            if (col.Length > 0)
                return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < positionCheckers.Length; i++)
            Gizmos.DrawWireSphere(positionCheckers[i].obj.transform.position, positionCheckers[i].radius);
    }
}
