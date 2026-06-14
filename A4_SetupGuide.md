# Assignment 4 — Endless Courier 2D
## Complete Setup Guide
**COMP 6910 | Summer 2026 | Jahidul Arafat**

---

## Scripts Overview

| File | Status | Purpose |
|------|--------|---------|
| `CarController.cs` | Replace | Car movement + oil slow effect |
| `WorldSpawner.cs` | Replace | Chunk streaming + obstacle spawning |
| `TargetArrow.cs` | Replace | Arrow follows player, points to target |
| `GameManager.cs` | NEW | Timer, score, music, SFX, screens |
| `DeliverySystem.cs` | NEW | Package pickup → delivery loop |
| `UIBuilder.cs` | NEW | Builds entire UI in one click |
| `Obstacle.cs` | NEW | Cone/rock/oil behavior |
| `ObstacleSpawner.cs` | NEW | Spawns obstacles on road tiles |
| `ObstacleNotifier.cs` | NEW | Warns player of nearby obstacles |
| `AutoPlayer.cs` | NEW | AI auto-drives for demo/testing |
| `TargetSpawner.cs` | Replace | Debug version — logs if still active |
| `ChunkData.cs` | Keep original | No changes needed |
| `CameraFollow.cs` | Keep original | No changes needed |

**Copy all replaced/new files into `Assets/Scripts/`. Wait for 0 errors before continuing.**

---

## STEP 1 — Fix ChunkData on chunk prefabs

7 of the 13 chunk prefabs are missing correct `ChunkData` ports. Without this the car always starts in a corner/grass pocket.

### How to fix each prefab:
Project → `Assets/Prefabs/Chunks/` → double-click prefab → Add Component → `ChunkData` → tick ports → Ctrl+S → back arrow

| Prefab | North | South | East | West | Bits |
|--------|:-----:|:-----:|:----:|:----:|------|
| `Chunk_Crossroads` | ✅ | ✅ | ✅ | ✅ | 15 |
| `Chunk_StraightNS` | ✅ | ✅ | | | 3 |
| `Chunk_StraightEW` | | | ✅ | ✅ | 12 |
| `Chunk_DeadEndN` | ✅ | | | | 1 |
| `Chunk_Loop` | ✅ | | ✅ | | 5 |
| `Chunk_Roundabout` | ✅ | ✅ | ✅ | ✅ | 15 |
| `Chunk_Zigzag` | ✅ | | ✅ | | 5 |

> The other 6 (`BottomLeft`, `BottomRight`, `TopLeft`, `TopRight`, `TJunctionTop`, `TJunctionBottom`) already have correct ports — leave them alone.

---

## STEP 2 — Disable TargetSpawner

| Action | Where |
|--------|-------|
| Click `TargetSpawner` | Hierarchy |
| Uncheck checkbox at top of Inspector | Inspector |
| Do NOT delete it | — |
| Ctrl+S to save | — |

---

## STEP 3 — Create Package and DeliveryTarget

### Package object:

| Setting | Value |
|---------|-------|
| Name | `Package` |
| Component | Sprite Renderer |
| Sprite | `target.png` |
| Order in Layer | `6` |
| Active at start | ❌ unchecked |

### DeliveryTarget object:

| Setting | Value |
|---------|-------|
| Name | `DeliveryTarget` |
| Component | Sprite Renderer |
| Sprite | `trophy.png` |
| Order in Layer | `10` |
| Active at start | ❌ unchecked |

### Fix sprite transparency (both sprites):
Project → select sprite → Inspector → **Alpha Is Transparency = ✅** → **Apply**

---

## STEP 4 — Create _GameManager

1. Hierarchy → right-click → Create Empty → name `_GameManager`
2. Add Component → `GameManager`
3. Add Component → `Audio Source` ← music
4. Add Component → `Audio Source` ← SFX (add a second one)

### Wire GameManager slots:

| Slot | Value |
|------|-------|
| Total Time | `90` |
| Music Source | first AudioSource on `_GameManager` |
| Background Music | `bump.ogg` from `Assets/Audio/` |
| Sfx Source | second AudioSource |
| Pickup Sound | `pickup.ogg` |
| Delivery Sound | `success.ogg` |

