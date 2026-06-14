// CarController.cs — A4 extension of skeleton
// Kept skeleton logic exactly. Added: ApplySpeedMod() for oil obstacles.
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    float acceleration = 5f;
    float deceleration = 5f;
    float maxSpeed     = 6f;
    float turnSpeed    = 200f;

    Rigidbody2D rb;
    Vector2 moveInput;
    float currentSpeed = 0f;

    float speedMod = 1f;
    Coroutine speedModRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        float moveAmount = moveInput.y;
        float effectiveMax = maxSpeed * speedMod;

        if (moveAmount != 0)
        {
            currentSpeed += moveAmount * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -effectiveMax, effectiveMax);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        float turnAmount = -moveInput.x * turnSpeed * Time.fixedDeltaTime;
        if (Mathf.Abs(currentSpeed) > 0.1f)
            rb.MoveRotation(rb.rotation + turnAmount * Mathf.Sign(currentSpeed));

        rb.linearVelocity = transform.up * currentSpeed;
    }

    // Called by Obstacle.cs (oil spill)
    public void ApplySpeedMod(float modifier, float duration)
    {
        if (speedModRoutine != null) StopCoroutine(speedModRoutine);
        speedModRoutine = StartCoroutine(SpeedModCo(modifier, duration));
    }

    System.Collections.IEnumerator SpeedModCo(float mod, float dur)
    {
        speedMod = mod;
        yield return new WaitForSeconds(dur);
        speedMod = 1f;
        speedModRoutine = null;
    }
}
