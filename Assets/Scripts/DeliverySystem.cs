// DeliverySystem.cs — A4 FINAL
// Uses single packageObject + deliveryObject (matches your scene wiring)
// No Instantiate, no prefabs, no clones
// Flow: package visible → collect → delivery visible → deliver → repeat

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class DeliverySystem : MonoBehaviour
{
    [Header("Scene References")]
    public Transform    player;
    public WorldSpawner worldSpawner;
    public TargetArrow  arrowScript;

    [Header("Objects — drag from Hierarchy")]
    public GameObject packageObject;    // the Package object in scene
    public GameObject deliveryObject;   // the DeliveryTarget object in scene

    [Header("Tuning")]
    public float collectDistance  = 2.5f;
    public float minSpawnDistance = 5f;

    bool worldReady  = false;
    bool gameStarted = false;
    bool hasPackage  = false;  // is player currently carrying a package

    void Start()
    {
        packageObject?.SetActive(false);
        deliveryObject?.SetActive(false);
    }

    public void OnWorldReady()  { worldReady  = true; }

    public void OnGameStarted()
    {
        gameStarted = true;
        if (worldReady) SpawnPackage();
    }

    void Update()
    {
        if (!gameStarted || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            return;

        if (!hasPackage)
        {
            // Show package, wait for player to collect it
            if (!packageObject.activeSelf) return;

            float dist = Vector3.Distance(player.position, packageObject.transform.position);
            if (dist < 4f) GameManager.Instance.ShowPrompt("Drive over the package!");
            else           GameManager.Instance.HidePrompt();

            if (dist < collectDistance)
            {
                hasPackage = true;
                packageObject.SetActive(false);
                GameManager.Instance.OnPackagePickedUp(1);
                GameManager.Instance.SetPackageHUD(1, 0);
                if (arrowScript) arrowScript.target = deliveryObject.transform;
                SpawnDelivery();
            }
        }
        else
        {
            // Player has package, show delivery target
            if (!deliveryObject.activeSelf) return;

            float dist = Vector3.Distance(player.position, deliveryObject.transform.position);
            if (dist < 4f) GameManager.Instance.ShowPrompt("Deliver the package!");
            else           GameManager.Instance.HidePrompt();

            if (dist < collectDistance)
            {
                Debug.Log($"[DS] DELIVERY TRIGGERED at dist={dist:F2}  " +
                          $"player={player.position}  delivery={deliveryObject.transform.position}");
                hasPackage = false;
                deliveryObject.SetActive(false);
                GameManager.Instance.OnDeliveryComplete(1);
                GameManager.Instance.SetPackageHUD(0, 0);
                SpawnPackage();
            }
        }
    }

    void SpawnPackage()
    {
        Vector3 pos = GetRoadPos();
        if (pos == Vector3.zero) { Invoke(nameof(SpawnPackage), 0.5f); return; }
        packageObject.transform.position = pos;
        packageObject.SetActive(true);
        if (arrowScript) arrowScript.target = packageObject.transform;
        Debug.Log($"[DS] Package at {pos}");
    }

    void SpawnDelivery()
    {
        // Must be far from both player AND package
        Vector3 packagePos = packageObject.transform.position;
        Vector3 pos = GetRoadPosFarFrom(packagePos);

        Debug.Log($"[DS] SpawnDelivery: farTile={pos}  player={player.position}  package was at={packagePos}");

        if (pos == Vector3.zero)
        {
            Debug.LogWarning("[DS] No far tile found — using any road tile");
            pos = GetRoadPos();
        }
        if (pos == Vector3.zero)
        {
            Debug.LogError("[DS] No road tiles at all! Cannot spawn delivery.");
            return;
        }

        deliveryObject.transform.position = pos;
        deliveryObject.SetActive(true);
        Debug.Log($"[DS] DeliveryTarget placed at {pos}  dist from player={Vector3.Distance(player.position,pos):F1}");
    }

    Vector3 GetRoadPos()
    {
        var tiles = GetAllRoadTiles();
        if (tiles.Count == 0) return Vector3.zero;
        return tiles[Random.Range(0, tiles.Count)];
    }

    Vector3 GetRoadPosFarFrom(Vector3 avoidPos)
    {
        var tiles = GetAllRoadTiles();
        var far = new List<Vector3>();
        foreach (var t in tiles)
            if (Vector3.Distance(t, avoidPos) > 10f &&
                Vector3.Distance(t, player.position) > 10f)
                far.Add(t);
        if (far.Count > 0) return far[Random.Range(0, far.Count)];
        return Vector3.zero;
    }

    List<Vector3> GetAllRoadTiles()
    {
        var result = new List<Vector3>();
        foreach (var kvp in worldSpawner.GetActiveChunks())
        {
            if (kvp.Value == null) continue;
            var road = kvp.Value.transform.Find("Road")?.GetComponent<Tilemap>();
            if (road == null) continue;
            foreach (var cell in road.cellBounds.allPositionsWithin)
                if (road.HasTile(cell))
                    result.Add(road.GetCellCenterWorld(cell));
        }
        return result;
    }
}