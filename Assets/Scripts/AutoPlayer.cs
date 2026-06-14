// AutoPlayer.cs — A4 FINAL
// Auto-drives car to nearest package then to delivery target.
// TAB toggles auto/manual during play.
// Only runs when game IsPlaying (not on start screen).

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class AutoPlayer : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed   = 4f;
    public float turnSpeed   = 150f;
    public bool  autoEnabled = true;

    [Header("Auto-found at runtime")]
    public DeliverySystem deliverySystem;
    public CarController  carController;
    public WorldSpawner   worldSpawner;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (deliverySystem == null) deliverySystem = FindAnyObjectByType<DeliverySystem>();
        if (carController  == null) carController  = GetComponent<CarController>();
        if (worldSpawner   == null) worldSpawner   = FindAnyObjectByType<WorldSpawner>();
    }

    void Update()
    {
        // TAB toggles auto/manual
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            autoEnabled = !autoEnabled;
            if (carController != null) carController.enabled = !autoEnabled;
            Debug.Log("[Auto] " + (autoEnabled ? "AUTO" : "MANUAL"));
        }
        // Keep CarController state in sync
        if (carController != null && carController.enabled == autoEnabled)
            carController.enabled = !autoEnabled;
    }

    void FixedUpdate()
    {
        if (!autoEnabled) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (deliverySystem == null) return;

        // Ask the TargetArrow what it's pointing at — that's our current target
        Transform target = GetCurrentTarget();
        if (target == null) return;

        Vector2 toTarget = (Vector2)(target.position - transform.position);
        float   dist     = toTarget.magnitude;

        // Steer toward target
        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f;
        float angle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle,
                                              turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(angle);

        // Drive forward, slow when close
        float speed = dist > 1.5f ? moveSpeed : moveSpeed * 0.4f;
        rb.linearVelocity = transform.up * speed;
    }

    // Use the TargetArrow's current target — it already knows whether
    // to point at a package or the delivery point
    Transform GetCurrentTarget()
    {
        var arrow = deliverySystem.arrowScript;
        if (arrow != null && arrow.target != null && arrow.target.gameObject.activeInHierarchy)
            return arrow.target;

        // Fallback: delivery object
        if (deliverySystem.deliveryObject != null &&
            deliverySystem.deliveryObject.activeInHierarchy)
            return deliverySystem.deliveryObject.transform;

        return null;
    }
}
