// Obstacle.cs — A4 Bonus
// Cone/Rock: solid — blocks car, reports hit to GameManager
// Oil: trigger — slows car, reports hit to GameManager

using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public enum ObstacleKind { Cone, Rock, Oil }
    public ObstacleKind kind = ObstacleKind.Cone;

    [Header("Oil Settings")]
    public float oilSlowAmount   = 0.4f;
    public float oilSlowDuration = 3f;

    void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"[Obstacle] {gameObject.name} has NO Collider2D!");
            return;
        }
        Debug.Log($"[Obstacle] {kind} ready at {transform.position}  isTrigger={col.isTrigger}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (kind != ObstacleKind.Oil) return;
        if (!other.CompareTag("Player")) return;

        var car = other.GetComponent<CarController>();
        if (car != null)
        {
            car.ApplySpeedMod(oilSlowAmount, oilSlowDuration);
            GameManager.Instance?.OnObstacleHit("Oil");
            GameManager.Instance?.ShowPopup($"Oil spill! Slowed for {oilSlowDuration}s",
                                             new Color(0.8f, 0.6f, 0f));
            Debug.Log("[Obstacle] Oil hit!");
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (kind == ObstacleKind.Oil) return;
        if (!col.gameObject.CompareTag("Player")) return;

        GameManager.Instance?.OnObstacleHit(kind.ToString());
        GameManager.Instance?.ShowPopup($"Hit a {kind}!", Color.red);
        Debug.Log($"[Obstacle] {kind} hit by player!");
    }
}
