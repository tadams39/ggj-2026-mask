
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator instance;
    public static int CHUNK_LOOKAHEAD_COUNT = 4; // number of chunks to load at any time

    public Transform playerTransform;
    public MapChunk[] chunks;
    public GamePrefab[] prefabs;
    public MapChunk[] currentChunks;

    [Header("Spawn Rate Scaling")]
    public float obstacleStartMultiplier = 1f;
    public float obstacleEndMultiplier = 2f;
    public float scoreStartMultiplier = 0.25f;
    public float scoreEndMultiplier = 1.5f;
    public float timeToMaxSpawnRate = 300f; // 5 minutes

    private Dictionary<GamePrefab.PrefabType, GamePrefab[]> prefabsByType;
    private float gameElapsedTime = 0f;

    public void Awake()
    {
        instance = this;
        prefabsByType = prefabs
            .GroupBy(p => p.myType)
            .ToDictionary(
                g => g.Key,
                g => g.ToArray()
            );
    }

    private void Start()
    {
        // Subscribe to game reset to reset the spawn timer
        GameEvents.OnGameReset += HandleGameReset;
    }

    private void OnDestroy()
    {
        GameEvents.OnGameReset -= HandleGameReset;
    }

    private void Update()
    {
        // Track elapsed time for spawn rate scaling
        gameElapsedTime += Time.deltaTime;
    }

    private void HandleGameReset()
    {
        // Reset spawn rate timer on full game reset
        gameElapsedTime = 0f;
    }

    public float GetSpawnMultiplier(GamePrefab.PrefabType type)
    {
        float t = Mathf.Clamp01(gameElapsedTime / timeToMaxSpawnRate);

        switch (type)
        {
            case GamePrefab.PrefabType.Obstacle:
                return Mathf.Lerp(obstacleStartMultiplier, obstacleEndMultiplier, t);

            case GamePrefab.PrefabType.Score:
                return Mathf.Lerp(scoreStartMultiplier, scoreEndMultiplier, t);

            case GamePrefab.PrefabType.Pickup:
                // Pickups stay at base rate (or you can add separate settings)
                return 1f;

            default:
                return 1f;
        }
    }

    public GamePrefab[] GetPrefabsOfType(GamePrefab.PrefabType type)
    {
        if (prefabsByType.TryGetValue(type, out var result))
        {
            return result;
        }
        return new GamePrefab[0];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Reset()
    {
        // Disable all mapChunks on start to hide them
        foreach (MapChunk chunk in chunks)
        {
            chunk.UnloadChunk();
        }

        // Prewarm with a couple of chunks
        for (int i = 0; i < CHUNK_LOOKAHEAD_COUNT; i++)
        {
            ProgressChunks();
        }

        // Move the player to the 1st index start position
        var enterPoint = currentChunks[2].enterAnchor;

        // Calculate spawn position (higher up to prevent falling through)
        Vector3 spawnOffset = Vector3.up * 5f + enterPoint.forward * 2f;
        Vector3 finalPosition = enterPoint.position + spawnOffset;

        Debug.Log($"Spawning player at chunk {currentChunks[2].name}");
        Debug.Log($"Enter anchor position: {enterPoint.position}");
        Debug.Log($"Final spawn position: {finalPosition}");

        playerTransform.GetComponent<SledController>().TeleportTo(enterPoint, spawnOffset);

        Debug.Log($"Player actual position after teleport: {playerTransform.position}");

        foreach( var k in currentChunks[2].GetComponentsInChildren<PlayerTrigger>())
        {
            k.Disable();
        }
    }

    public void ProgressChunks()
    {
        if (currentChunks.Length == 0)
        {
            // Prewarm
            currentChunks = new MapChunk[CHUNK_LOOKAHEAD_COUNT];
            for(int i = 0; i < CHUNK_LOOKAHEAD_COUNT; i++)
            {
                currentChunks[i] = chunks[i];
            }
        }
        // Unload the last chunk
        currentChunks[0].UnloadChunk();

        // Rebuild currentChunks with 0th index as the new oldest chunk
        for (int i = 0; i < currentChunks.Length - 1; i++)
        {
            currentChunks[i] = currentChunks[i + 1];
        }

        // Find the next chunk (any random chunk besides a current one)
        MapChunk nextChunk = null;
        while (nextChunk == null)
        {
            MapChunk candidate = chunks[Random.Range(0, chunks.Length)];
            bool isCurrentlyActive = false;

            foreach (MapChunk current in currentChunks)
            {
                if (current == candidate)
                {
                    isCurrentlyActive = true;
                    break;
                }
            }

            if (!isCurrentlyActive)
            {
                nextChunk = candidate;
            }
        }

        // Add it to new currentChunks
        currentChunks[^1] = nextChunk;

        // Configure it
        MapChunk previousChunk = currentChunks[^2];
        nextChunk.ConfigureTo(previousChunk.exitAnchor);
    }
}
