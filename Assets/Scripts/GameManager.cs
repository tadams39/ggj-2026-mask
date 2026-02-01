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

    private void Update()
    {
        ResetAfterFallOut();
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

    void ResetAfterFallOut()
    {
        float bounds = GetComponent<LevelGenerator>().currentChunks[GetComponent<LevelGenerator>().currentChunks.Length - 1].transform.position.y - 100;
        float playerY = GetComponent<LevelGenerator>().playerTransform.position.y;

        //Debug.Log("bounds: " + bounds);
        //Debug.Log("player y: " + playerY);

        if (playerY < bounds)
        {
            Debug.Log("We flew off of the track!");
            ResetGame();
        }
    }
}
