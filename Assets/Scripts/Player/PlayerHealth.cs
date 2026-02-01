using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float obstacleDamage = 25f;
    [SerializeField] private float invulnerabilityDuration = 1f;

    [SerializeField]
    private float currentHealth;
    private float invulnerabilityTimer = 0f;
    private SledController sledController;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsInvulnerable => invulnerabilityTimer > 0f;

    private void Awake()
    {
        instance = this;
        sledController = GetComponent<SledController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        // Subscribe to events
        GameEvents.OnDamageTaken += HandleDamage;
        GameEvents.OnDamageOverTime += HandleDamageOverTime;
        GameEvents.OnGameReset += HandleGameReset;
        GameEvents.OnLevelReset += HandleLevelReset;
    }

    private void OnDestroy()
    {
        GameEvents.OnDamageTaken -= HandleDamage;
        GameEvents.OnDamageOverTime -= HandleDamageOverTime;
        GameEvents.OnGameReset -= HandleGameReset;
        GameEvents.OnLevelReset -= HandleLevelReset;
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    // Obstacle damage - resets level on hit
    private void HandleDamage(float damage)
    {
        // Check if invulnerable (either from timer or from powerup)
        if (IsInvulnerable || (sledController != null && sledController.IsInvincible))
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        // Grant brief invulnerability after taking damage
        invulnerabilityTimer = invulnerabilityDuration;

        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            // Player died - full game reset
            Debug.Log("Player died! Triggering full game reset.");
            GameEvents.TriggerPlayerDeath();
            GameEvents.TriggerGameReset();
        }
        else
        {
            // Just took damage - reset level only
            Debug.Log("Player hit obstacle! Resetting level.");
            GameEvents.TriggerLevelReset();
        }
    }

    // Continuous damage (fog) - no level reset, just health drain
    private void HandleDamageOverTime(float damage)
    {
        // Invincibility powerup blocks fog damage too
        if (sledController != null && sledController.IsInvincible)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        if (currentHealth <= 0f)
        {
            // Player died from fog - full game reset
            Debug.Log("Player died from fog! Triggering full game reset.");
            GameEvents.TriggerPlayerDeath();
            GameEvents.TriggerGameReset();
        }
    }

    private void HandleGameReset()
    {
        // Full reset - restore health
        currentHealth = maxHealth;
        invulnerabilityTimer = invulnerabilityDuration; // Brief invuln after respawn

        if (LevelGenerator.instance != null)
        {
            LevelGenerator.instance.Reset();
        }
    }

    private void HandleLevelReset()
    {
        // Level reset only - preserve health
        invulnerabilityTimer = invulnerabilityDuration; // Brief invuln after respawn

        if (LevelGenerator.instance != null)
        {
            LevelGenerator.instance.Reset();
        }
    }

    public void TakeDamage(float damage)
    {
        GameEvents.TriggerDamage(damage);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
}
