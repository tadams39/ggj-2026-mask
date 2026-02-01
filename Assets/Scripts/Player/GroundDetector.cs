using UnityEngine;

/// <summary>
/// Handles ground detection using multi-point raycasting.
/// Calculates ground normal, slope angles, and provides grounded state.
/// </summary>
public class GroundDetector : MonoBehaviour
{
    [Header("Raycast Configuration")]
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private float rayOriginHeight = 0.2f;
    [SerializeField] private float raySpacing = 0.5f;
    [SerializeField] private LayerMask groundLayerMask = ~0; // Hit everything by default

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;
    [SerializeField] private bool logGroundDetection = false;

    // Public properties
    public bool IsGrounded { get; private set; }
    public Vector3 GroundNormal { get; private set; }
    public float LateralSlope { get; private set; }
    public float ForwardSlope { get; private set; }
    public float DistanceToGround { get; private set; }

    // Raycast offsets in local space
    private Vector3[] raycastOffsets;
    private RaycastHit[] raycastHits;
    private int validHitCount;

    private void Awake()
    {
        // Initialize raycast pattern: center, front, back, left, right
        raycastOffsets = new Vector3[5];
        raycastHits = new RaycastHit[5];

        UpdateRaycastOffsets();
    }

    private void FixedUpdate()
    {
        PerformGroundDetection();
        CalculateSlopes();
    }

    private void UpdateRaycastOffsets()
    {
        raycastOffsets[0] = Vector3.zero;                          // Center
        raycastOffsets[1] = Vector3.forward * raySpacing;         // Front
        raycastOffsets[2] = Vector3.back * raySpacing;            // Back
        raycastOffsets[3] = Vector3.right * raySpacing;           // Right
        raycastOffsets[4] = Vector3.left * raySpacing;            // Left
    }

    private void PerformGroundDetection()
    {
        validHitCount = 0;
        Vector3 averageNormal = Vector3.zero;
        float closestDistance = float.MaxValue;

        Vector3 rayOrigin = transform.position + Vector3.up * rayOriginHeight;

        for (int i = 0; i < raycastOffsets.Length; i++)
        {
            // Convert local offset to world space
            Vector3 worldOffset = transform.TransformDirection(raycastOffsets[i]);
            Vector3 rayStart = rayOrigin + worldOffset;

            if (Physics.Raycast(rayStart, Vector3.down, out raycastHits[i], raycastDistance, groundLayerMask))
            {
                averageNormal += raycastHits[i].normal;
                validHitCount++;

                // Track closest hit for distance
                if (raycastHits[i].distance < closestDistance)
                {
                    closestDistance = raycastHits[i].distance;
                }

                // Debug visualization
                if (showDebugRays)
                {
                    Debug.DrawLine(rayStart, raycastHits[i].point, Color.green);
                }
            }
            else
            {
                // Debug visualization for misses
                if (showDebugRays)
                {
                    Debug.DrawRay(rayStart, Vector3.down * raycastDistance, Color.red);
                }
            }
        }

        // Update grounded state and normal
        IsGrounded = validHitCount > 0;

        if (IsGrounded)
        {
            GroundNormal = (averageNormal / validHitCount).normalized;
            DistanceToGround = closestDistance - rayOriginHeight;

            if (logGroundDetection)
            {
                Debug.Log($"Ground detected: {validHitCount} hits, distance: {DistanceToGround:F2}");
            }
        }
        else
        {
            GroundNormal = Vector3.up;
            DistanceToGround = raycastDistance;

            if (logGroundDetection)
            {
                Debug.LogWarning($"No ground detected! Position: {transform.position}");
            }
        }
    }

    private void CalculateSlopes()
    {
        if (!IsGrounded)
        {
            LateralSlope = 0f;
            ForwardSlope = 0f;
            return;
        }

        // Calculate forward slope (how steep the hill is in forward direction)
        ForwardSlope = Vector3.Angle(Vector3.up, GroundNormal);

        // Calculate lateral slope (sideways tilt)
        // Project ground normal onto the plane perpendicular to forward direction
        Vector3 lateralDirection = Vector3.Cross(transform.forward, Vector3.up).normalized;
        Vector3 projectedNormal = Vector3.ProjectOnPlane(GroundNormal, transform.forward).normalized;

        // Calculate signed angle (negative = tilted left, positive = tilted right)
        LateralSlope = Vector3.SignedAngle(Vector3.up, projectedNormal, transform.forward);
    }

    /// <summary>
    /// Gets the raycast hit at a specific index (0 = center, 1 = front, 2 = back, 3 = right, 4 = left)
    /// </summary>
    public bool TryGetRaycastHit(int index, out RaycastHit hit)
    {
        if (index >= 0 && index < raycastHits.Length && validHitCount > 0)
        {
            hit = raycastHits[index];
            return raycastHits[index].collider != null;
        }

        hit = default;
        return false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || raycastHits == null) return;

        // Draw ground normal
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + GroundNormal * 2f);

        // Draw lateral slope indicator
        if (Mathf.Abs(LateralSlope) > 0.1f)
        {
            Vector3 lateralDir = transform.right * Mathf.Sign(LateralSlope);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + lateralDir * Mathf.Abs(LateralSlope) * 0.1f);
        }

        // Draw hit points (only if grounded)
        if (IsGrounded)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < raycastHits.Length; i++)
            {
                if (raycastHits[i].collider != null)
                {
                    Gizmos.DrawSphere(raycastHits[i].point, 0.1f);
                }
            }
        }
    }
}
