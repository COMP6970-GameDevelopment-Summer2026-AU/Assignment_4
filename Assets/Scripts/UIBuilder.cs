// UIBuilder.cs — A4
// Builds the entire UI in one click.
// USAGE: Tools → Courier Rush → Build UI

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class UIBuilder
{
    [MenuItem("Tools/Courier Rush/Build UI")]
    public static void BuildUI()
    {
        // ── Remove stale canvas ───────────────────────────────────────────────
        var old = GameObject.Find("CourierUI");
        if (old != null) { Object.DestroyImmediate(old); Debug.Log("[UI] Removed old canvas."); }

        // ── Canvas ────────────────────────────────────────────────────────────
        var canvasGO = new GameObject("CourierUI");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem — must use New Input System module
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ── HUD texts ─────────────────────────────────────────────────────────
        var timerGO = MakeTMP(canvasGO, "TimerText", "Time: 01:30",
            32, new Color(1f, 0.85f, 0f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -50f), new Vector2(320f, 55f));

        var scoreGO = MakeTMP(canvasGO, "ScoreText", "Deliveries: 0",
            28, new Color(0.4f, 1f, 0.4f),
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(170f, -50f), new Vector2(280f, 55f));

        // Package count — top right, orange
        var pkgCountGO = MakeTMP(canvasGO, "PackageCountText", "Packages: 0 / 10",
            28, new Color(1f, 0.7f, 0.2f),
            new Vector2(1f,1f), new Vector2(1f,1f),
            new Vector2(-170f,-50f), new Vector2(300f,55f));

        var statusGO = MakeTMP(canvasGO, "StatusText", "PICK UP PACKAGES!",
            36, new Color(0f, 0.85f, 1f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 70f), new Vector2(640f, 60f),
            FontStyles.Bold);

        // ── Prompt — pulsing SPACE hint above status bar ─────────────────────────
        var promptGO = MakeTMP(canvasGO, "PromptText", "Press  SPACE",
            38, Color.white,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 140f), new Vector2(700f, 60f),
            FontStyles.Bold);
        promptGO.SetActive(false);

        // ── Popup message — screen center, starts hidden ───────────────────────
        var popupGO = MakeTMP(canvasGO, "PopupText", "",
            42, Color.yellow,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 80f), new Vector2(900f, 120f),
            FontStyles.Bold);
        popupGO.SetActive(false);   // hidden until triggered

        // ── START SCREEN ──────────────────────────────────────────────────────
        var startPanel = MakePanel(canvasGO, "StartScreen", new Color(0, 0, 0, 0.85f));

        MakeTMP(startPanel, "TitleText", "ENDLESS COURIER 2D",
            68, new Color(1f, 0.85f, 0f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 120f), new Vector2(900f, 100f), FontStyles.Bold);

        MakeTMP(startPanel, "SubText",
            "Pick up packages  •  Deliver before time runs out  •  Score big!",
            20, new Color(0.75f, 0.75f, 0.75f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 40f), new Vector2(800f, 40f));

        MakeTMP(startPanel, "ControlsText", "WASD / Arrow Keys to drive",
            18, new Color(0.6f, 0.8f, 0.6f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -10f), new Vector2(600f, 35f));

        MakeButton(startPanel, "StartButton", "START GAME",
            new Vector2(0f, -110f), new Color(0.1f, 0.55f, 0.1f));

        // ── GAME OVER SCREEN ──────────────────────────────────────────────────
        // Solid black panel — fully covers the scene
        var overPanel = MakePanel(canvasGO, "GameOverScreen", new Color(0f, 0f, 0f, 1f));
        overPanel.SetActive(false);

        // All text combined into ONE TextMeshPro object — no overlap possible
        // GameManager.EndGame() writes the full report into FinalReportText
        // FinalScoreText is a hidden dummy kept for compatibility
        var finalGO = MakeTMP(overPanel, "FinalScoreText", "",
            1, Color.clear,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 9999f), new Vector2(1f, 1f)); // off-screen, invisible

        // Single report text — centered, large, readable
        var reportGO = MakeTMP(overPanel, "FinalReportText", "",
            22, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 40f), new Vector2(600f, 500f));

        // PLAY AGAIN — bottom center
        MakeButton(overPanel, "PlayAgainButton", "PLAY AGAIN",
            new Vector2(0f, -220f), new Color(0.1f, 0.2f, 0.65f));

        // ── Auto-wire GameManager ─────────────────────────────────────────────
        var gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.timerText      = timerGO.GetComponent<TextMeshProUGUI>();
            gm.scoreText      = scoreGO.GetComponent<TextMeshProUGUI>();
            gm.statusText     = statusGO.GetComponent<TextMeshProUGUI>();
            gm.packageCountText = pkgCountGO.GetComponent<TextMeshProUGUI>();
            gm.promptText     = promptGO.GetComponent<TextMeshProUGUI>();
            gm.popupText      = popupGO.GetComponent<TextMeshProUGUI>();
            gm.startScreen    = startPanel;
            gm.gameOverScreen = overPanel;
            gm.finalScoreText  = finalGO.GetComponent<TextMeshProUGUI>();
            gm.finalReportText = reportGO.GetComponent<TextMeshProUGUI>();
            EditorUtility.SetDirty(gm);
            Debug.Log("[UI] GameManager slots auto-wired ✅");
        }
        else
        {
            Debug.LogWarning("[UI] No GameManager found — create _GameManager first, then re-run.");
        }

        WireButton(startPanel, "StartButton",    gm, "StartGame");
        WireButton(overPanel,  "PlayAgainButton", gm, "RestartGame");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("UI Built ✅",
            "Canvas 'CourierUI' created with:\n\n" +
            "  • TimerText\n  • ScoreText\n  • StatusText\n" +
            "  • PopupText  ← NEW (package collected message)\n" +
            "  • StartScreen + START button\n" +
            "  • GameOverScreen + PLAY AGAIN button\n\n" +
            (gm != null ? "All slots and buttons auto-wired ✅" :
                "⚠️ Create _GameManager first then re-run."), "OK");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static GameObject MakeTMP(GameObject parent, string name, string text,
        float size, Color color,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pos, Vector2 sizeDelta,
        FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt               = go.AddComponent<RectTransform>();
        rt.anchorMin         = anchorMin;
        rt.anchorMax         = anchorMax;
        rt.pivot             = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition  = pos;
        rt.sizeDelta         = sizeDelta;
        var tmp              = go.AddComponent<TextMeshProUGUI>();
        tmp.text             = text;
        tmp.fontSize         = size;
        tmp.color            = color;
        tmp.fontStyle        = style;
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
        return go;
    }

    static GameObject MakePanel(GameObject parent, string name, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = bg;
        return go;
    }

    static GameObject MakeButton(GameObject parent, string name, string label,
        Vector2 pos, Color bg, bool anchorBottom = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt              = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorBottom ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
        rt.anchorMax        = anchorBottom ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(240f, 65f);
        go.AddComponent<Image>().color = bg;
        go.AddComponent<Button>();
        var lbl = new GameObject("Label");
        lbl.transform.SetParent(go.transform, false);
        var lrt      = lbl.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;
        var t        = lbl.AddComponent<TextMeshProUGUI>();
        t.text       = label;
        t.fontSize   = 30;
        t.color      = Color.white;
        t.fontStyle  = FontStyles.Bold;
        t.alignment  = TextAlignmentOptions.Center;
        return go;
    }

    static void WireButton(GameObject panel, string buttonName,
        GameManager gm, string methodName)
    {
        if (gm == null) return;
        var btn = panel.transform.Find(buttonName)?.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        var method = typeof(GameManager).GetMethod(methodName);
        if (method == null) return;
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            btn.onClick,
            System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), gm, method)
            as UnityEngine.Events.UnityAction);
        EditorUtility.SetDirty(btn);
    }
}
#endif