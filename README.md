# Endless Courier 2D — Assignment 4
**Course:** COMP 6970 | Summer 2026
**Developer:** Jahidul Arafat
**Engine:** Unity 2D (Universal Render Pipeline)

---
```aiignore
# 1. Install Git LFS (one time per machine)
git lfs install

# 2. Clone the repo
git clone https://github.com/COMP6970-GameDevelopment-Summer2026-AU/Assignment_4.git

# 3. Pull all LFS files (sprites, audio)
git lfs pull

# 4. Open the project in Unity
# Unity will reimport assets automatically
```

---

## Gameplay Demo

[![Endless Courier 2D — Assignment 4 Demo](https://img.youtube.com/vi/laaz0N-OjtI/maxresdefault.jpg)](https://youtu.be/laaz0N-OjtI)

> Click the thumbnail above to watch the full gameplay demo on YouTube.


## How to Run

1. Open the project in Unity
2. Open `Assets/Scenes/SampleScene.unity`
3. Press **Play**
4. Click **START GAME** on the start screen
5. Use **WASD** or **Arrow Keys** to drive
6. Press **TAB** to toggle Auto-drive mode

---

## Gameplay

The player drives a car through an endlessly generating road network. Packages appear on road tiles — drive over them to collect, then deliver to the trophy icon for points. The goal is to complete as many deliveries as possible before the 90-second timer runs out.

---

## Requirements Coverage

### Requirement 1 — Delivery target appears after package pickup
| Detail | Implementation |
|--------|---------------|
| When the car drives over a package icon it disappears | `DeliverySystem.Update()` — proximity check within `2.5` units |
| A trophy icon immediately appears on a road tile | `DeliverySystem.SpawnDelivery()` — placed at least 10 units away |
| Arrow above car redirects from package to trophy | `TargetArrow.Update()` — `arrowScript.target` switched by DeliverySystem |

**Script:** `DeliverySystem.cs`

---

### Requirement 2 — Player must deliver to the delivery target
| Detail | Implementation |
|--------|---------------|
| Trophy icon visible on road at all times | `deliveryObject.SetActive(true)` after package collected |
| Delivery only triggers when player reaches the trophy | Distance check `< 2.5` units in `DeliverySystem.Update()` |
| Trophy disappears after delivery | `deliveryObject.SetActive(false)` |

**Script:** `DeliverySystem.cs`

---

### Requirement 3 — New package appears after each delivery
| Detail | Implementation |
|--------|---------------|
| After delivery completes, `SpawnPackage()` is called | `DeliverySystem.Update()` delivery block → `SpawnPackage()` |
| Package placed on a random road tile | Scans all active chunk road tiles, picks randomly |
| Loop continues indefinitely until timer runs out | No end condition — repeats until `GameManager.EndGame()` |

**Script:** `DeliverySystem.cs`

---

### Requirement 4 — Timer that limits gameplay
| Detail | Implementation |
|--------|---------------|
| 90-second countdown timer | `totalTime = 90f` in `GameManager` |
| Timer decrements every frame | `timeRemaining -= Time.deltaTime` in `GameManager.Update()` |
| Game ends when timer reaches zero | `if (timeRemaining <= 0f) EndGame()` |

**Script:** `GameManager.cs`

---

### Requirement 5 — Timer displayed on screen
| Detail | Implementation |
|--------|---------------|
| Timer shown as `Time: MM:SS` | `TimerText` — top center of screen, yellow |
| Turns red when below 15 seconds | `timerText.color = timeRemaining < 15f ? Color.red : yellow` |
| Built automatically | `UIBuilder.cs` → Tools → Courier Rush → Build UI |

**Scripts:** `GameManager.cs`, `UIBuilder.cs`

---

### Requirement 6 — Delivery score counter
| Detail | Implementation |
|--------|---------------|
| Score increments on each delivery | `score++` in `GameManager.OnDeliveryComplete()` |
| Score resets to 0 on game restart | `score = 0` in `GameManager.StartGame()` |
| Score shown in end-game report | Full breakdown in `GameManager.EndGame()` |

**Script:** `GameManager.cs`

---

### Requirement 7 — Score displayed on screen
| Detail | Implementation |
|--------|---------------|
| Live score shown as `Score: X` | `ScoreText` — top left, green |
| Updates every frame | `GameManager.UpdateHUD()` called in `Update()` |
| Final score in game over screen | `FinalReportText` — large, center screen |

**Scripts:** `GameManager.cs`, `UIBuilder.cs`

---

### Requirement 8 — Background music
| Detail | Implementation |
|--------|---------------|
| Music plays from game start | `musicSource.Play()` in `GameManager.Start()` |
| Loops continuously | `musicSource.loop = true` |
| Volume set to comfortable level | `musicSource.volume = 0.4f` |
| Audio clip assigned in Inspector | `backgroundMusic` slot on `_GameManager` |

**Script:** `GameManager.cs`

---

### Requirement 9 — Sound effects on pickup and delivery
| Detail | Implementation |
|--------|---------------|
| Pickup sound on package collection | `sfxSource.PlayOneShot(pickupSound)` in `OnPackagePickedUp()` |
| Delivery sound on trophy collection | `sfxSource.PlayOneShot(deliverySound)` in `OnDeliveryComplete()` |
| Bump sound on wall collision | `AudioSource.PlayOneShot(BumpSound)` in `CarController.OnCollisionEnter2D()` |

**Script:** `GameManager.cs`, `CarController.cs`

---

### Requirement 10 — At least four new road chunk prefabs
Four new prefabs created in addition to the 9 skeleton chunks:

| Chunk Name | Ports | Road Shape | Different From Skeleton? |
|------------|-------|------------|--------------------------|
| `Chunk_StraightNS` | N+S | Vertical straight corridor | ✅ New |
| `Chunk_StraightEW` | E+W | Horizontal straight corridor | ✅ New |
| `Chunk_TJunctionLeft` | N+S+W | T-junction opening left | ✅ New |
| `Chunk_DeadEndS` | S | Dead end opening south | ✅ New |

All new chunks have:
- `Ground` tilemap (grass tiles)
- `Road` tilemap with `Rigidbody2D` (Static) + `TilemapCollider2D` + `CompositeCollider2D`
- `ChunkData` component with correct port flags
- Added to `WorldSpawner → Chunk Prefabs` array

**Script:** `WorldSpawner.cs`, `ChunkData.cs`

---

### Requirement 11 — Complete gameplay loop

| Feature | Implementation |
|---------|---------------|
| **Start Screen** | Black panel with title, subtitle, controls hint, START button |
| **START button** | Calls `GameManager.StartGame()` — starts timer, spawns first package |
| **Car movement** | `CarController.cs` — WASD/arrows, acceleration, deceleration, turning |
| **Camera follow** | `CameraFollow.cs` — smooth lerp toward player |
| **Road generation** | `WorldSpawner.cs` — streams chunks in radius=2 around player |
| **Package pickup** | `DeliverySystem.cs` — proximity trigger, arrow updates |
| **Delivery target** | `DeliverySystem.cs` — spawns far from player after pickup |
| **Timer** | `GameManager.cs` — 90 seconds, red flash below 15s |
| **Score** | `GameManager.cs` — increments per delivery |
| **Music** | `GameManager.cs` — looping background audio |
| **SFX** | `GameManager.cs` + `CarController.cs` — pickup, delivery, bump |
| **Game Over Screen** | Full summary report with all stats |
| **PLAY AGAIN** | `GameManager.RestartGame()` — reloads scene |

---

## Bonus — Obstacle System (+10 pts)

### Three obstacle types:

| Obstacle | Sprite | Collider | Effect |
|----------|--------|----------|--------|
| **Cone** | `cone_straight.png` | Solid (Is Trigger = OFF) | Blocks car, plays bump sound, shows "Hit a Cone!" popup |
| **Rock** | `rock1.png` | Solid (Is Trigger = OFF) | Blocks car, plays bump sound, shows "Hit a Rock!" popup |
| **Oil Spill** | `oil.png` | Trigger (Is Trigger = ON) | Slows car to 40% speed for 3 seconds, shows warning popup |

### How obstacles spawn:
- `ObstacleSpawner.SpawnOnChunk()` is called by `WorldSpawner` every time a new chunk is placed
- 5% of road tiles (`spawnChance = 0.05`) randomly receive an obstacle
- Obstacles are children of their chunk — auto-destroyed when chunk unloads
- Distribution: 40% oil, 25% rock, 35% cone

### Proximity warning system:
- `ObstacleNotifier` component on Player checks every 0.2 seconds
- When an obstacle is within 3 units: prompt shows `"⚠ Rock ahead! (2.3m)"`
- Works in both manual and auto-drive modes

**Scripts:** `Obstacle.cs`, `ObstacleSpawner.cs`, `ObstacleNotifier.cs`

---

## End-Game Summary Report

When the timer runs out the game over screen shows a full report:

```
TIME'S UP!

Final Score:  7

──────────────────────
     GAME SUMMARY
──────────────────────

Packages Collected     8
Deliveries Made        7
Delivery Rate         88%

── Obstacles Hit ──
Cones Hit              3
Rocks Hit              1
Oil Spills             2
Total Obstacles        6

── Time ──
Time Limit            90s
Time Used             90s
```

---

## Project Structure

```
Assets/
├── Audio/
│   ├── pickup.ogg
│   ├── success.ogg
│   └── bump.ogg
├── Prefabs/
│   ├── Chunks/          ← 13 road chunks (9 skeleton + 4 new)
│   ├── Cone.prefab
│   ├── Rock.prefab
│   ├── OilSpill.prefab
│   ├── Package.prefab
│   └── DeliveryTarget.prefab
├── Scripts/
│   ├── CarController.cs
│   ├── CameraFollow.cs
│   ├── ChunkData.cs
│   ├── WorldSpawner.cs
│   ├── GameManager.cs
│   ├── DeliverySystem.cs
│   ├── TargetArrow.cs
│   ├── Obstacle.cs
│   ├── ObstacleSpawner.cs
│   ├── ObstacleNotifier.cs
│   ├── AutoPlayer.cs
│   └── UIBuilder.cs
├── Scenes/
│   └── SampleScene.unity
├── Sprites/
│   ├── car.png
│   ├── arrow.png
│   ├── target.png
│   ├── trophy.png
│   ├── cone_straight.png
│   ├── rock1.png
│   └── oil.png
└── Tiles/
    ├── road tiles
    └── grass tiles
```

---