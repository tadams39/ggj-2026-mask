using UnityEngine;

public class DeathCloud : MonoBehaviour
{
    [Header("Speed Settings")]
    public float minSpeed = 10f;
    public float maxSpeed = 20f;
    public float timeToMaxSpeed = 300f; // 5 minutes in seconds

    [Header("Distance Settings")]
    public float maxDistanceFromPlayer = 100f;
    public float damageRadius = 15f; // Player takes damage when within this radius

    [Header("Damage Settings")]
    public float damagePerSecond = 1f;

    [Header("Visual Settings")]
    public float cloudHeight = 2f;

    [Header("Debug")]
    public bool logDistance = false;

    private Transform playerTransform;
    private SledController sledController;
    private float elapsedTime = 0f;

    private float CurrentSpeed
    {
        get
        {
            float t = Mathf.Clamp01(elapsedTime / timeToMaxSpeed);
            return Mathf.Lerp(minSpeed, maxSpeed, t);
        }
    }

    private void Start()
    {
        GameEvents.OnGameReset += HandleGameReset;
        GameEvents.OnLevelReset += HandleLevelReset;

        sledController = FindFirstObjectByType<SledController>();
        if (sledController != null)
        {
            playerTransform = sledController.transform;
        }

        if (playerTransform != null)
        {
            PositionAtMaxDistance();
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

        // Track elapsed time for speed ramping
        elapsedTime += Time.deltaTime;

        Vector3 targetPosition = playerTransform.position;
        targetPosition.y += cloudHeight;

        float currentDistance = Vector3.Distance(transform.position, playerTransform.position);

        // Rubber band: if too far away, snap to max distance
        if (currentDistance > maxDistanceFromPlayer)
        {
            Vector3 directionToPlayer = (targetPosition - transform.position).normalized;
            transform.position = targetPosition - directionToPlayer * maxDistanceFromPlayer;
            currentDistance = maxDistanceFromPlayer;
        }

        // Move toward player at fixed speed
        float speed = CurrentSpeed;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Recalculate distance after movement
        currentDistance = Vector3.Distance(transform.position, playerTransform.position);

        // Apply damage if player is within damage radius
        if (currentDistance <= damageRadius)
        {
            GameEvents.TriggerDamageOverTime(damagePerSecond * Time.deltaTime);

            if (logDistance)
            {
                Debug.Log($"[DeathCloud] DAMAGING PLAYER! Distance: {currentDistance:F1}m");
            }
        }
        else if (logDistance)
        {
            Debug.Log($"[DeathCloud] Distance: {currentDistance:F1}m, Speed: {speed:F1}m/s, Time: {elapsedTime:F0}s");
        }
    }

    private void HandleGameReset()
    {
        // Full reset - reset timer and position
        elapsedTime = 0f;
        PositionAtMaxDistance();
    }

    private void HandleLevelReset()
    {
        // Level reset - reposition but keep timer
        PositionAtMaxDistance();
    }

    private void PositionAtMaxDistance()
    {
        if (playerTransform == null || sledController == null) return;

        Vector3 playerBackward = -sledController.GetForwardDirection();
        Vector3 targetPosition = playerTransform.position + playerBackward * maxDistanceFromPlayer;
        targetPosition.y = playerTransform.position.y + cloudHeight;

        transform.position = targetPosition;
    }

    public float GetDistanceFromPlayer()
    {
        if (playerTransform == null) return maxDistanceFromPlayer;
        return Vector3.Distance(transform.position, playerTransform.position);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize damage radius in editor
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, damageRadius);
    }
}
