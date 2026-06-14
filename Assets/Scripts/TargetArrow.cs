// TargetArrow.cs — A4 extension of skeleton
// Added: arrow follows player position (offset above car)
// Original only rotated — it stayed frozen at its scene position.
using UnityEngine;

public class TargetArrow : MonoBehaviour
{
    public Transform player;
    public Transform target;

    void Update()
    {
        if (player == null) return;

        // Follow player
        transform.position = new Vector3(player.position.x, player.position.y + 1.2f, 0f);

        // Hide when no target
        bool hasTarget = target != null && target.gameObject.activeInHierarchy;
        GetComponent<SpriteRenderer>().enabled = hasTarget;
        if (!hasTarget) return;

        // Point at target
        Vector2 dir = target.position - player.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
