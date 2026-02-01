using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("Format Settings")]
    [SerializeField] private string scoreFormat = "Score: {0}";
    [SerializeField] private string highScoreFormat = "Best: {0}";

    private void Start()
    {
        // Subscribe to score events
        GameEvents.OnScoreChanged += UpdateScore;
        GameEvents.OnHighScoreChanged += UpdateHighScore;

        // Initialize display
        UpdateScore(0);
        UpdateHighScore(0);
    }

    private void OnDestroy()
    {
        GameEvents.OnScoreChanged -= UpdateScore;
        GameEvents.OnHighScoreChanged -= UpdateHighScore;
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, score);
        }
    }

    private void UpdateHighScore(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = string.Format(highScoreFormat, highScore);
        }
    }
}
