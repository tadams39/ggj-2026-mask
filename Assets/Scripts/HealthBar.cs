using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;

    private void Start()
    {
        // Subscribe to damage events to update UI
        GameEvents.OnDamageTaken += OnDamageTaken;
        GameEvents.OnGameReset += OnGameReset;

        // Initialize to full health
        UpdateHealthBar();
    }

    private void OnDestroy()
    {
        GameEvents.OnDamageTaken -= OnDamageTaken;
        GameEvents.OnGameReset -= OnGameReset;
    }

    private void OnDamageTaken(float damage)
    {
        UpdateHealthBar();
    }

    private void OnGameReset()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (PlayerHealth.instance != null && healthFill != null)
        {
            healthFill.fillAmount = PlayerHealth.instance.CurrentHealth / PlayerHealth.instance.MaxHealth;
        }
    }
}
