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

    [Header("Look Target")]
    [Tooltip("How far above the sled to look (in meters)")]
    public float lookOffsetAbove = 1f;

    [Header("Smoothing")]
    [Tooltip("How quickly the camera follows (higher = snappier)")]
    public float positionSmoothSpeed = 5f;

    private Rigidbody playerRigidbody;

    void Start()
    {
        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
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

        // Calculate the direction from player to exit (this is "forward")
        Vector3 toExit = currentChunk.exitAnchor.position - playerPos;
        toExit.y = 0; // Flatten to horizontal plane
        Vector3 forwardDir = -toExit.normalized;

        // "Behind" is the opposite direction
        Vector3 behindDir = -forwardDir;

        // Calculate target camera position
        Vector3 targetPosition = playerPos
            + Vector3.up * heightAbovePlayer
            + behindDir * distanceBehind;

        // Smoothly move camera to target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothSpeed * Time.deltaTime);

        // Look at a point above the sled
        Vector3 lookTarget = playerPos + Vector3.up * lookOffsetAbove;
        transform.LookAt(lookTarget);
    }

    private MapChunk GetCurrentChunk()
    {
        var chunks = LevelGenerator.instance.currentChunks;
        if (chunks == null || chunks.Length < 2) return null;

        // Return chunk at index 1 (the chunk the player is typically in after progression)
        // Index 0 is the chunk being left behind, index 1+ are ahead
        return chunks[1];
    }
}
