using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager instance;

    [Header("Duration Settings")]
    [SerializeField] private float defaultDuration = 10f;
    [SerializeField] private float coinDurationBonus = 1f;

    [Header("Powerup Effects")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float jumpGravityMultiplier = 0.3f;

    private SledController sledController;
    private GamePrefab.PickupType activePowerup = GamePrefab.PickupType.None;
    private float remainingDuration = 0f;

    public GamePrefab.PickupType ActivePowerup => activePowerup;
    public float RemainingDuration => remainingDuration;
    public bool HasActivePowerup => activePowerup != GamePrefab.PickupType.None && remainingDuration > 0f;

    private void Awake()
    {
        instance = this;
        sledController = GetComponent<SledController>();
    }

    private void Start()
    {
        // Subscribe to events
        GameEvents.OnPowerupCollected += HandlePowerupCollected;
        GameEvents.OnCoinCollected += HandleCoinCollected;
        GameEvents.OnGameReset += HandleGameReset;
        GameEvents.OnLevelReset += HandleLevelReset;
    }

    private void OnDestroy()
    {
        GameEvents.OnPowerupCollected -= HandlePowerupCollected;
        GameEvents.OnCoinCollected -= HandleCoinCollected;
        GameEvents.OnGameReset -= HandleGameReset;
        GameEvents.OnLevelReset -= HandleLevelReset;
    }

    private void Update()
    {
        if (HasActivePowerup)
        {
            remainingDuration -= Time.deltaTime;

            if (remainingDuration <= 0f)
            {
                DeactivatePowerup();
            }
        }
    }

    private void HandlePowerupCollected(GamePrefab.PickupType type)
    {
        if (type == GamePrefab.PickupType.None) return;

        // If same powerup type, extend duration
        if (type == activePowerup)
        {
            remainingDuration += defaultDuration;
            Debug.Log($"Extended {type} powerup. Duration: {remainingDuration:F1}s");
        }
        else
        {
            // Different powerup - deactivate old one and activate new
            if (HasActivePowerup)
            {
                DeactivatePowerup();
            }

            activePowerup = type;
            remainingDuration = defaultDuration;
            ApplyPowerupEffect(type);
            Debug.Log($"Activated {type} powerup for {defaultDuration}s");
        }
    }

    private void HandleCoinCollected()
    {
        // Extend active powerup duration when collecting coins
        if (HasActivePowerup)
        {
            remainingDuration += coinDurationBonus;
            Debug.Log($"Coin extended powerup duration by {coinDurationBonus}s. Remaining: {remainingDuration:F1}s");
        }
    }

    private void HandleGameReset()
    {
        // Full reset - clear powerup
        if (HasActivePowerup)
        {
            DeactivatePowerup();
        }
    }

    private void HandleLevelReset()
    {
        // Level reset - keep powerup active
        // Do nothing, powerup persists through level resets
    }

    public Material speedMat;
    public Material invinciblityMat;
    public Material jumpMat;
    public GameObject rider;

    private void ApplyPowerupEffect(GamePrefab.PickupType type)
    {
        if (sledController == null) return;

        switch (type)
        {
            case GamePrefab.PickupType.Speed:
                Debug.Log("Got speed");
                rider.GetComponent<MeshRenderer>().material = speedMat;
                sledController.SpeedMultiplier = speedMultiplier;
                break;

            case GamePrefab.PickupType.Invincibility:
                Debug.Log("Got invincibility");
                rider.GetComponent<MeshRenderer>().material = invinciblityMat;
                sledController.IsInvincible = true;
                break;

            case GamePrefab.PickupType.Jump:
                Debug.Log("Got jump");
                rider.GetComponent<MeshRenderer>().material = jumpMat;
                sledController.CanJump = true;
                sledController.GravityMultiplier = jumpGravityMultiplier;
                break;
        }
    }

    private void DeactivatePowerup()
    {
        if (sledController != null)
        {
            // Reset all modifiers
            sledController.SpeedMultiplier = 1f;
            sledController.GravityMultiplier = 1f;
            sledController.CanJump = false;
            sledController.IsInvincible = false;
        }

        Debug.Log($"Powerup {activePowerup} expired");
        activePowerup = GamePrefab.PickupType.None;
        remainingDuration = 0f;

        GameEvents.TriggerPowerupExpired();
    }
}
