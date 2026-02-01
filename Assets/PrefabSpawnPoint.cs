using UnityEditor;
using UnityEngine;

public class PrefabSpawnPoint : MonoBehaviour
{
    public GamePrefab.PrefabType type;

    public float baseChance = 0.25f;

    public GamePrefab prefabInstance;

    public void Reset()
    {
        if (prefabInstance)
        {
            Destroy(prefabInstance);
        }
    }

    public void Spawn()
    {
        if (Random.Range(0, 1.0f) < baseChance)
        {
            var prefabOptions = LevelGenerator.instance.GetPrefabsOfType(type);
            prefabInstance = Instantiate(prefabOptions[Random.Range(0, prefabOptions.Length)], transform.position, transform.rotation, transform);
            prefabInstance.transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            prefabInstance.gameObject.SetActive(true);
        }
    }

}
