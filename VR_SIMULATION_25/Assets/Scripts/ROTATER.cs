using UnityEngine;
using UnityEngine.InputSystem;

public class ROTATER : MonoBehaviour
{
    RigidbodyConstraints rc = RigidbodyConstraints.FreezeRotationX;
    public float torqueAmount = 10f;
    private Rigidbody rb;
    public float speedThreshold = 0.05f;
    public float checkTime = 1f;
    private float timer = 0f;
    public bool isTryingToClimb = false;

    [SerializeField] private InputActionProperty rotateAction;
    // Bind this to "A" button on Right Controller in Inspector

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void rotater()
    {
        bool isPressed = rotateAction.action.IsPressed(); // true while holding A

        if (isPressed)
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
