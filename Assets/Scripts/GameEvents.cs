using System;

public static class GameEvents
{
    // Health events
    public static event Action<float> OnDamageTaken;
    public static event Action OnPlayerDeath;
    public static event Action OnGameReset;       // Full reset (death) - resets score
    public static event Action OnLevelReset;      // Level only (R key) - preserves score

    // Score events
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnHighScoreChanged;
    public static event Action OnCoinCollected;

    // Powerup events
    public static event Action<GamePrefab.PickupType> OnPowerupCollected;
    public static event Action OnPowerupExpired;

    // Invoke methods for safe event triggering
    public static void TriggerDamage(float amount) => OnDamageTaken?.Invoke(amount);
    public static void TriggerPlayerDeath() => OnPlayerDeath?.Invoke();
    public static void TriggerGameReset() => OnGameReset?.Invoke();
    public static void TriggerLevelReset() => OnLevelReset?.Invoke();
    public static void TriggerScoreChanged(int score) => OnScoreChanged?.Invoke(score);
    public static void TriggerHighScoreChanged(int highScore) => OnHighScoreChanged?.Invoke(highScore);
    public static void TriggerCoinCollected() => OnCoinCollected?.Invoke();
    public static void TriggerPowerupCollected(GamePrefab.PickupType type) => OnPowerupCollected?.Invoke(type);
    public static void TriggerPowerupExpired() => OnPowerupExpired?.Invoke();
}
