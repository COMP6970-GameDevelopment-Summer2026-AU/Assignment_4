// TargetSpawner.cs — DEBUG VERSION
// Logs every time it tries to spawn or collect so we can see if it's still running

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TargetSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject target;
    public WorldSpawner worldSpawner;

    float minSpawnDistance = 10f;
    float collectDistance  = 1f;

    void Start()
    {
        Debug.LogError("[TargetSpawner] START CALLED — this script is ACTIVE and will conflict! " +
                       "Disable the TargetSpawner GameObject in the Hierarchy NOW.");
        Invoke(nameof(SpawnTargetInitial), 0.1f);
    }

    void SpawnTargetInitial()
    {
        Debug.LogError("[TargetSpawner] SpawnTargetInitial — spawning target!");
        SpawnTarget(true);
    }

    void SpawnTarget(bool ignoreDistance)
    {
        Debug.LogError($"[TargetSpawner] SpawnTarget(ignoreDistance={ignoreDistance}) called!");

        List<Vector3> candidates = new List<Vector3>();
        foreach (var kvp in worldSpawner.GetActiveChunks())
        {
            GameObject chunkObject = kvp.Value;
            if (chunkObject == null) continue;
            if (!ignoreDistance && Vector3.Distance(player.position, chunkObject.transform.position) < minSpawnDistance) continue;
            Tilemap road = chunkObject.transform.Find("Road")?.GetComponent<Tilemap>();
            if (road == null) continue;
            foreach (Vector3Int cellPosition in road.cellBounds.allPositionsWithin)
                if (road.HasTile(cellPosition))
                    candidates.Add(road.GetCellCenterWorld(cellPosition));
        }

        if (candidates.Count == 0) { Debug.LogWarning("[TargetSpawner] No candidates!"); return; }

        target.transform.position = candidates[Random.Range(0, candidates.Count)];
        target.SetActive(true);
        Debug.LogError($"[TargetSpawner] Target moved to {target.transform.position}");
    }

    void Update()
    {
        if (target == null || player == null || worldSpawner == null) return;

        float distance = Vector3.Distance(player.position, target.transform.position);
        if (distance < collectDistance)
        {
            Debug.LogError($"[TargetSpawner] COLLECTED target at dist={distance:F2} — respawning!");
            SpawnTarget(false);
            return;
        }

        Vector2Int targetCoord = worldSpawner.WorldToGrid(target.transform.position);
        if (!worldSpawner.IsChunkActive(targetCoord))
        {
            Debug.LogError("[TargetSpawner] Target chunk unloaded — respawning!");
            SpawnTarget(false);
        }
    }
}