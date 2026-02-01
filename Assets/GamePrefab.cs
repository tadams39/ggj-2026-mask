using UnityEngine;

public class GamePrefab : MonoBehaviour
{
    public enum PrefabType
    {
        Obstacle,
        Pickup,
        Score
    }

    public PrefabType myType;

    public enum PickupType
    {
        None,
        Speed,
        Invincibility,
        Jump
    }
    public PickupType pickupType; // Should be None if obstacle or score

    [Header("Settings")]
    [SerializeField] private float obstacleDamage = 25f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound;

    [Header("Debug")]
    [SerializeField] private bool logCollisions = true;

    private bool hasBeenCollected = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if it's the player
        SledController player = collision.gameObject.GetComponent<SledController>();
        if (player == null)
        {
            player = collision.gameObject.GetComponentInParent<SledController>();
        }

        if (player != null)
        {
            if (logCollisions)
            {
                Debug.Log($"[GamePrefab] OnCollisionEnter - Player hit {gameObject.name} (Type: {myType})");
            }
            HandlePlayerContact(player);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        SledController player = other.GetComponent<SledController>();
        if (player == null)
        {
            player = other.GetComponentInParent<SledController>();
        }

        if (player != null)
        {
            if (logCollisions)
            {
                Debug.Log($"[GamePrefab] OnTriggerEnter - Player entered {gameObject.name} (Type: {myType})");
            }
            HandlePlayerContact(player);
        }
    }

    private void HandlePlayerContact(SledController player)
    {
        // Prevent double-collection
        if (hasBeenCollected && myType != PrefabType.Obstacle)
        {
            return;
        }

        switch (myType)
        {
            case PrefabType.Obstacle:
                HandleObstacleHit(player);
                break;

            case PrefabType.Score:
                hasBeenCollected = true;
                HandleCoinPickup();
                break;

            case PrefabType.Pickup:
                hasBeenCollected = true;
                HandlePowerupPickup();
                break;
        }
    }

    private void HandleObstacleHit(SledController player)
    {
        // If player is invincible, destroy the obstacle instead of dealing damage
        if (player.IsInvincible)
        {
            Debug.Log($"[GamePrefab] OBSTACLE DESTROYED by invincible player: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Debug.Log($"[GamePrefab] OBSTACLE HIT! Dealing {obstacleDamage} damage");
        GameEvents.TriggerDamage(obstacleDamage);
    }

    private void HandleCoinPickup()
    {
        Debug.Log($"[GamePrefab] COIN COLLECTED: {gameObject.name}");

        PlayCollectSound();
        GameEvents.TriggerCoinCollected();
        Destroy(gameObject);
    }

    private void HandlePowerupPickup()
    {
        Debug.Log($"[GamePrefab] POWERUP COLLECTED: {pickupType} from {gameObject.name}");

        PlayCollectSound();
        GameEvents.TriggerPowerupCollected(pickupType);
        Destroy(gameObject);
    }

    private void PlayCollectSound()
    {
        if (audioSource != null && collectSound != null)
        {
            // Play at position so it persists after destroy
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }
}
