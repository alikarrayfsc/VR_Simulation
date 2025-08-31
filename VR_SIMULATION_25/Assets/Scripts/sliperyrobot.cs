using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine;

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

        float horizontalArrowInput = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) horizontalArrowInput = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) horizontalArrowInput = -1f;

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
    {
        if (collider == null || visualWheel == null) return;
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        visualWheel.position = position;
        visualWheel.rotation = rotation;
    }

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
        }

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