> Timer Text, Score Text, Status Text, Package Count Text, Prompt, Popup, Start Screen, Game Over Screen, Final Score Text → **leave empty — UIBuilder fills these in Step 6.**

---

## STEP 5 — Create DeliveryManager

1. Hierarchy → right-click → Create Empty → name `DeliveryManager`
2. Add Component → `DeliverySystem`

### Wire DeliverySystem slots:

| Slot | Value |
|------|-------|
| Player | drag `Player` from Hierarchy |
| World Spawner | drag `WorldSpawner` from Hierarchy |
| Arrow Script | drag `Arrow` from Hierarchy |
| Package Object | drag `Package` from Hierarchy |
| Delivery Object | drag `DeliveryTarget` from Hierarchy |
| Collect Distance | `2.5` |
| Min Spawn Distance | `5` |

---

## STEP 6 — Build the UI

1. Confirm `_GameManager` is in scene
2. Unity menu → **Tools → Courier Rush → Build UI**
3. Confirm dialog appears

### What gets built automatically:

| Element | Position | Color |
|---------|----------|-------|
| `TimerText` | Top center | Yellow → Red below 15s |
| `ScoreText` | Top left | Green |
| `PackageCountText` | Top right | Orange/Blue |
| `StatusText` | Bottom center | Cyan bold |
| `PromptText` | Above status | Pulsing white/yellow |
| `PopupText` | Screen center | Varies — fades out |
| `StartScreen` | Full screen | Black panel + title + START button |
| `GameOverScreen` | Full screen | Black panel + score + PLAY AGAIN |

> All GameManager text slots and button OnClick events wired automatically.

---

## STEP 7 — Script Execution Order

Prevents startup hang where DeliverySystem runs before WorldSpawner.

| Step | Action |
|------|--------|
| 1 | Edit → Project Settings → Script Execution Order |
| 2 | Click **+** → select `WorldSpawner` |
| 3 | Set value to `-100` |
| 4 | Click **Apply** |

---

## STEP 8 — Tags

| Step | Action |
|------|--------|
| 1 | Hierarchy → click `Player` |
| 2 | Inspector → Tag dropdown → **Player** |
| 3 | If missing: Edit → Project Settings → Tags and Layers → add `Player` |

---

## STEP 9 — Wire CameraFollow

| Slot | Value |
|------|-------|
| Target | drag `Player` from Hierarchy |

Click `Main Camera` → `CameraFollow` component → wire above.

---

## STEP 10 — Wire Arrow

Click `Arrow` in Hierarchy → `Target Arrow` component:

| Slot | Value |
|------|-------|
| Player | drag `Player` from Hierarchy |
| Target | leave empty (DeliverySystem sets at runtime) |

---

## STEP 11 — AutoPlayer (optional demo mode)

1. Click `Player` in Hierarchy → **Add Component → AutoPlayer**

| Slot | Value |
|------|-------|
| Auto Enabled | ✅ checked = auto drives |
| Move Speed | `4` |
| Turn Speed | `150` |
| Delivery System | leave empty (auto-found) |
| Car Controller | leave empty (auto-found) |

> Press **TAB** during play to toggle between Auto and Manual mode.

---

## STEP 12 — Play Test Checklist

Press Play and verify:

### Start screen:
| Check | Expected |
|-------|----------|
| Start screen visible | ✅ |
| Background music playing | ✅ |
| Game paused on start screen | ✅ |

### Gameplay:
| Check | Expected |
|-------|----------|
| Click START → timer counts down | ✅ |
| Package icon appears on road | ✅ |
| Arrow points to package | ✅ |
| Status = "Find the package!" | ✅ |
| WASD / arrows move car | ✅ |
| Camera follows car | ✅ |
| New chunks appear while driving | ✅ |

### Package pickup:
| Check | Expected |
|-------|----------|
| Drive over package → disappears | ✅ |
| Pickup sound plays | ✅ |
| Status → "Carrying 1 — deliver it!" | ✅ |
| Trophy appears on road | ✅ |
| Arrow redirects to trophy | ✅ |

### Delivery:
| Check | Expected |
|-------|----------|
| Drive to trophy → delivered | ✅ |
| Delivery sound plays | ✅ |
| Score increases by 1 | ✅ |
| New package appears | ✅ |

