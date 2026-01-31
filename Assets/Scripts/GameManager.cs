using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public LevelGenerator levelGenerator;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        levelGenerator.Reset();
    }

    // Update is called once per frame
    void Update()
    {

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
}
