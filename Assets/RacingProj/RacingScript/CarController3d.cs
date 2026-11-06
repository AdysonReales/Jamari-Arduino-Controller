using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController2D : MonoBehaviour
{
    public float acceleration = 15f;
    public float steering = 90f;
    public float maxSpeed = 10f;
    public float driftFactor = 0.95f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = 1f;
        rb.angularDamping = 2f;
        rb.centerOfMass = Vector2.zero;
    }

    void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Vertical");   // W/S
        float turnInput = Input.GetAxis("Horizontal"); // A/D

        if (Mathf.Abs(moveInput) > 0.01f && rb.linearVelocity.magnitude < maxSpeed)
            rb.AddForce(transform.up * moveInput * acceleration);

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float steerAmount = turnInput * steering * Time.fixedDeltaTime;
            float direction = (moveInput >= 0) ? 1f : -1f;
            rb.MoveRotation(rb.rotation - steerAmount * direction);
        }

        Vector2 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        localVel.x *= driftFactor;
        rb.linearVelocity = transform.TransformDirection(localVel);
    }
}
