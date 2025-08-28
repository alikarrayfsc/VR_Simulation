using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine;

// Fusion des two scripts : SLIPERYROBOT and DriveRobot (Updated)
public class SLIPERYROBOT : MonoBehaviour
{
    // --- Wheel Assignments (Assign ALL Primary WheelColliders) ---
    [Header("Wheel Assignments (Assign ALL Primary WheelColliders)")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider backLeftWheel;
    public WheelCollider backRightWheel;

    float currentsidwaysdirection;
    

    [Header("Visual Wheel Setup (Optional)")]
    // Assign ALL WheelColliders that have a visual representation here, in order.
    // Ensure these are the WheelColliders of your PRIMARY drive wheels.
    public List<WheelCollider> allWheelCollidersForVisuals = new List<WheelCollider>();
    // Assign ALL corresponding visual Transforms here, matching the order of allWheelCollidersForVisuals.
    public List<Transform> allVisualWheels = new List<Transform>();

    [Header("Movement Powers")]
    public float maxMotorForce = 2000f; // Power for standard forward/backward/turning movement
    // --- DriveRobot additions (as per your provided script) ---
    [Header("Robot Wheels - Assign Here")]
    public WheelCollider[] leftWheels;
    public WheelCollider[] rightWheels;

    // --- Lateral Strafing Wheel Visuals (Q/E) ---
    // These are NOW ONLY for the VISUALS of the strafing wheels, they do NOT use WheelColliders.
    [Header("Lateral Strafing Wheel Visuals")]
    public Transform[] centerStrafingWheelVisuals;

    [Header("Physics Body & Custom Lateral Slip")]
    public Rigidbody robotMainRigidbody; // Assign the main Rigidbody of your robot for applying lateral force.
    public float customLateralForceStrength = 2000f;
    public Rigidbody rb; // Reference for the Center of Mass (CoM) visualization.

    [Header("Visual Wheels - Assign Here (from DriveRobot part)")]
    public Transform[] leftWheelVisuals;
    public Transform[] rightWheelVisuals;

    void FixedUpdate()
    {
        currentsidwaysdirection = 0;
        // --- Inputs for standard movement (arrow keys) ---
        float verticalArrowInput = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) verticalArrowInput = -1f;
        else if (Input.GetKey(KeyCode.DownArrow)) verticalArrowInput = 1f;

        float horizontalArrowInput = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) horizontalArrowInput = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) horizontalArrowInput = -1f;

        // --- Inputs for lateral strafing ---
        bool strafeLeftInput = Input.GetKey(KeyCode.Q);
        bool strafeRightInput = Input.GetKey(KeyCode.E);
        bool isStrafing = strafeLeftInput || strafeRightInput;
        if (isStrafing) {
            if (strafeRightInput)
            {
                currentsidwaysdirection = -1;
            }
            else if (strafeLeftInput)
            {
                currentsidwaysdirection = 1;
            }
        }

        // --- List of active wheels for standard movement (6 wheels as per your provided script) ---
        List<WheelCollider> activeWheels = new List<WheelCollider>
        {
            frontLeftWheel, frontRightWheel,
            backLeftWheel, backRightWheel
        };
        activeWheels.RemoveAll(item => item == null); // Remove any unassigned wheel slots

        // --- Reset all identified wheels (primary drive wheels) ---
        // This ensures all torques, brakes, and steer angles are zeroed before applying new forces.
        SetAllWheelsToZeroTorque();

        // The ConfigureStrafingWheels method is REMOVED as there are no WheelColliders for these wheels anymore.
        // The visual rotation for strafing wheels is now handled directly within UpdateAllWheelVisualsCombined.

        // --- Standard movement (arrow keys) ---
        // These blocks are retained as per your request "do not remove the other wheel's movement".
        if (verticalArrowInput != 0f)
        {
            // Forward/backward movement (applies to the 6 'activeWheels')
            foreach (WheelCollider wheel in activeWheels)
            {
                wheel.motorTorque = verticalArrowInput * maxMotorForce;
            }
        }
        else if (horizontalArrowInput != 0f)
        {
            // Turning (applies to the 6 'activeWheels')
            foreach (WheelCollider wheel in activeWheels)
            {
                if (wheel.transform.localPosition.x < 0) // Left side wheels
                    wheel.motorTorque = horizontalArrowInput * maxMotorForce; // Using single maxMotorForce
                else // Right side wheels
                    wheel.motorTorque = -horizontalArrowInput * maxMotorForce; // Using single maxMotorForce
            }
        }
        else
        {
            // If no arrow key input, ensure motor torque is zero for standard wheels
            foreach (WheelCollider wheel in activeWheels)
            {
                wheel.motorTorque = 0f;
            }
        }

        // --- Lateral Strafing (Q/E) ---
        if (isStrafing)
        {
            if (robotMainRigidbody == null)
            {
                Debug.LogWarning("robotMainRigidbody is not assigned. Cannot apply lateral force for strafing. Please assign it in the Inspector.");
                // Continue without applying force, but allow visual updates if robotMainRigidbody is null.
            }
            else
            {
                Vector3 lateralForce = robotMainRigidbody.transform.right * currentsidwaysdirection * customLateralForceStrength;
                robotMainRigidbody.AddForce(lateralForce, ForceMode.Force);
            }
        }

        // --- Visual update ---
        // Merged and updated visual update method to manage all wheel visuals selectively.
        UpdateAllWheelVisualsCombined(isStrafing, currentsidwaysdirection); // Pass strafing info for visual wheels
    }

    // --- Helper to reset all recognized wheel states for WheelColliders ---
    // This method now collects all WheelColliders declared in the script and resets their state.
    void SetAllWheelsToZeroTorque()
    {
        List<WheelCollider> allAvailableWheelColliders = new List<WheelCollider>();

        // Add the explicitly named primary wheel colliders
        if (frontLeftWheel != null) allAvailableWheelColliders.Add(frontLeftWheel);
        if (frontRightWheel != null) allAvailableWheelColliders.Add(frontRightWheel); 
        if (backLeftWheel != null) allAvailableWheelColliders.Add(backLeftWheel);
        if (backRightWheel != null) allAvailableWheelColliders.Add(backRightWheel);

        // Add any other WheelColliders assigned for visuals (allWheelCollidersForVisuals) if not already included
        if (allWheelCollidersForVisuals != null)
        {
            foreach (WheelCollider wheelCol in allWheelCollidersForVisuals)
            {
                if (wheelCol != null && !allAvailableWheelColliders.Contains(wheelCol))
                {
                    allAvailableWheelColliders.Add(wheelCol);
                }
            }
        }

        // Add DriveRobot's specific wheel lists if they are distinct and need reset
        if (leftWheels != null)
        {
            foreach (WheelCollider wheel in leftWheels)
            {
                if (wheel != null && !allAvailableWheelColliders.Contains(wheel))
                {
                    allAvailableWheelColliders.Add(wheel);
                }
            }
        }
        if (rightWheels != null)
        {
            foreach (WheelCollider wheel in rightWheels)
            {
                if (wheel != null && !allAvailableWheelColliders.Contains(wheel))
                {
                    allAvailableWheelColliders.Add(wheel);
                }
            }
        }

        // Apply reset to all collected WheelColliders
        foreach (WheelCollider wheel in allAvailableWheelColliders)
        {
            if (wheel != null)
            {
                wheel.motorTorque = 0f;
                wheel.brakeTorque = 0f;

                // Also reset sideways friction on all wheels to a default to prevent residual slipperiness
                // This ensures non-strafing wheels have grip.
                WheelFrictionCurve normalSidewaysFriction = new WheelFrictionCurve();
                normalSidewaysFriction.extremumSlip = 0.2f;
                normalSidewaysFriction.extremumValue = 1f;
                normalSidewaysFriction.asymptoteSlip = 0.5f;
                normalSidewaysFriction.asymptoteValue = 0.75f;
                normalSidewaysFriction.stiffness = 1f;
                wheel.sidewaysFriction = normalSidewaysFriction;

                wheel.steerAngle = 0f; // Reset steer angle too
            }
        }
    }

    // This method (ConfigureStrafingWheels) is REMOVED as there are no WheelColliders for the strafing wheels.

    // Helper for updating single wheel visuals, used within the combined function
    void UpdateSingleWheelVisual(WheelCollider collider, Transform visualWheel)
    {
        if (collider == null || visualWheel == null) return;
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        visualWheel.position = position;
        visualWheel.rotation = rotation;
    }

    // NEW: Combined visual update method to manage all wheel visuals selectively
    // Now takes strafing direction to apply direct visual rotation to strafing visuals.
    void UpdateAllWheelVisualsCombined(bool isStrafing, float strafeDirection)
    {
        // Target visual rotation for strafing wheels (e.g., 90 degrees for sideway movement)
        // Adjust the sign depending on your wheel's local axis and desired turn direction.
        float targetVisualAngleX = strafeDirection ;
    

        // Update visuals for 'SLIPERYROBOT's primary wheels (allWheelCollidersForVisuals)
        // These wheels are still using WheelColliders and their visuals follow GetWorldPose.
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
                visualWheel.rotation = rot; // Normal update for primary wheels
            }
        }

        // Update visuals for 'DriveRobot' specific wheels (leftWheels, rightWheels)
        // These are also assumed to be WheelColliders and their visuals follow GetWorldPose.
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

        // NEW: Manually update the rotation of the CENTER STRAFING VISUALS (no WheelCollider used here)
        if (centerStrafingWheelVisuals != null)
        {
            for (int i = 0; i < centerStrafingWheelVisuals.Length; i++)
            {
                Transform visualWheel = centerStrafingWheelVisuals[i];
                if (visualWheel == null) continue;

                if (isStrafing)
                {
                    // Directly set the local rotation for strafing
                    visualWheel.localRotation = Quaternion.Euler(visualWheel.localEulerAngles.x, visualWheel.localEulerAngles.y, visualWheel.localEulerAngles.z);
                }
                else
                {
                    // Reverts to forward-facing when not strafing
                    visualWheel.localRotation = Quaternion.Euler(visualWheel.localEulerAngles.x, visualWheel.localEulerAngles.y, visualWheel.localEulerAngles.z);
                }
                // Position is not managed here; it's assumed to be handled by parent Rigidbody's movement.
            }
        }
    }

    void OnDrawGizmos()
    {
        if (rb != null)
        {
            Vector3 worldCOM = rb.worldCenterOfMass;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldCOM, 0.05f);
        }
    }
}