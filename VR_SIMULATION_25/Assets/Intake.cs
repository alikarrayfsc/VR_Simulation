using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Multi-ball CollectorManager:
/// - Rotates visual rollers
/// - Detects BiodiversityUnit objects via a detectionTrigger (child collider)
/// - Converts detected balls to "carried" state (colliders -> triggers, rb -> kinematic)
/// - Moves each carried ball along a Catmull-Rom spline using Rigidbody.MovePosition/MoveRotation in FixedUpdate
/// - Restores physics when each ball finishes the path
/// </summary>
public class CollectorManager : MonoBehaviour
{
    [Header("Path Settings")]
    [Tooltip("Control points for the spline. First = entry, last = drop point above storage.")]
    public Transform[] controlPoints;

    [SerializeField] private float PathDuration = 3f;
    [SerializeField] private bool PathFaceForward = true;

    [Header("Collector Settings")]
    [Tooltip("All roller parts of the intake that should rotate (visual only).")]
    [SerializeField] private GameObject[] collectorParts;
    [SerializeField] private Vector3 collectorRotationAxis = Vector3.up;
    [SerializeField] private float collectorRotationSpeed = 50f;

    [Tooltip("If true, collector rotates automatically. If false, it needs button control.")]
    [SerializeField] private bool automaticMode = true;
    [Tooltip("Key used to toggle spinning in Control Mode.")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;

    [Header("Detection Settings")]
    [Tooltip("Trigger collider used to detect Biodiversity Units. Place it as a child of Intake.")]
    [SerializeField] private Collider detectionTrigger;

    [Header("Debugging")]
    [Tooltip("Enable to see debug logs about detection and carrying.")]
    [SerializeField] private bool debugLogs = true;

    // rotation state
    private bool isCollectorSpinning = false;

    // data for each carried ball
    private class Carried
    {
        public BiodiversityUnitManager manager;
        public Rigidbody rb;
        public Collider[] colliders;
        public bool[] originalIsTrigger;
        public float pathTime;
    }
    private readonly List<Carried> carriedBalls = new List<Carried>();

    private void Start()
    {
        // basic validation
        if (controlPoints == null || controlPoints.Length < 2)
            Debug.LogError("CollectorManager: Please assign at least 2 control points!");

        if (detectionTrigger == null)
            Debug.LogError("CollectorManager: Please assign a trigger collider in Detection Settings!");
        else
        {
            if (!detectionTrigger.isTrigger)
            {
                Debug.LogWarning("CollectorManager: detectionTrigger is not set as trigger — forcing it to trigger.");
                detectionTrigger.isTrigger = true;
            }

            // Add or wire the TriggerForwarder so trigger events call this manager
            var forwarder = detectionTrigger.GetComponent<TriggerForwarder>();
            if (forwarder == null)
            {
                forwarder = detectionTrigger.gameObject.AddComponent<TriggerForwarder>();
            }
            forwarder.manager = this;
        }

        if (automaticMode) isCollectorSpinning = true;
    }

    private void Update()
    {
        HandleCollectorRotation();
        // NOTE: movement of carried balls happens in FixedUpdate
    }

    private void FixedUpdate()
    {
        // Update all carried balls along the spline using physics-friendly move
        for (int i = carriedBalls.Count - 1; i >= 0; i--)
        {
            var c = carriedBalls[i];

            // safeguard
            if (c == null || c.manager == null || c.rb == null)
            {
                carriedBalls.RemoveAt(i);
                continue;
            }

            c.pathTime += Time.fixedDeltaTime / Mathf.Max(0.0001f, PathDuration);
            float t = Mathf.Clamp01(c.pathTime);

            Vector3 pos = GetCatmullRomClamped(t);
            // use MovePosition/MoveRotation so physics remains stable
            c.rb.MovePosition(pos);

            if (PathFaceForward)
            {
                float lookAhead = 0.001f;
                float t2 = Mathf.Min(t + lookAhead, 1f);
                Vector3 pos2 = GetCatmullRomClamped(t2);
                Vector3 dir = (pos2 - pos).normalized;
                if (dir != Vector3.zero) c.rb.MoveRotation(Quaternion.LookRotation(dir));
            }

            if (t >= 1f)
            {
                FinishCarry(c);
                carriedBalls.RemoveAt(i);
            }
        }
    }

    #region Rotation
    private void HandleCollectorRotation()
    {
        if (collectorParts == null || collectorParts.Length == 0) return;

        if (automaticMode)
        {
            foreach (var part in collectorParts)
                if (part != null) part.transform.Rotate(collectorRotationAxis * collectorRotationSpeed * Time.deltaTime, Space.Self);
            isCollectorSpinning = true;
        }
        else
        {
            if (Input.GetKeyDown(toggleKey)) isCollectorSpinning = !isCollectorSpinning;
            if (isCollectorSpinning)
                foreach (var part in collectorParts)
                    if (part != null) part.transform.Rotate(collectorRotationAxis * collectorRotationSpeed * Time.deltaTime, Space.Self);
        }
    }
    #endregion

    #region Detection API
    // Called by TriggerForwarder or fallback OnTriggerEnter
    public void HandleDetectionEnter(Collider other)
    {
        if (debugLogs) Debug.Log($"CollectorManager: Trigger entered by '{other.name}'");

        if (!isCollectorSpinning)
        {
            if (debugLogs) Debug.Log("CollectorManager: Not spinning — ignoring detection.");
            return;
        }

        if (!other.CompareTag("BiodiversityUnit"))
        {
            if (debugLogs) Debug.Log("CollectorManager: Collider is not BiodiversityUnit — ignoring.");
            return;
        }

        // find manager on the collider or parent (supports compound colliders)
        var unitManager = other.GetComponent<BiodiversityUnitManager>() ?? other.GetComponentInParent<BiodiversityUnitManager>();
        if (unitManager == null)
        {
            if (debugLogs) Debug.LogWarning("CollectorManager: BiodiversityUnit detected but no BiodiversityUnitManager attached — ignoring.");
            return;
        }

        StartCarry(unitManager);
    }

    // fallback if detectionTrigger is on same GameObject (still calls the same handler)
    private void OnTriggerEnter(Collider other)
    {
        // if detectionTrigger is assigned and the trigger uses a forwarder, forwarder calls HandleDetectionEnter.
        // But keep fallback to handle direct triggers.
        HandleDetectionEnter(other);
    }
    #endregion

    #region Carry logic
    private void StartCarry(BiodiversityUnitManager unit)
    {
        if (unit == null) return;
        if (controlPoints == null || controlPoints.Length < 2) return;

        Rigidbody rb = unit.GetComponent<Rigidbody>();
        if (rb == null)
        {
            if (debugLogs) Debug.LogWarning("CollectorManager: BiodiversityUnit has no Rigidbody — cannot carry.");
            return;
        }

        // collect all colliders on the unit (including children)
        Collider[] cols = unit.GetComponentsInChildren<Collider>();
        if (cols == null || cols.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("CollectorManager: BiodiversityUnit has no colliders.");
        }

        // store original isTrigger values and set them to true so the ball doesn't push the robot
        bool[] original = new bool[cols.Length];
        for (int i = 0; i < cols.Length; i++)
        {
            original[i] = cols[i].isTrigger;
            cols[i].isTrigger = true;
        }

        // stop existing motion and set to kinematic so we control it with MovePosition
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // create carried entry
        Carried c = new Carried
        {
            manager = unit,
            rb = rb,
            colliders = cols,
            originalIsTrigger = original,
            pathTime = 0f
        };

        carriedBalls.Add(c);

        if (debugLogs) Debug.Log($"CollectorManager: Started carrying '{unit.name}'. Active carried count = {carriedBalls.Count}");
    }

    private void FinishCarry(Carried c)
    {
        if (c == null) return;

        // restore physics
        if (c.rb != null)
        {
            c.rb.isKinematic = false;
            c.rb.velocity = Vector3.zero; // optional: give small downward velocity if needed
        }

        // restore collider states
        if (c.colliders != null && c.originalIsTrigger != null)
        {
            for (int i = 0; i < c.colliders.Length; i++)
            {
                if (c.colliders[i] != null)
                    c.colliders[i].isTrigger = c.originalIsTrigger[i];
            }
        }

        if (debugLogs) Debug.Log($"CollectorManager: Finished carrying '{c.manager?.name}'. Active carried count = {carriedBalls.Count - 1}");
    }
    #endregion

    #region Catmull-Rom spline
    // Catmull–Rom spline with clamped ends
    private Vector3 GetCatmullRomClamped(float tNorm)
    {
        int count = controlPoints.Length;
        if (count == 2)
        {
            return Vector3.Lerp(controlPoints[0].position, controlPoints[1].position, tNorm);
        }

        float totalSegments = count - 1;
        float tScaled = tNorm * totalSegments;
        int i = Mathf.FloorToInt(tScaled);
        if (i >= count - 1) i = count - 2;
        float u = tScaled - i;

        Vector3 p0 = controlPoints[Mathf.Max(i - 1, 0)].position;
        Vector3 p1 = controlPoints[i].position;
        Vector3 p2 = controlPoints[i + 1].position;
        Vector3 p3 = controlPoints[Mathf.Min(i + 2, count - 1)].position;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * u +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (u * u) +
            (-p0 + 3f * p1 - 3f * p2 + p3) * (u * u * u)
        );
    }
    #endregion
}

/// <summary>
/// Small helper attached to the detectionTrigger object. Forwards the trigger event to the manager.
/// </summary>
public class TriggerForwarder : MonoBehaviour
{
    [HideInInspector] public CollectorManager manager;
    private void OnTriggerEnter(Collider other)
    {
        manager?.HandleDetectionEnter(other);
    }
}