### Timer & Game Over:
| Check | Expected |
|-------|----------|
| Timer turns red below 15s | ✅ |
| Timer hits 0:00 → Game Over screen | ✅ |
| Final score displayed | ✅ |
| PLAY AGAIN restarts | ✅ |

---

## STEP 13 — Four New Road Chunk Prefabs (Req 10 — 20 pts)

You already have 9 chunks. Create 4 new ones by duplicating existing chunks.

### How to duplicate:
Project → `Assets/Prefabs/Chunks/` → select source prefab → **Ctrl+D** → rename → double-click → repaint tiles → fix ChunkData ports → Ctrl+S

### 4 new chunks:

| New Name | Copy From | North | South | East | West | Road Shape |
|----------|-----------|:-----:|:-----:|:----:|:----:|------------|
| `Chunk_StraightNS` | `Chunk_BottomLeft` | ✅ | ✅ | | | Vertical straight |
| `Chunk_StraightEW` | `Chunk_TJunctionBottom` | | | ✅ | ✅ | Horizontal straight |
| `Chunk_TJunctionLeft` | `Chunk_TJunctionTop` | ✅ | ✅ | | ✅ | T opening N+S+W |
| `Chunk_DeadEndS` | `Chunk_BottomLeft` | | ✅ | | | Dead end south |

### Add to WorldSpawner:
Hierarchy → `WorldSpawner` → Inspector → `Chunk Prefabs` → increase Size by 4 → drag each new prefab into new slots.

---

## STEP 14 — Obstacle System (Bonus — 10 pts)

### Copy sprites into project:
Copy from `kenney_racing-pack/PNG/Objects/` → `Assets/Sprites/`:

| Sprite file | Used for |
|-------------|----------|
| `cone_straight.png` | Cone prefab |
| `rock1.png` | Rock prefab |
| `oil.png` | OilSpill prefab |

For each: Project → click sprite → Inspector → **Alpha Is Transparency = ✅** → **Apply**

### Create 3 obstacle prefabs:

| Setting | Cone | Rock | OilSpill |
|---------|------|------|----------|
| Sprite | `cone_straight` | `rock1` | `oil` |
| Order in Layer | `6` | `6` | `4` |
| Circle Collider 2D | ✅ | ✅ | ✅ |
| Is Trigger | ❌ OFF | ❌ OFF | ✅ ON |
| Obstacle → Kind | Cone | Rock | Oil |
| Oil Slow Amount | — | — | `0.4` |
| Oil Slow Duration | — | — | `3` |
| Save to | `Assets/Prefabs/Cone.prefab` | `Assets/Prefabs/Rock.prefab` | `Assets/Prefabs/OilSpill.prefab` |

### Wire ObstacleSpawner on WorldSpawner:
Click `WorldSpawner` → Inspector → `Obstacle Spawner (Script)`:

| Slot | Value |
|------|-------|
| Cone Prefab | drag `Cone.prefab` from Project |
| Rock Prefab | drag `Rock.prefab` from Project |
| Oil Prefab | drag `OilSpill.prefab` from Project |
| Spawn Chance | `0.05` |

### Add ObstacleNotifier to Player:
Hierarchy → click `Player` → Add Component → `ObstacleNotifier`

| Slot | Value |
|------|-------|
| Warning Distance | `3` |

### Obstacle Play Test:

| Check | Expected |
|-------|----------|
| Cones/rocks visible on road | ✅ |
| Hit cone/rock → popup "Hit a Cone!" | ✅ |
| Drive over oil → car slows for 3s | ✅ |
| Warning prompt when obstacle nearby | ✅ |
| Console: `[Obstacle] Spawned Cone at...` | ✅ |

---

## Rubric Coverage

| Requirement | Points | Covered by |
|-------------|--------|-----------|
| Package and Delivery Loop | 25 | `DeliverySystem.cs` |
| Timer and Score UI | 25 | `GameManager.cs` + `UIBuilder.cs` |
| Road Generation + 4 New Chunks | 20 | `WorldSpawner.cs` + Steps 1 & 13 |
| Audio and Feedback | 15 | `GameManager.cs` |
| Gameplay Polish | 15 | All systems working together |
| Obstacle System (bonus) | 10 | `Obstacle.cs` + `ObstacleSpawner.cs` |
| **Total** | **110** | |

