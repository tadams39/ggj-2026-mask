using UnityEngine;

public class LockedCamera : MonoBehaviour
{
    [Header("Target")]
    public GameObject player;

    [Header("Camera Position")]
    [Tooltip("Height above the player in meters")]
    public float heightAbovePlayer = 8f;

    [Tooltip("Distance behind the player in meters")]
    public float distanceBehind = 4f;

    [Tooltip("When player is within this distance of exit, use 100% player forward")]
    public float exitProximityThreshold = 10f;

    [Tooltip("Beyond this distance, use minimum forward influence (10%)")]
    public float exitFarThreshold = 30f;

    [Tooltip("Minimum influence of player forward vector (0-1)")]
    [Range(0f, 1f)]
    public float minForwardInfluence = 0.1f;

    [Header("Look Target")]
    [Tooltip("How far above the sled to look (in meters)")]
    public float lookOffsetAbove = 1f;

    public int currentChunkIndex = 1;

    [Header("Smoothing")]
    [Tooltip("How quickly the camera follows (higher = snappier)")]
    public float positionSmoothSpeed = 5f;

    [Tooltip("How quickly the camera rotates to look at target (higher = snappier)")]
    public float rotationSmoothSpeed = 5f;

    

    private Rigidbody playerRigidbody;
    private SledController sledController;

    void Start()
    {
        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
            sledController = player.GetComponent<SledController>();
        }
    }

    void LateUpdate()
    {
        if (playerRigidbody == null || LevelGenerator.instance == null) return;

        Vector3 playerPos = playerRigidbody.position;

        // Get the current chunk's exit anchor to determine "forward" direction
        // Player is typically in chunk at index 1 after initial progression
        MapChunk currentChunk = GetCurrentChunk();
        if (currentChunk == null || currentChunk.exitAnchor == null) return;

        // Calculate direction for camera positioning
        Vector3 toExit = currentChunk.exitAnchor.position - playerPos;
        float distanceToExit = new Vector2(toExit.x, toExit.z).magnitude; // Horizontal distance only

        // Get player's velocity-based forward direction
        Vector3 playerForward = sledController.GetForwardDirection();

        // Get exit direction (flattened)
        Vector3 exitDir = toExit;
        exitDir.y = 0;
        exitDir.Normalize();

        // Calculate forward influence based on distance
        // At exitProximityThreshold or closer: 100% forward
        // At exitFarThreshold or farther: minForwardInfluence (10%)
        // Between: linear interpolation
        float forwardInfluence;
        if (distanceToExit <= exitProximityThreshold)
        {
            forwardInfluence = 1f;
        }
        else if (distanceToExit >= exitFarThreshold)
        {
            forwardInfluence = minForwardInfluence;
        }
        else
        {
            // Linear interpolation between thresholds
            float t = (distanceToExit - exitProximityThreshold) / (exitFarThreshold - exitProximityThreshold);
            forwardInfluence = Mathf.Lerp(1f, minForwardInfluence, t);
        }

        // Blend between exit direction and player forward
        Vector3 forwardDir = Vector3.Lerp(-exitDir, playerForward, forwardInfluence).normalized;

        // "Behind" is the opposite direction
        Vector3 behindDir = -forwardDir;

        // Calculate target camera position
        Vector3 targetPosition = playerPos
            + Vector3.up * heightAbovePlayer
            + behindDir * distanceBehind;

        // Smoothly move camera to target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothSpeed * Time.deltaTime);

        // Smoothly rotate to look at a point above the sled
        Vector3 lookTarget = playerPos + Vector3.up * lookOffsetAbove;
        Vector3 lookDirection = lookTarget - transform.position;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }

    private MapChunk GetCurrentChunk()
    {
        var chunks = LevelGenerator.instance.currentChunks;
        if (chunks == null || chunks.Length < 2) return null;

        // Return chunk at index 1 (the chunk the player is typically in after progression)
        // Index 0 is the chunk being left behind, index 1+ are ahead
        return chunks[currentChunkIndex];
    }
}
