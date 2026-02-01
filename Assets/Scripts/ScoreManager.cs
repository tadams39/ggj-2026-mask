using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("Score Settings")]
    [SerializeField] private float distanceScoreMultiplier = 1f; // Points per meter
    [SerializeField] private int coinScoreValue = 50;

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [SerializeField]
    private int currentScore = 0;
    [SerializeField]
    private int highScore = 0;
    private Vector3 lastPlayerPosition;
    private bool isTracking = false;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Subscribe to events
        GameEvents.OnCoinCollected += HandleCoinCollected;
        GameEvents.OnGameReset += HandleGameReset;
        GameEvents.OnLevelReset += HandleLevelReset;

        // Find player if not assigned
        if (playerTransform == null)
        {
            var sled = FindFirstObjectByType<SledController>();
            if (sled != null)
            {
                playerTransform = sled.transform;
            }
        }

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
            isTracking = true;
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnCoinCollected -= HandleCoinCollected;
        GameEvents.OnGameReset -= HandleGameReset;
        GameEvents.OnLevelReset -= HandleLevelReset;
    }

    private void Update()
    {
        if (!isTracking || playerTransform == null) return;

        // Calculate distance traveled this frame (only forward/downhill progress)
        Vector3 currentPosition = playerTransform.position;
        float distanceTraveled = Vector3.Distance(
            new Vector3(lastPlayerPosition.x, 0, lastPlayerPosition.z),
            new Vector3(currentPosition.x, 0, currentPosition.z)
        );

        // Only count forward progress (not teleports)
        if (distanceTraveled < 10f && distanceTraveled > 0.01f)
        {
            int scoreToAdd = Mathf.FloorToInt(distanceTraveled * distanceScoreMultiplier);
            if (scoreToAdd > 0)
            {
                AddScore(scoreToAdd);
            }
        }

        lastPlayerPosition = currentPosition;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        GameEvents.TriggerScoreChanged(currentScore);

        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            GameEvents.TriggerHighScoreChanged(highScore);
        }
    }

    private void HandleCoinCollected()
    {
        AddScore(coinScoreValue);
    }

    private void HandleGameReset()
    {
        // Full reset - save high score, reset current score
        currentScore = 0;
        GameEvents.TriggerScoreChanged(currentScore);

        // Reset tracking position after a short delay (after teleport)
        Invoke(nameof(ResetTracking), 0.1f);
    }

    private void HandleLevelReset()
    {
        // Level reset only - preserve score
        // Just reset tracking position
        Invoke(nameof(ResetTracking), 0.1f);
    }

    private void ResetTracking()
    {
        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
        }
    }
}
