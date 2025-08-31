using System.Collections.Generic;
using UnityEngine;

<<<<<<< HEAD
public class SLIPERYROBOT : MonoBehaviour
{
    [Header("Wheel Assignments (Assign ALL Primary WheelColliders)")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider backLeftWheel;
    public WheelCollider backRightWheel;

    float currentsidwaysdirection;

    [Header("Visual Wheel Setup (Optional)")]
    public List<WheelCollider> allWheelCollidersForVisuals = new List<WheelCollider>();
    public List<Transform> allVisualWheels = new List<Transform>();

    [Header("Movement Powers")]
    public float maxMotorForce = 2000f;

    [Header("Braking")]
    public float brakeForce = 500f; // NEW

    [Header("Robot Wheels - Assign Here")]
    public WheelCollider[] leftWheels;
    public WheelCollider[] rightWheels;

    [Header("Lateral Strafing Wheel Visuals")]
    public Transform[] centerStrafingWheelVisuals;

    [Header("Physics Body & Custom Lateral Slip")]
    public Rigidbody robotMainRigidbody;
    public float customLateralForceStrength = 2000f;
    public Rigidbody rb;

    [Header("Visual Wheels - Assign Here (from DriveRobot part)")]
    public Transform[] leftWheelVisuals;
    public Transform[] rightWheelVisuals;
    public float rotationslower;

    void FixedUpdate()
    {
        currentsidwaysdirection = 0;

        float verticalArrowInput = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) verticalArrowInput = -1f;
        else if (Input.GetKey(KeyCode.DownArrow)) verticalArrowInput = 1f;
=======
[RequireComponent(typeof(Rigidbody))]
public class SlipperyRobotWithMiddleWheelZ : MonoBehaviour
{
    [Header("Wheel Colliders (assign)")]
    public WheelCollider frontLeftWheel;   // Z
    public WheelCollider frontRightWheel;  // E
    public WheelCollider backLeftWheel;    // S
    public WheelCollider backRightWheel;   // D

    [Header("Middle Wheel Visual (no collider)")]
    public Transform centerVisual;

    [Header("Wheel Visuals (optional)")]
    public Transform frontLeftVisual;
    public Transform frontRightVisual;
    public Transform backLeftVisual;
    public Transform backRightVisual;

    [Header("Physics & tuning")]
    public Rigidbody rb;
    public float maxMotorForce = 1500f;       // torque for main wheels
    public float torqueSmoothing = 8f;
    public float brakeForce = 8000f;
    public float lateralStrength = 1200f;     // force for middle wheel
    public float rotationTorque = 1200f;      // for turning
    public float rotationSmoothing = 6f;
    public float wheelVisualRotationSpeed = 800f;

    // internal smoothed targets
    float targetFL, targetFR, targetBL, targetBR;
    float currentFL, currentFR, currentBL, currentBR;
    float targetYawTorque, currentYawTorque;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        ConfigureDefaultSidewaysFriction(frontLeftWheel);
        ConfigureDefaultSidewaysFriction(frontRightWheel);
        ConfigureDefaultSidewaysFriction(backLeftWheel);
        ConfigureDefaultSidewaysFriction(backRightWheel);
    }

    void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
>>>>>>> f71c74902644edea5c067ce16b27ce9208e23903

        // --- Rotation (Arrow Keys) ---
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) turnInput = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) turnInput = -1f;

