using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("References")]
    public LevelGenerator levelGenerator;
    public DeathCloud deathCloud;

    [Header("Death Cloud Settings")]
    [SerializeField] private GameObject deathCloudPrefab;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        levelGenerator.Reset();
        SpawnDeathCloud();
    }

    private void SpawnDeathCloud()
    {
        if (deathCloudPrefab != null && deathCloud == null)
        {
            GameObject cloudObj = Instantiate(deathCloudPrefab);
            deathCloud = cloudObj.GetComponent<DeathCloud>();
        }
    }

    // Debug button for resetting the level generator
    [ContextMenu("Reset Level")]
    public void ResetLevel()
    {
        levelGenerator?.Reset();
    }

    // Debug button for progressing chunks
    [ContextMenu("Progress Chunks")]
    public void ProgressLevelChunks()
    {
        levelGenerator?.ProgressChunks();
    }

    // Full game reset (called when player dies)
    public void ResetGame()
    {
        GameEvents.TriggerGameReset();
    }
}
