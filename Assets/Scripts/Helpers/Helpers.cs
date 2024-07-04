using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Helpers
{
    public static int[] neighX = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
    public static int[] neighY = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };

    public static int[] neswX = { 0, 1, 0, -1 };
    public static int[] neswY = { -1, 0, 1, 0 };

    public static string[] letters = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
                                      "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
                                      "U", "V", "W", "X", "Y", "Z"};

    public static IEnumerator DelayedPositionChange(Transform whatToMove, Vector3 whereToMove)
    {
        yield return null;
        whatToMove.position = whereToMove;
    }

    public static T GetRandomElement<T> (List<T> list)
    {
        if (list == null)
            return default;

        return list[Random.Range(0, list.Count)];
    }

    public static Quaternion RandomQuaternion()
    {
        return Quaternion.Euler(Random.Range(0f, 90f), Random.Range(0f, 90f), Random.Range(0f, 90f));
    }

    public static bool LayerInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    public static List<T> ShuffleList<T>(List<T> list)
    {
        return list.OrderBy(x => Random.value).ToList();
    }

    public static void SwapArrayElements<T>(T[] arr, int idx1, int idx2)
    {
        T temp = arr[idx1];
        arr[idx1] = arr[idx2];
        arr[idx2] = temp;
    }

    public static List<T> DeepCopyList<T>(List<T> list)
    {
        List<T> copy = new List<T>();
        foreach (T item in list)
            copy.Add(item);

        return copy;
    }
}
