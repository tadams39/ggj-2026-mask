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
            Destroy(prefabInstance.gameObject);
        }
    }

    public void Spawn()
    {
        // Get the spawn multiplier based on elapsed game time
        float spawnMultiplier = LevelGenerator.instance != null
            ? LevelGenerator.instance.GetSpawnMultiplier(type)
            : 1f;

        // Apply multiplier to base chance
        float effectiveChance = baseChance * spawnMultiplier;

        if (Random.Range(0f, 1f) < effectiveChance)
        {
            var prefabOptions = LevelGenerator.instance.GetPrefabsOfType(type);
            if (prefabOptions.Length == 0) return;

            prefabInstance = Instantiate(prefabOptions[Random.Range(0, prefabOptions.Length)], transform.position, transform.rotation, transform);
            prefabInstance.transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            prefabInstance.gameObject.SetActive(true);
        }
    }
}
