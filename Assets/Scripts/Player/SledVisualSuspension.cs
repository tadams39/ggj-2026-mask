using UnityEngine;

/// <summary>
/// Visual suspension system that smoothly follows the physics-based sled controller.
/// Reduces visual jitter from terrain bumps while maintaining responsive feel.
/// Attach this to the visual mesh object, not the physics object.
/// </summary>
public class SledVisualSuspension : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The physics object to follow (the GameObject with Rigidbody and SledController)")]
    [SerializeField] private Transform physicsTarget;

    [Header("Position Smoothing")]
    [Tooltip("How quickly the visual follows horizontal movement (higher = more responsive)")]
    [SerializeField] private float horizontalFollowSpeed = 20f;

    [Tooltip("How quickly the visual follows vertical movement (lower = smoother over bumps)")]
    [SerializeField] private float verticalFollowSpeed = 8f;

    [Tooltip("Optional: Clamp max vertical offset from target (0 = no clamp)")]
    [SerializeField] private float maxVerticalOffset = 2f;

    [Header("Rotation Smoothing")]
    [Tooltip("How quickly rotation follows the physics object")]
    [SerializeField] private float rotationFollowSpeed = 10f;

    [Tooltip("Additional smoothing for pitch/roll to reduce jitter")]
    [SerializeField] private float pitchRollSmoothing = 5f;

    [Header("Offset")]
    [Tooltip("Local offset from the physics object (if visual pivot differs)")]
    [SerializeField] private Vector3 visualOffset = Vector3.zero;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    private void Start()
    {
        if (physicsTarget == null)
        {
            Debug.LogError("SledVisualSuspension: No physics target assigned! Please assign the physics sled object.");
            enabled = false;
            return;
        }

        // Initialize at target position/rotation
        Vector3 targetPos = physicsTarget.position + physicsTarget.TransformDirection(visualOffset);

        transform.position = targetPos;
        transform.rotation = physicsTarget.rotation;
    }

    private void LateUpdate()
    {
        if (physicsTarget == null) return;

        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        Vector3 physicsPosition = physicsTarget.position + physicsTarget.TransformDirection(visualOffset);

        // Split into horizontal and vertical components for different smoothing
        Vector3 currentPos = transform.position;
        Vector3 horizontalTarget = new Vector3(physicsPosition.x, currentPos.y, physicsPosition.z);
        float verticalTarget = physicsPosition.y;

        // Smooth horizontal movement (more responsive)
        Vector3 newHorizontal = Vector3.Lerp(
            new Vector3(currentPos.x, 0, currentPos.z),
            new Vector3(horizontalTarget.x, 0, horizontalTarget.z),
            horizontalFollowSpeed * Time.deltaTime
        );

        // Smooth vertical movement (less responsive to absorb bumps)
        float newVertical = Mathf.Lerp(
            currentPos.y,
            verticalTarget,
            verticalFollowSpeed * Time.deltaTime
        );

        // Clamp vertical offset if specified
        if (maxVerticalOffset > 0f)
        {
            float verticalDiff = newVertical - verticalTarget;
            verticalDiff = Mathf.Clamp(verticalDiff, -maxVerticalOffset, maxVerticalOffset);
            newVertical = verticalTarget + verticalDiff;
        }

        transform.position = new Vector3(newHorizontal.x, newVertical, newHorizontal.z);
    }

    private void UpdateRotation()
    {
        // Get target rotation with additional smoothing for pitch/roll
        Quaternion physicsRotation = physicsTarget.rotation;

        // Extract euler angles for fine control
        Vector3 physicsEuler = physicsRotation.eulerAngles;
        Vector3 currentEuler = transform.rotation.eulerAngles;

        // Smooth yaw (heading) more responsively
        float newYaw = Mathf.LerpAngle(
            currentEuler.y,
            physicsEuler.y,
            rotationFollowSpeed * Time.deltaTime
        );

        // Smooth pitch and roll with additional damping to reduce jitter
        float newPitch = Mathf.LerpAngle(
            currentEuler.x,
            physicsEuler.x,
            pitchRollSmoothing * Time.deltaTime
        );

        float newRoll = Mathf.LerpAngle(
            currentEuler.z,
            physicsEuler.z,
            pitchRollSmoothing * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(newPitch, newYaw, newRoll);
    }

    // Editor visualization
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || physicsTarget == null) return;

        // Draw line from physics object to visual object
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(physicsTarget.position, transform.position);

        // Draw target position
        Gizmos.color = Color.yellow;
        Vector3 targetPos = physicsTarget.position + physicsTarget.TransformDirection(visualOffset);
        Gizmos.DrawWireSphere(targetPos, 0.2f);

        // Draw connection
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, targetPos);
    }
}
