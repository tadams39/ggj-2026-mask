using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerupUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject powerupPanel;
    [SerializeField] private Image powerupIcon;
    [SerializeField] private Image durationFill;
    [SerializeField] private TextMeshProUGUI powerupNameText;

    [Header("Powerup Icons")]
    [SerializeField] private Sprite speedIcon;
    [SerializeField] private Sprite invincibilityIcon;
    [SerializeField] private Sprite jumpIcon;

    [Header("Powerup Colors")]
    [SerializeField] private Color speedColor = Color.yellow;
    [SerializeField] private Color invincibilityColor = Color.cyan;
    [SerializeField] private Color jumpColor = Color.green;

    private float maxDuration = 10f;

    private void Start()
    {
        // Subscribe to powerup events
        GameEvents.OnPowerupCollected += OnPowerupCollected;
        GameEvents.OnPowerupExpired += OnPowerupExpired;

        // Hide panel initially
        if (powerupPanel != null)
        {
            powerupPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnPowerupCollected -= OnPowerupCollected;
        GameEvents.OnPowerupExpired -= OnPowerupExpired;
    }

    private void Update()
    {
        // Update duration bar
        if (PowerupManager.instance != null && PowerupManager.instance.HasActivePowerup)
        {
            if (durationFill != null)
            {
                durationFill.fillAmount = PowerupManager.instance.RemainingDuration / maxDuration;
            }
        }
    }

    private void OnPowerupCollected(GamePrefab.PickupType type)
    {
        if (powerupPanel != null)
        {
            powerupPanel.SetActive(true);
        }

        // Set icon and color based on type
        switch (type)
        {
            case GamePrefab.PickupType.Speed:
                SetPowerupDisplay(speedIcon, speedColor, "SPEED");
                break;
            case GamePrefab.PickupType.Invincibility:
                SetPowerupDisplay(invincibilityIcon, invincibilityColor, "INVINCIBLE");
                break;
            case GamePrefab.PickupType.Jump:
                SetPowerupDisplay(jumpIcon, jumpColor, "JUMP");
                break;
        }

        // Reset duration bar
        if (durationFill != null)
        {
            durationFill.fillAmount = 1f;
        }
    }

    private void SetPowerupDisplay(Sprite icon, Color color, string name)
    {
        if (powerupIcon != null)
        {
            powerupIcon.sprite = icon;
            powerupIcon.color = color;
        }

        if (durationFill != null)
        {
            durationFill.color = color;
        }

        if (powerupNameText != null)
        {
            powerupNameText.text = name;
            powerupNameText.color = color;
        }
    }

    private void OnPowerupExpired()
    {
        if (powerupPanel != null)
        {
            powerupPanel.SetActive(false);
        }
    }
}