<<<<<<< HEAD
        bool strafeLeftInput = Input.GetKey(KeyCode.Q);
        bool strafeRightInput = Input.GetKey(KeyCode.E);
        bool isStrafing = strafeLeftInput || strafeRightInput;

        if (isStrafing)
        {
            if (strafeRightInput)
                currentsidwaysdirection = -1;
            else if (strafeLeftInput)
                currentsidwaysdirection = 1;
        }

        List<WheelCollider> activeWheels = new List<WheelCollider>
        {
            frontLeftWheel, frontRightWheel,
            backLeftWheel, backRightWheel
        };
        activeWheels.RemoveAll(item => item == null);

        SetAllWheelsToZeroTorque();

        if (verticalArrowInput != 0f)
        {
            foreach (WheelCollider wheel in activeWheels)
            {
                wheel.motorTorque = verticalArrowInput * maxMotorForce;
                wheel.brakeTorque = 0f; // Remove braking
            }
        }
        else if (horizontalArrowInput != 0f)
        {
            foreach (WheelCollider wheel in activeWheels)
            {
                if (wheel.transform.localPosition.x < 0) // Left side
                    wheel.motorTorque = horizontalArrowInput * maxMotorForce * rotationslower;
                else
                    wheel.motorTorque = -horizontalArrowInput * maxMotorForce * rotationslower;

                wheel.brakeTorque = 0f; // Remove braking
            }
        }
        else if (!isStrafing) // No input at all
        {
            foreach (WheelCollider wheel in activeWheels)
            {
                wheel.motorTorque = 0f;
                wheel.brakeTorque = brakeForce; // Apply braking
            }
        }

        // Lateral Strafing
        if (isStrafing)
        {
            foreach (WheelCollider wheel in activeWheels)
                wheel.brakeTorque = 0f; // Remove brake during strafing

            if (robotMainRigidbody != null)
            {
                Vector3 lateralForce = robotMainRigidbody.transform.right * currentsidwaysdirection * customLateralForceStrength;
                robotMainRigidbody.AddForce(lateralForce, ForceMode.Force);
            }
            else
            {
                Debug.LogWarning("robotMainRigidbody is not assigned. Cannot apply lateral force.");
            }
        }

        UpdateAllWheelVisualsCombined(isStrafing, currentsidwaysdirection);
    }

    void SetAllWheelsToZeroTorque()
    {
        List<WheelCollider> allAvailableWheelColliders = new List<WheelCollider>();

        if (frontLeftWheel != null) allAvailableWheelColliders.Add(frontLeftWheel);
        if (frontRightWheel != null) allAvailableWheelColliders.Add(frontRightWheel);
        if (backLeftWheel != null) allAvailableWheelColliders.Add(backLeftWheel);
        if (backRightWheel != null) allAvailableWheelColliders.Add(backRightWheel);

        if (allWheelCollidersForVisuals != null)
        {
            foreach (WheelCollider wheelCol in allWheelCollidersForVisuals)
            {
                if (wheelCol != null && !allAvailableWheelColliders.Contains(wheelCol))
                    allAvailableWheelColliders.Add(wheelCol);
            }
        }

        if (leftWheels != null)
        {
            foreach (WheelCollider wheel in leftWheels)
            {
                if (wheel != null && !allAvailableWheelColliders.Contains(wheel))
                    allAvailableWheelColliders.Add(wheel);
            }
        }
        if (rightWheels != null)
        {
            foreach (WheelCollider wheel in rightWheels)
            {
                if (wheel != null && !allAvailableWheelColliders.Contains(wheel))
                    allAvailableWheelColliders.Add(wheel);
            }
        }

        foreach (WheelCollider wheel in allAvailableWheelColliders)
        {
            if (wheel != null)
            {
                wheel.motorTorque = 0f;
                wheel.brakeTorque = 0f;

                WheelFrictionCurve normalSidewaysFriction = new WheelFrictionCurve();
                normalSidewaysFriction.extremumSlip = 0.2f;
                normalSidewaysFriction.extremumValue = 1f;
                normalSidewaysFriction.asymptoteSlip = 0.5f;
                normalSidewaysFriction.asymptoteValue = 0.75f;
                normalSidewaysFriction.stiffness = 1f;
                wheel.sidewaysFriction = normalSidewaysFriction;

                wheel.steerAngle = 0f;
            }
        }
    }

    void UpdateSingleWheelVisual(WheelCollider collider, Transform visualWheel)
=======
        targetYawTorque = turnInput * rotationTorque;
        currentYawTorque = Mathf.Lerp(currentYawTorque, targetYawTorque, 1 - Mathf.Exp(-rotationSmoothing * delta));
        if (Mathf.Abs(currentYawTorque) > 0.01f)
            rb.AddTorque(Vector3.up * currentYawTorque, ForceMode.Force);

        // --- Individual wheel keys ---
        bool keyZ = Input.GetKey(KeyCode.S); // front left
        bool keyE = Input.GetKey(KeyCode.D); // front right
        bool keyS = Input.GetKey(KeyCode.W); // back left
        bool keyD = Input.GetKey(KeyCode.E); // back right

        // Middle wheel strafing keys
        float strafeDir = 0f;
        if (Input.GetKey(KeyCode.I)) strafeDir = 1f;    // strafe right
        if (Input.GetKey(KeyCode.P)) strafeDir = -1f;   // strafe left

        // Reset targets
        targetFL = targetFR = targetBL = targetBR = 0f;

        if (keyZ) targetFL = maxMotorForce;
        if (keyE) targetFR = maxMotorForce;
        if (keyS) targetBL = -maxMotorForce;
        if (keyD) targetBR = -maxMotorForce;

        // Brake if no input
        bool anyInput = keyZ || keyE || keyS || keyD || strafeDir != 0f || turnInput != 0f;
        float appliedBrake = anyInput ? 0f : brakeForce;

        // Smooth torque
        currentFL = Mathf.Lerp(currentFL, targetFL, 1 - Mathf.Exp(-torqueSmoothing * delta));
        currentFR = Mathf.Lerp(currentFR, targetFR, 1 - Mathf.Exp(-torqueSmoothing * delta));
        currentBL = Mathf.Lerp(currentBL, targetBL, 1 - Mathf.Exp(-torqueSmoothing * delta));
        currentBR = Mathf.Lerp(currentBR, targetBR, 1 - Mathf.Exp(-torqueSmoothing * delta));

        ApplyMotorAndBrakes(frontLeftWheel, currentFL, appliedBrake);
        ApplyMotorAndBrakes(frontRightWheel, currentFR, appliedBrake);
        ApplyMotorAndBrakes(backLeftWheel, currentBL, appliedBrake);
        ApplyMotorAndBrakes(backRightWheel, currentBR, appliedBrake);

        // --- Middle wheel lateral force ---
        float lateralForce = strafeDir * lateralStrength;
        if (Mathf.Abs(lateralForce) > 0.01f)
            rb.AddForce(transform.right * lateralForce, ForceMode.Force);

        // --- Update visuals ---
        UpdateWheelVisual(frontLeftWheel, frontLeftVisual, currentFL);
        UpdateWheelVisual(frontRightWheel, frontRightVisual, currentFR);
        UpdateWheelVisual(backLeftWheel, backLeftVisual, currentBL);
        UpdateWheelVisual(backRightWheel, backRightVisual, currentBR);

        // Middle wheel visual rotation along Z axis
        if (centerVisual != null)
        {
            float rotationAmount = strafeDir * lateralStrength * delta;
            centerVisual.Rotate(Vector3.forward, rotationAmount, Space.Self);
        }
    }

    void ApplyMotorAndBrakes(WheelCollider col, float torqueVal, float brake)
