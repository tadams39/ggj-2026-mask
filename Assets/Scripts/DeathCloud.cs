using UnityEngine;

public class DeathCloud : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float maxSecondsAway = 10f;
    [SerializeField] private float initialSpeedMultiplier = 1f;
    [SerializeField] private float speedAcceleration = 0.01f; // Speed increase per second

    [Header("Damage Settings")]
    [SerializeField] private float damagePerSecond = 20f;

    [Header("Visual Settings")]
    [SerializeField] private float cloudHeight = 2f;

    private Transform playerTransform;
    private SledController sledController;
    private float currentSpeedMultiplier;
    private bool playerInCloud = false;

    private void Start()
    {
        // Subscribe to events
        GameEvents.OnGameReset += HandleGameReset;
        GameEvents.OnLevelReset += HandleLevelReset;

        // Find player
        sledController = FindFirstObjectByType<SledController>();
        if (sledController != null)
        {
            playerTransform = sledController.transform;
        }

        currentSpeedMultiplier = initialSpeedMultiplier;

        // Initial positioning
        if (playerTransform != null)
        {
            PositionBehindPlayer();
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnGameReset -= HandleGameReset;
        GameEvents.OnLevelReset -= HandleLevelReset;
    }

    private void Update()
    {
        if (playerTransform == null || sledController == null) return;

        // Gradually increase speed over time
        currentSpeedMultiplier += speedAcceleration * Time.deltaTime;

        // Calculate target position based on player speed
        float playerSpeed = Mathf.Max(sledController.Speed, 5f); // Minimum speed for calculation
        float targetDistance = maxSecondsAway * playerSpeed;

        // Get player's backward direction
        Vector3 playerBackward = -sledController.GetForwardDirection();
        Vector3 targetPosition = playerTransform.position + playerBackward * targetDistance;
        targetPosition.y = playerTransform.position.y + cloudHeight;

        // Move toward target position (cloud accelerates to catch up)
        float cloudSpeed = playerSpeed * currentSpeedMultiplier;
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // Move faster when far from target, slower when close
        float catchUpFactor = Mathf.Clamp01(distanceToTarget / targetDistance);
        float actualSpeed = cloudSpeed * (1f + catchUpFactor);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, actualSpeed * Time.deltaTime);

        // Apply damage if player is in the cloud
        if (playerInCloud)
        {
            GameEvents.TriggerDamage(damagePerSecond * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SledController>() != null)
        {
            playerInCloud = true;
            Debug.Log("Player entered death cloud!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<SledController>() != null)
        {
            playerInCloud = false;
            Debug.Log("Player escaped death cloud!");
        }
    }

    private void HandleGameReset()
    {
        // Full reset - reset speed multiplier and position
        currentSpeedMultiplier = initialSpeedMultiplier;
        playerInCloud = false;
        PositionBehindPlayer();
    }

    private void HandleLevelReset()
    {
        // Level reset - just reposition, keep speed multiplier
        playerInCloud = false;
        PositionBehindPlayer();
    }

    private void PositionBehindPlayer()
    {
        if (playerTransform == null || sledController == null) return;

        float playerSpeed = Mathf.Max(sledController.Speed, 5f);
        float targetDistance = maxSecondsAway * playerSpeed;

        Vector3 playerBackward = -sledController.GetForwardDirection();
        Vector3 targetPosition = playerTransform.position + playerBackward * targetDistance;
        targetPosition.y = playerTransform.position.y + cloudHeight;

        transform.position = targetPosition;
    }

    public float GetSecondsAway()
    {
        if (playerTransform == null || sledController == null) return maxSecondsAway;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float playerSpeed = Mathf.Max(sledController.Speed, 1f);
        return distance / playerSpeed;
    }
}
