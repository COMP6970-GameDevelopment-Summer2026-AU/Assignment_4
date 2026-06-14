// ObstacleNotifier.cs — A4 Bonus
// Attach to Player.
// Shows a warning in the HUD when an obstacle is nearby.
// Works in both Auto and Manual play mode.

using UnityEngine;

public class ObstacleNotifier : MonoBehaviour
{
    [Header("Warning distance")]
    public float warningDistance = 3f;

    float checkInterval = 0.2f;  // check every 0.2s not every frame
    float nextCheck     = 0f;

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;

        var (dist, kind) = ObstacleSpawner.GetNearestObstacle(transform.position);

        if (dist < warningDistance && !string.IsNullOrEmpty(kind))
        {
            string warn = kind == "Oil"
                ? $"⚠ Oil spill ahead! ({dist:F1}m)"
                : $"⚠ {kind} ahead! ({dist:F1}m)";
            GameManager.Instance.ShowPrompt(warn);
            Debug.Log($"[ObstacleNotifier] {warn}");
        }
    }
}