>>>>>>> f71c74902644edea5c067ce16b27ce9208e23903
    {
        if (col == null) return;
        col.motorTorque = torqueVal;
        col.brakeTorque = brake;
        col.steerAngle = 0f;
    }

<<<<<<< HEAD
    void UpdateAllWheelVisualsCombined(bool isStrafing, float strafeDirection)
    {
        if (allWheelCollidersForVisuals.Count == allVisualWheels.Count)
        {
            for (int i = 0; i < allWheelCollidersForVisuals.Count; i++)
            {
                WheelCollider wheelCol = allWheelCollidersForVisuals[i];
                Transform visualWheel = allVisualWheels[i];
                if (wheelCol == null || visualWheel == null) continue;

                Vector3 pos;
                Quaternion rot;
                wheelCol.GetWorldPose(out pos, out rot);
                visualWheel.position = pos;
                visualWheel.rotation = rot;
            }
=======
    void UpdateWheelVisual(WheelCollider col, Transform visual, float torqueEstimate)
    {
        if (visual == null) return;
        if (col != null)
        {
            Vector3 pos;
            Quaternion rot;
            col.GetWorldPose(out pos, out rot);
            visual.position = pos;
            visual.rotation = rot;

            float spin = torqueEstimate * Time.fixedDeltaTime;
            visual.Rotate(Vector3.right, spin * wheelVisualRotationSpeed, Space.Self);
>>>>>>> f71c74902644edea5c067ce16b27ce9208e23903
        }
    }

<<<<<<< HEAD
        for (int i = 0; i < leftWheels.Length; i++)
        {
            if (leftWheels[i] != null && i < leftWheelVisuals.Length && leftWheelVisuals[i] != null)
            {
                UpdateSingleWheelVisual(leftWheels[i], leftWheelVisuals[i]);
            }
        }
        for (int i = 0; i < rightWheels.Length; i++)
        {
            if (rightWheels[i] != null && i < rightWheelVisuals.Length && rightWheelVisuals[i] != null)
            {
                UpdateSingleWheelVisual(rightWheels[i], rightWheelVisuals[i]);
            }
        }

        if (centerStrafingWheelVisuals != null)
        {
            for (int i = 0; i < centerStrafingWheelVisuals.Length; i++)
            {
                Transform visualWheel = centerStrafingWheelVisuals[i];
                if (visualWheel == null) continue;

                // Optional: update rotation/visual feedback for strafing wheels
                visualWheel.localRotation = Quaternion.Euler(
                    visualWheel.localEulerAngles.x,
                    visualWheel.localEulerAngles.y,
                    visualWheel.localEulerAngles.z
                );
            }
        }
=======
    void ConfigureDefaultSidewaysFriction(WheelCollider col)
    {
        if (col == null) return;
        WheelFrictionCurve side = col.sidewaysFriction;
        side.extremumSlip = 0.2f;
        side.extremumValue = 1f;
        side.asymptoteSlip = 0.5f;
        side.asymptoteValue = 0.75f;
        side.stiffness = 1f;
        col.sidewaysFriction = side;
>>>>>>> f71c74902644edea5c067ce16b27ce9208e23903
    }

    void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rb.worldCenterOfMass, 0.06f);
        }
    }
}
