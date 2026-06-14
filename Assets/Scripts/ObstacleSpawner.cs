// ObstacleSpawner.cs — A4 Bonus
// Spawns cones, rocks, oil on road tiles when a chunk is placed.
// Added: detailed logging + nearest obstacle HUD notification

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject conePrefab;
    public GameObject rockPrefab;
    public GameObject oilPrefab;

    [Range(0f, 0.2f)] public float spawnChance = 0.05f;

    // All active obstacles tracked for proximity warning
    static List<GameObject> allObstacles = new List<GameObject>();

    // ── Called by WorldSpawner when a chunk is placed ─────────────────────────
    public void SpawnOnChunk(GameObject chunk)
    {
        if (conePrefab == null && rockPrefab == null && oilPrefab == null)
        {
            Debug.LogWarning("[Obstacle] No prefabs assigned! " +
                             "Wire Cone/Rock/Oil prefabs in WorldSpawner → Obstacle Spawner.");
            return;
        }

        Tilemap road = chunk.transform.Find("Road")?.GetComponent<Tilemap>();
        if (road == null)
        {
            Debug.LogWarning($"[Obstacle] No 'Road' tilemap in chunk {chunk.name}");
            return;
        }

        int spawned = 0;
        foreach (Vector3Int cell in road.cellBounds.allPositionsWithin)
        {
            if (!road.HasTile(cell)) continue;
            if (Random.value > spawnChance) continue;

            float r = Random.value;
            GameObject prefab;
            string kind;

            if (r < 0.4f)       { prefab = oilPrefab;  kind = "Oil";  }
            else if (r < 0.65f) { prefab = rockPrefab; kind = "Rock"; }
            else                { prefab = conePrefab; kind = "Cone"; }

            if (prefab == null) continue;

            Vector3 pos = road.GetCellCenterWorld(cell);
            GameObject obj = Instantiate(prefab, pos, Quaternion.identity, chunk.transform);
            obj.SetActive(true);
            allObstacles.Add(obj);
            spawned++;
            Debug.Log($"[Obstacle] Spawned {kind} at {pos} in {chunk.name}");
        }

        if (spawned == 0)
            Debug.LogWarning($"[Obstacle] 0 obstacles spawned in {chunk.name} " +
                             $"(spawnChance={spawnChance}, prefabs: " +
                             $"cone={conePrefab!=null} rock={rockPrefab!=null} oil={oilPrefab!=null})");
        else
            Debug.Log($"[Obstacle] Spawned {spawned} obstacles in {chunk.name}");
    }

    // ── Called every frame by ObstacleNotifier to get nearest obstacle ────────
    public static (float dist, string kind) GetNearestObstacle(Vector3 playerPos)
    {
        float  bestDist = float.MaxValue;
        string bestKind = "";

        // Clean up destroyed obstacles
        allObstacles.RemoveAll(o => o == null);

        foreach (var obs in allObstacles)
        {
            if (obs == null || !obs.activeSelf) continue;
            float d = Vector3.Distance(playerPos, obs.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                var obstacle = obs.GetComponent<Obstacle>();
                bestKind = obstacle != null ? obstacle.kind.ToString() : "Obstacle";
            }
        }
        return (bestDist, bestKind);
    }

    // Clean up list when chunks unload
    void OnDestroy()
    {
        allObstacles.RemoveAll(o => o == null);
    }
}
