using UnityEngine;
using UnityEngine.XR; // <-- Important for CommonUsages

public class ROTATER : MonoBehaviour
{
    RigidbodyConstraints rc = RigidbodyConstraints.FreezeRotationX;
    public float torqueAmount = 10f;
    private Rigidbody rb;

    public float speedThreshold = 0.05f;
    public float checkTime = 1f;
    private float timer = 0f;

    public bool isTryingToClimb = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void rotater()
    {
        bool buttonPressed = false;

        // Get right-hand controller
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // Check if the "primaryButton" (A on Oculus) is pressed
        if (rightHand.isValid)
        {
            rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed);
        }

        if (buttonPressed)
        {
            rb.AddTorque(Vector3.right * torqueAmount);
            rb.constraints = RigidbodyConstraints.None;
            isTryingToClimb = true;
        }
        else
        {
            rb.constraints = rc;
            isTryingToClimb = false;
        }
    }

    void strugglecheck()
    {
        if (isTryingToClimb)
        {
            if (Mathf.Abs(rb.velocity.y) < speedThreshold)
                timer += Time.deltaTime;
            else
                timer = 0f;

            if (timer > checkTime)
            {
                Debug.Log("Robot is struggling!");
                torqueAmount += 0.05f;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    void FixedUpdate()
    {
        rotater();
        strugglecheck();
    }
}
