using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main sled character controller with physics-based movement and keel mechanic.
/// The keel mechanic resists lateral sliding until a slope threshold, then allows controlled drift.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GroundDetector))]
public class SledController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float accelerationPower = 15f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private float brakeStrength = 2f;

    [Header("Steering")]
    [SerializeField] private float steerStrength = 40f;
    [SerializeField] private float maxAngularVelocity = 100f;

    [Header("Speed-Dependent Steering")]
    [SerializeField] private float minSteerFactor = 0.5f;  // Steering multiplier at max speed
    [SerializeField] private float speedForFullReduction = 20f;  // Speed at which steering starts reducing

    [Header("Keel Mechanic")]
    [Tooltip("Degrees of lateral slope before drift starts")]
    [SerializeField] private float lateralResistanceThreshold = 15f;
    [Tooltip("Maximum force applied to resist lateral sliding")]
    [SerializeField] private float maxResistanceForce = 25f;
    [Tooltip("Damping factor when drifting (0 = free slide, 1 = full resistance)")]
    [SerializeField] private float driftCoefficient = 0.4f;
    [Tooltip("Smoothing range around threshold (degrees)")]
    [SerializeField] private float keelTransitionRange = 5f;

    [Header("Physics")]
    [SerializeField] private float mass = 50f;
    [SerializeField] private float drag = 0.5f;
    [SerializeField] private float angularDrag = 2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private bool logInput = false;

    // Components
    private Rigidbody rb;
    private GroundDetector groundDetector;

    // State
    private bool isDrifting = false;
    private float currentSpeed = 0f;
    private Vector3 lateralVelocity = Vector3.zero;

    // Input
    private float horizontalInput = 0f;
    private float verticalInput = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundDetector = GetComponent<GroundDetector>();

        ConfigureRigidbody();
    }

    private void Start()
    {
        // Register with LevelGenerator
        if (LevelGenerator.instance != null)
        {
            LevelGenerator.instance.playerTransform = transform;
        }
    }

    private void ConfigureRigidbody()
    {
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Freeze X and Z rotation (only allow Y-axis turning)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Set max angular velocity
        rb.maxAngularVelocity = maxAngularVelocity;
    }

    private void Update()
    {
        ReadInput();
        UpdateState();
    }

    private void FixedUpdate()
    {
        if (groundDetector.IsGrounded)
        {
            ApplySteering();
            ApplyAcceleration();
            ApplyKeelMechanic();
        }

        ClampVelocity();
    }

    private void ReadInput()
    {
        // New Input System - using simplified API
        horizontalInput = 0f;
        verticalInput = 0f;

        // Keyboard input
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontalInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontalInput += 1f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                verticalInput += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                verticalInput -= 1f;
        }
        else if (logInput)
        {
            Debug.LogWarning("Keyboard.current is null!");
        }

        // Gamepad input (if connected)
        if (Gamepad.current != null)
        {
            horizontalInput += Gamepad.current.leftStick.x.ReadValue();
            verticalInput += Gamepad.current.leftStick.y.ReadValue();
        }

        // Clamp to -1 to 1 range
        horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
        verticalInput = Mathf.Clamp(verticalInput, -1f, 1f);

        if (logInput && (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f))
        {
            Debug.Log($"Input: Horizontal={horizontalInput:F2}, Vertical={verticalInput:F2}, Grounded={groundDetector.IsGrounded}");
        }
    }

    private void UpdateState()
    {
        currentSpeed = rb.linearVelocity.magnitude;

        // Calculate lateral velocity (sideways sliding component)
        lateralVelocity = Vector3.Project(rb.linearVelocity, transform.right);

        // Update drift state based on lateral slope
        isDrifting = Mathf.Abs(groundDetector.LateralSlope) > lateralResistanceThreshold;
    }

    private void ApplySteering()
    {
        if (Mathf.Abs(horizontalInput) < 0.01f) return;

        // Calculate speed-dependent steering reduction
        float speedFactor = Mathf.Clamp01(currentSpeed / speedForFullReduction);
        float steeringMultiplier = Mathf.Lerp(1f, minSteerFactor, speedFactor);

        // Apply torque for steering
        float steerTorque = horizontalInput * steerStrength * steeringMultiplier;
        rb.AddTorque(Vector3.up * steerTorque, ForceMode.Acceleration);
    }

    private void ApplyAcceleration()
    {
        // Forward acceleration
        if (verticalInput > 0.1f)
        {
            Vector3 accelForce = transform.forward * verticalInput * accelerationPower;
            rb.AddForce(accelForce, ForceMode.Force);

            if (logInput)
            {
                Debug.Log($"Applying force: {accelForce}, rb.mass={rb.mass}, rb.isKinematic={rb.isKinematic}, rb.velocity={rb.linearVelocity}");
            }
        }
        // Braking
        else if (verticalInput < -0.1f)
        {
            rb.linearVelocity *= (1f - brakeStrength * Time.fixedDeltaTime);
        }
    }

    private void ApplyKeelMechanic()
    {
        if (!groundDetector.IsGrounded) return;

        float lateralSlope = groundDetector.LateralSlope;
        float lateralSpeed = lateralVelocity.magnitude;

        // Calculate gravity's lateral component (force trying to slide us sideways)
        Vector3 gravityLateralForce = Physics.gravity.y * Mathf.Sin(lateralSlope * Mathf.Deg2Rad) * transform.right;

        // Calculate keel factor (0 = full resistance, 1 = full drift)
        float keelFactor = CalculateKeelFactor(lateralSlope);

        // Interpolate between resistance and drift behavior
        float resistanceStrength = Mathf.Lerp(maxResistanceForce, driftCoefficient * lateralSpeed, keelFactor);

        // Apply counter-force to lateral movement
        if (lateralSpeed > 0.01f)
        {
            Vector3 keelForce = -lateralVelocity.normalized * resistanceStrength;

            // When resisting (keel engaged), also counter gravity's lateral pull
            if (keelFactor < 0.5f)
            {
                keelForce -= gravityLateralForce * (1f - keelFactor);
            }

            rb.AddForce(keelForce, ForceMode.Force);
        }
    }

    private float CalculateKeelFactor(float lateralSlope)
    {
        // Smooth transition from resistance to drift
        // Returns 0 when fully resisting, 1 when fully drifting
        float absSlope = Mathf.Abs(lateralSlope);
        float lowerBound = lateralResistanceThreshold - keelTransitionRange;
        float upperBound = lateralResistanceThreshold + keelTransitionRange;

        return Mathf.InverseLerp(lowerBound, upperBound, absSlope);
    }

    private void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    /// <summary>
    /// Resets the sled's velocity and rotation. Useful for respawning.
    /// </summary>
    public void ResetPhysics()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp(); // Ensure rigidbody is active
        }
    }

    /// <summary>
    /// Teleports the sled to a new position and rotation safely.
    /// </summary>
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"SledController.Teleport called with position: {position}, rotation: {rotation.eulerAngles}");

        // Reset physics first to prevent momentum from affecting the teleport
        ResetPhysics();

        // Direct transform assignment is more reliable for teleporting
        // MovePosition/MoveRotation can be unreliable when called outside FixedUpdate
        transform.position = position;
        transform.rotation = rotation;

        // Force sync the rigidbody position after direct transform modification
        if (rb != null)
        {
            rb.position = position;
            rb.rotation = rotation;
        }

        Debug.Log($"SledController.Teleport completed. Actual position: {transform.position}");
    }

    /// <summary>
    /// Teleports the sled to a transform's position and rotation.
    /// </summary>
    public void TeleportTo(Transform target, Vector3 offset = default)
    {
        Vector3 targetPos = target.position + offset;
        Teleport(targetPos, target.rotation);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showDebugGizmos) return;
        if (rb == null) return;

        Vector3 pos = transform.position;

        // Draw velocity vector (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + rb.linearVelocity * 0.5f);

        // Draw lateral velocity (blue when resisting, red when drifting)
        Gizmos.color = isDrifting ? Color.red : Color.blue;
        Gizmos.DrawLine(pos, pos + lateralVelocity);

        // Draw forward direction (white)
        Gizmos.color = Color.white;
        Gizmos.DrawLine(pos, pos + transform.forward * 2f);

        // Draw keel state indicator (sphere above sled)
        Vector3 indicatorPos = pos + Vector3.up * 2f;
        Gizmos.color = isDrifting ? new Color(1f, 0f, 0f, 0.5f) : new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(indicatorPos, 0.3f);

        // Debug UI text (in Scene view)
#if UNITY_EDITOR
        if (showDebugUI)
        {
            string debugText = $"Speed: {currentSpeed:F1} m/s\n" +
                              $"Lateral Slope: {groundDetector.LateralSlope:F1}°\n" +
                              $"Keel State: {(isDrifting ? "DRIFTING" : "RESISTING")}\n" +
                              $"Grounded: {groundDetector.IsGrounded}";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, debugText);
        }
#endif
    }

    // Public getters for external systems
    public float Speed => currentSpeed;
    public bool IsDrifting => isDrifting;
    public bool IsGrounded => groundDetector.IsGrounded;
    public Vector3 Velocity => rb.linearVelocity;
}
