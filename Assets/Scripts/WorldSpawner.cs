// WorldSpawner.cs — A4
// Auto-finds ObstacleSpawner on same GameObject
// Notifies DeliverySystem when world is ready

using UnityEngine;
using System.Collections.Generic;

public class WorldSpawner : MonoBehaviour
{
    public Transform    player;
    public GameObject[] chunkPrefabs;
    public float        chunkSize = 10f;
    public int          radius    = 2;

    // Auto-found in Start() — no need to wire manually
    ObstacleSpawner obstacleSpawner;

    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        // Find ObstacleSpawner on this same GameObject
        obstacleSpawner = GetComponent<ObstacleSpawner>();

        if (obstacleSpawner == null)
            Debug.LogWarning("[WorldSpawner] No ObstacleSpawner component found on this GameObject.");
        else
            Debug.Log("[WorldSpawner] ObstacleSpawner found ✅ — obstacles will spawn on chunks.");

        RefreshGrid();

        // Notify DeliverySystem — direct call, works even when timeScale=0
        var ds = FindAnyObjectByType<DeliverySystem>();
        if (ds != null) ds.OnWorldReady();
        else Debug.LogWarning("[WorldSpawner] No DeliverySystem found in scene.");
    }

    void Update() => RefreshGrid();

    void RefreshGrid()
    {
        Vector2Int playerCoord = WorldToGrid(player.position);

        var desired = new HashSet<Vector2Int>();
        for (int x = playerCoord.x - radius; x <= playerCoord.x + radius; x++)
            for (int y = playerCoord.y - radius; y <= playerCoord.y + radius; y++)
                desired.Add(new Vector2Int(x, y));

        var toRemove = new List<Vector2Int>();
        foreach (var kvp in activeChunks)
            if (!desired.Contains(kvp.Key)) toRemove.Add(kvp.Key);

        foreach (var coord in toRemove)
        {
            Destroy(activeChunks[coord]);
            activeChunks.Remove(coord);
        }

        foreach (var coord in desired)
            if (!activeChunks.ContainsKey(coord))
                PlaceChunk(coord);
    }

    int PickCompatiblePrefab(Vector2Int coord)
    {
        ChunkPorts required = ChunkPorts.None;
        ChunkPorts forbidden = ChunkPorts.None;

        CheckNeighbour(coord + Vector2Int.up,    ref required, ref forbidden, ChunkPorts.South, ChunkPorts.North);
        CheckNeighbour(coord + Vector2Int.down,  ref required, ref forbidden, ChunkPorts.North, ChunkPorts.South);
        CheckNeighbour(coord + Vector2Int.right, ref required, ref forbidden, ChunkPorts.West,  ChunkPorts.East);
        CheckNeighbour(coord + Vector2Int.left,  ref required, ref forbidden, ChunkPorts.East,  ChunkPorts.West);

        var candidates = new List<int>();
        for (int i = 0; i < chunkPrefabs.Length; i++)
        {
            ChunkData cd = chunkPrefabs[i].GetComponent<ChunkData>();
            if (cd == null) continue;
            if ((cd.ports & required)  != required)        continue;
            if ((cd.ports & forbidden) != ChunkPorts.None) continue;
            candidates.Add(i);
        }

        if (candidates.Count == 0) { Debug.LogWarning("[WorldSpawner] No compatible chunk found!"); return 0; }
        return candidates[Random.Range(0, candidates.Count)];
    }

    void CheckNeighbour(Vector2Int nCoord,
        ref ChunkPorts required, ref ChunkPorts forbidden,
        ChunkPorts nPort, ChunkPorts myPort)
    {
        if (!activeChunks.TryGetValue(nCoord, out GameObject chunk)) return;
        ChunkData d = chunk?.GetComponent<ChunkData>();
        if (d == null) return;
        if (d.HasPort(nPort)) required |= myPort;
        else                  forbidden |= myPort;
    }

    void PlaceChunk(Vector2Int coord)
    {
        int        idx   = PickCompatiblePrefab(coord);
        GameObject chunk = Instantiate(chunkPrefabs[idx], GridToWorld(coord), Quaternion.identity);
        activeChunks[coord] = chunk;

        if (obstacleSpawner != null)
            obstacleSpawner.SpawnOnChunk(chunk);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos) => new Vector2Int(
        Mathf.FloorToInt(worldPos.x / chunkSize),
        Mathf.FloorToInt(worldPos.y / chunkSize));

    Vector3 GridToWorld(Vector2Int coord) =>
        new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0f);

    public Dictionary<Vector2Int, GameObject> GetActiveChunks() => activeChunks;
    public bool IsChunkActive(Vector2Int coord) => activeChunks.ContainsKey(coord);
}