// GameManager.cs — A4 FINAL
// Full end-game report: score, packages, deliveries, obstacles hit

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer (Req 4-5)")]
    public float totalTime = 90f;
    public TextMeshProUGUI timerText;

    [Header("Score (Req 6-7)")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI statusText;

    [Header("Package HUD")]
    public TextMeshProUGUI packageCountText;

    [Header("Prompt + Popup")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI popupText;
    public float           popupDuration = 1.5f;

    [Header("Audio (Req 8-9)")]
    public AudioSource musicSource;
    public AudioClip   backgroundMusic;
    public AudioSource sfxSource;
    public AudioClip   pickupSound;
    public AudioClip   deliverySound;

    [Header("Screens (Req 11)")]
    public GameObject      startScreen;
    public GameObject      gameOverScreen;
    public TextMeshProUGUI finalScoreText;      // main score line
    public TextMeshProUGUI finalReportText;     // detailed report

    // ── Runtime stats ─────────────────────────────────────────────────────────
    float timeRemaining;
    int   score;
    bool  playing;

    // Stats for end report
    int totalPackagesCollected = 0;
    int totalDeliveries        = 0;
    int conesHit               = 0;
    int rocksHit               = 0;
    int oilsHit                = 0;

    Coroutine popupRoutine;
    Coroutine pulseRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        timeRemaining = totalTime;
        score = 0;

        if (musicSource && backgroundMusic)
        {
            musicSource.clip   = backgroundMusic;
            musicSource.loop   = true;
            musicSource.volume = 0.4f;
            musicSource.Play();
        }

        if (popupText)        popupText.gameObject.SetActive(false);
        if (promptText)       promptText.gameObject.SetActive(false);
        if (packageCountText) packageCountText.text = "";

        ShowStartScreen();
        UpdateHUD();
    }

    void Update()
    {
        if (!playing) return;
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f) { timeRemaining = 0f; EndGame(); }
        UpdateHUD();
    }

    // ── Package HUD ───────────────────────────────────────────────────────────
    public void SetPackageHUD(int held, int remaining)
    {
        if (packageCountText == null) return;
        if (held == 0)
            packageCountText.text = "Find the package!";
        else
            packageCountText.text = $"Carrying {held} — deliver it!";
        packageCountText.color = held > 0
            ? new Color(1f, 0.85f, 0.3f)
            : new Color(0.7f, 0.9f, 1f);
    }

    // ── Called by DeliverySystem ──────────────────────────────────────────────
    public void OnPackagePickedUp(int nowHolding)
    {
        totalPackagesCollected++;
        sfxSource?.PlayOneShot(pickupSound);
        SetStatus($"Carrying {nowHolding} — drive to delivery!", Color.yellow);
        ShowPopup($"Package collected! ({totalPackagesCollected} total)", new Color(1f,0.85f,0.3f));
    }

    public void OnDeliveryComplete(int packagesDelivered)
    {
        totalDeliveries++;
        score += packagesDelivered;
        sfxSource?.PlayOneShot(deliverySound);
        SetStatus("Collect more packages!", Color.cyan);
        ShowPopup($"Delivered! Score: {score}  Deliveries: {totalDeliveries}",
                  new Color(0.4f,1f,0.4f));
    }

    // ── Called by Obstacle.cs ─────────────────────────────────────────────────
    public void OnObstacleHit(string kind)
    {
        switch (kind)
        {
            case "Cone": conesHit++; break;
            case "Rock": rocksHit++; break;
            case "Oil":  oilsHit++;  break;
        }
        Debug.Log($"[GM] Obstacle hit: {kind}  (cones={conesHit} rocks={rocksHit} oils={oilsHit})");
    }

    // ── Prompt ────────────────────────────────────────────────────────────────
    public void ShowPrompt(string msg)
    {
        if (promptText == null) return;
        promptText.text = msg;
        promptText.gameObject.SetActive(true);
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulsePrompt());
    }

    public void HidePrompt()
    {
        if (promptText == null) return;
        if (pulseRoutine != null) { StopCoroutine(pulseRoutine); pulseRoutine = null; }
        promptText.gameObject.SetActive(false);
    }

    System.Collections.IEnumerator PulsePrompt()
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.time * 2f, 1f);
            if (promptText)
                promptText.color = Color.Lerp(new Color(1f,1f,0f,0.5f), Color.white, t);
            yield return null;
        }
    }

    public bool IsPlaying => playing;

    // ── Screens ───────────────────────────────────────────────────────────────
    void ShowStartScreen()
    {
        playing = false;
        Time.timeScale = 0f;
        startScreen?.SetActive(true);
        gameOverScreen?.SetActive(false);
    }

    public void StartGame()
    {
        playing                = true;
        timeRemaining          = totalTime;
        score                  = 0;
        totalPackagesCollected = 0;
        totalDeliveries        = 0;
        conesHit               = 0;
        rocksHit               = 0;
        oilsHit                = 0;
        Time.timeScale         = 1f;
        startScreen?.SetActive(false);
        gameOverScreen?.SetActive(false);
        SetStatus("Collect packages!", Color.cyan);
        FindAnyObjectByType<DeliverySystem>()?.OnGameStarted();
    }

    void EndGame()
    {
        playing        = false;
        Time.timeScale = 0f;
        HidePrompt();
        // Stop any active popup so it doesn't overlap game over screen
        if (popupRoutine != null) { StopCoroutine(popupRoutine); popupRoutine = null; }
        if (popupText) popupText.gameObject.SetActive(false);
        if (packageCountText) packageCountText.text = "";
        gameOverScreen?.SetActive(true);

        // ── Main score line ───────────────────────────────────────────────────
        if (finalScoreText)
            finalScoreText.text = $"Final Score:  {score}";

        // ── Detailed report ───────────────────────────────────────────────────
        int totalObstaclesHit = conesHit + rocksHit + oilsHit;
        float accuracy = totalPackagesCollected > 0
            ? (float)totalDeliveries / totalPackagesCollected * 100f
            : 0f;

        string report =
            $"━━━━━━━━━━━━━━━━━━━━━━━━\n" +
            $"  GAME SUMMARY\n" +
            $"━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
            $"  Packages Collected   {totalPackagesCollected}\n" +
            $"  Deliveries Made      {totalDeliveries}\n" +
            $"  Delivery Rate        {accuracy:F0}%\n\n" +
            $"  ── Obstacles Hit ──\n" +
            $"  Cones                {conesHit}\n" +
            $"  Rocks                {rocksHit}\n" +
            $"  Oil Spills           {oilsHit}\n" +
            $"  Total Obstacles      {totalObstaclesHit}\n\n" +
            $"  ── Time ──\n" +
            $"  Time Limit           {(int)totalTime}s\n" +
            $"  Time Used            {(int)(totalTime - timeRemaining)}s\n";

        if (finalReportText) finalReportText.text = report;

        Debug.Log($"[GM] GAME OVER — Score={score}  Packages={totalPackagesCollected}  " +
                  $"Deliveries={totalDeliveries}  Obstacles={totalObstaclesHit}");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ── Popup ─────────────────────────────────────────────────────────────────
    public void ShowPopup(string msg, Color color)
    {
        if (popupText == null) return;
        if (popupRoutine != null) StopCoroutine(popupRoutine);
        popupRoutine = StartCoroutine(PopupRoutine(msg, color));
    }

    System.Collections.IEnumerator PopupRoutine(string msg, Color color)
    {
        popupText.text  = msg;
        popupText.color = color;
        popupText.gameObject.SetActive(true);
        yield return new WaitForSeconds(popupDuration - 0.4f);
        float t = 0f;
        Color s = popupText.color;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            popupText.color = new Color(s.r, s.g, s.b, 1f - t/0.4f);
            yield return null;
        }
        popupText.gameObject.SetActive(false);
        popupRoutine = null;
    }

    // ── HUD ──────────────────────────────────────────────────────────────────
    void UpdateHUD()
    {
        if (timerText)
        {
            int m = Mathf.FloorToInt(timeRemaining/60f);
            int s = Mathf.FloorToInt(timeRemaining%60f);
            timerText.text  = $"Time: {m:00}:{s:00}";
            timerText.color = timeRemaining < 15f ? Color.red : new Color(1f,0.85f,0f);
        }
        if (scoreText) scoreText.text = "Score: " + score;
    }

    void SetStatus(string msg, Color col)
    {
        if (statusText) { statusText.text = msg; statusText.color = col; }
    }
}