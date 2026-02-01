using UnityEngine;

public class TriggerSound : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource trigSource;
    public AudioClip obstacleSound;
    public AudioClip coinSound;
    public AudioClip powerupSound;

    [Header("Damage Settings")]
    [SerializeField] private float obstacleDamage = 25f;

    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit something with a GamePrefab component
        GamePrefab prefab = collision.gameObject.GetComponent<GamePrefab>();
        if (prefab == null)
        {
            // Also check parent (prefab might be on parent object)
            prefab = collision.gameObject.GetComponentInParent<GamePrefab>();
        }

        if (prefab != null)
        {
            HandleGamePrefabCollision(prefab, collision.gameObject);
        }
        // Legacy support for tagged obstacles without GamePrefab
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            HandleObstacleHit();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check for trigger-based pickups
        GamePrefab prefab = other.GetComponent<GamePrefab>();
        if (prefab == null)
        {
            prefab = other.GetComponentInParent<GamePrefab>();
        }

        if (prefab != null)
        {
            HandleGamePrefabCollision(prefab, other.gameObject);
        }
    }

    private void HandleGamePrefabCollision(GamePrefab prefab, GameObject obj)
    {
        switch (prefab.myType)
        {
            case GamePrefab.PrefabType.Obstacle:
                HandleObstacleHit();
                break;

            case GamePrefab.PrefabType.Score:
                HandleCoinPickup(obj);
                break;

            case GamePrefab.PrefabType.Pickup:
                HandlePowerupPickup(prefab, obj);
                break;
        }
    }

    private void HandleObstacleHit()
    {
        // Play collision sound
        if (trigSource != null && obstacleSound != null)
        {
            trigSource.PlayOneShot(obstacleSound);
        }

        // Trigger damage
        GameEvents.TriggerDamage(obstacleDamage);
    }

    private void HandleCoinPickup(GameObject coinObj)
    {
        // Play coin sound
        if (trigSource != null && coinSound != null)
        {
            trigSource.PlayOneShot(coinSound);
        }

        // Trigger coin collected event
        GameEvents.TriggerCoinCollected();

        // Destroy the coin (or deactivate if pooled)
        Destroy(coinObj);
    }

    private void HandlePowerupPickup(GamePrefab prefab, GameObject powerupObj)
    {
        // Play powerup sound
        if (trigSource != null && powerupSound != null)
        {
            trigSource.PlayOneShot(powerupSound);
        }

        // Trigger powerup collected event with the type
        GameEvents.TriggerPowerupCollected(prefab.pickupType);

        // Destroy the powerup
        Destroy(powerupObj);
    }
}
