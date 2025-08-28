using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ROTATER : MonoBehaviour
{
    RigidbodyConstraints rc = RigidbodyConstraints.FreezeRotationX;
    public float torqueAmount = 10f;
    private Rigidbody rb;
    public float speedThreshold = 0.05f;
    public float checkTime = 1f;
    private float timer = 0f;
    public bool isTryingToClimb = false;
    public float appliedForce = 0f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void rotater()
    {
        float input = Input.GetAxis("Fire1");
        rb.AddTorque(Vector3.right * input * torqueAmount);
        if(input !=0)
        {
           rb.constraints = RigidbodyConstraints.None;
            isTryingToClimb = true;
        }
        else{
            
          
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

            if (timer > checkTime){
                Debug.Log("Robot is struggling!");
                torqueAmount = torqueAmount + 0.05f;}
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
