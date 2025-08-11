using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton pattern để dễ truy cập từ bất kỳ đâu
    public static GameManager Instance;

    [Header("Game Settings")]
    public int startingLives = 3;
    public int maxLives = 3;

    [Header("Spawn Control Settings")]
    [SerializeField] private float initialObstacleSpawnInterval = 10f; // Bắt đầu từ 10s
    [SerializeField] private float minObstacleSpawnInterval = 5f; // Tối thiểu 5s
    [SerializeField] private float obstacleSpawnDecreaseRate = 0.5f; // Giảm 0.5s mỗi lần
    [SerializeField] private float obstacleSpawnDecreaseInterval = 30f; // Giảm mỗi 30s

    [SerializeField] private float minCoinSpawnInterval = 5f; // Coin spawn 5-10s
    [SerializeField] private float maxCoinSpawnInterval = 10f;

    [Header("UI References - GamePlay")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI timeText;       // Hiển thị thời gian chơi

    [Header("UI References - Panels")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject victoryPanel;
    public GameObject levelStartPanel;     // Panel đếm ngược bắt đầu level
    public GameObject notificationPanel;   // Panel thông báo
    public GameObject powerupPanel;        // Panel hiển thị powerup đang active

    [Header("UI References - Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button victoryRestartButton;
    public Button victoryMainMenuButton;
    public Button nextLevelButton;         // Nút chuyển màn tiếp theo
    public Button skipCountdownButton;     // Nút bỏ qua đếm ngược

    [Header("UI References - Game Over")]
    public TextMeshProUGUI finalScoreText; // Điểm cuối cùng hiển thị trong Game Over
    public TextMeshProUGUI bestScoreText;  // Điểm cao nhất hiển thị trong Game Over
    public GameObject newRecordIndicator;  // Hiển thị khi có kỷ lục mới

    [Header("UI References - Level Start")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI levelDescriptionText;
    public TextMeshProUGUI countdownText;

    [Header("UI References - Victory")]
    public TextMeshProUGUI victoryScoreText;
    public TextMeshProUGUI victoryTimeText;
    public GameObject[] starIcons;         // Icons sao đánh giá

    // Game State
    private int currentCoins = 0;
    private int totalCoins = 0; // Tổng coin để mở khóa nhân vật
    private int currentLives;
    private bool isGamePaused = false;
    private bool isGameOver = false;
    private bool isVictory = false;
    private bool isLevelStarting = true;
    private float maxSpeedThisGame = 1f;
    private bool isInitialized = false; // Để tránh khởi tạo nhiều lần

    // Spawn timing
    private float gameStartTime;
    private float currentObstacleSpawnInterval;
    private float nextObstacleSpawnIntervalDecrease;

    // Events để các script khác có thể lắng nghe
    public System.Action<int> OnCoinsChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action OnGameOver;
    public System.Action OnVictory;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Chỉ khởi tạo nếu KHÔNG phải MainMenu scene
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "MainMenu" && !isInitialized)
        {
            InitializeGame();
            SetupUI();
            isInitialized = true;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        if (scene.name == "MainMenu")
        {
            // Đã ở main menu - không cần làm gì thêm
            Debug.Log("In MainMenu - GameManager reset by MainMenuManager");
        }
        else
        {
            // Gameplay scene
            Debug.Log("In Gameplay scene - initializing...");

            // Refresh UI references
            RefreshUIReferences();

            // Initialize game nếu chưa
            if (!isInitialized)
            {
                InitializeGame();
                SetupUI();
                isInitialized = true;
            }
        }
    }

    void Update()
    {
        // Pause game bằng phím ESC (chỉ khi đang chơi)
        if (Input.GetKeyDown(KeyCode.Escape) && CanPause())
        {
            if (isGamePaused)
                ResumeGame();
            else
                PauseGame();
        }

        // Update spawn timing
        if (CanPlay())
        {
            UpdateSpawnTiming();
            TrackMaxSpeed();
        }
    }

    void InitializeGame()
    {
        Debug.Log("Initializing game...");

        // Dừng tất cả coroutine đang chạy
        StopAllCoroutines();

        ResetGameStates();
        ResetGameData();

        UpdateUI();
        HideAllGameplayPanels();

        // Bắt đầu level start countdown
        StartLevelStart();
    }

    void ResetGameStates()
    {
        isGameOver = false;
        isGamePaused = false;
        isVictory = false;
        isLevelStarting = true;
        Time.timeScale = 1f;
    }

    void ResetGameData()
    {
        currentLives = startingLives;
        currentCoins = 0;
        maxSpeedThisGame = 1f;

        // Initialize spawn timing
        gameStartTime = Time.time;
        currentObstacleSpawnInterval = initialObstacleSpawnInterval;
        nextObstacleSpawnIntervalDecrease = gameStartTime + obstacleSpawnDecreaseInterval;

        // Load tổng coin từ PlayerPrefs
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
    }

    void HideAllGameplayPanels()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
        if (powerupPanel != null)
            powerupPanel.SetActive(false);
        if (levelStartPanel != null)
            levelStartPanel.SetActive(false);
    }

    void StartLevelStart()
    {
        if (levelStartPanel != null)
            levelStartPanel.SetActive(true);
        StartCoroutine(LevelStartCountdown());
    }

    void ResetForNewGame()
    {
        Debug.Log("Resetting for new game...");

        StopAllCoroutines();
        ResetGameStates();
        ResetGameData();

        // Refresh UI references và setup lại
        RefreshUIReferences();
        SetupUI();

        UpdateUI();
        HideAllGameplayPanels();
        StartLevelStart();
    }

    bool CanPause()
    {
        return !isGameOver && !isVictory && !isLevelStarting;
    }

    void UpdateSpawnTiming()
    {
        // Giảm obstacle spawn interval theo thời gian
        if (Time.time >= nextObstacleSpawnIntervalDecrease)
        {
            if (currentObstacleSpawnInterval > minObstacleSpawnInterval)
            {
                currentObstacleSpawnInterval = Mathf.Max(
                    currentObstacleSpawnInterval - obstacleSpawnDecreaseRate,
                    minObstacleSpawnInterval
                );

                nextObstacleSpawnIntervalDecrease = Time.time + obstacleSpawnDecreaseInterval;

                Debug.Log($"[GAMEMANAGER] Obstacle spawn interval decreased to {currentObstacleSpawnInterval}s");
            }
        }
    }

    void TrackMaxSpeed()
    {
        // Theo dõi tốc độ tối đa đạt được (lấy từ ParallaxController)
        ParallaxController parallax = FindFirstObjectByType<ParallaxController>();
        if (parallax != null)
        {
            float currentSpeed = parallax.GetCurrentSpeedMultiplier();
            if (currentSpeed > maxSpeedThisGame)
                maxSpeedThisGame = currentSpeed;
        }
    }

    void SetupUI()
    {
        Debug.Log("Setting up UI...");

        // Refresh references trước khi setup
        RefreshUIReferences();

        // Clear old listeners trước khi add new
        ClearButtonListeners();

        // Game Over buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        // Pause buttons
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        // Victory buttons
        if (victoryRestartButton != null)
            victoryRestartButton.onClick.AddListener(RestartGame);
        if (victoryMainMenuButton != null)
            victoryMainMenuButton.onClick.AddListener(GoToMainMenu);
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(NextLevel);

        // Level Start buttons
        if (skipCountdownButton != null)
            skipCountdownButton.onClick.AddListener(SkipCountdown);

        Debug.Log("UI setup completed!");
    }

    void ClearButtonListeners()
    {
        // Clear old listeners để tránh duplicate
        if (restartButton != null)
            restartButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveAllListeners();
        if (pauseButton != null)
            pauseButton.onClick.RemoveAllListeners();
        if (resumeButton != null)
            resumeButton.onClick.RemoveAllListeners();
        if (victoryRestartButton != null)
            victoryRestartButton.onClick.RemoveAllListeners();
        if (victoryMainMenuButton != null)
            victoryMainMenuButton.onClick.RemoveAllListeners();
        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveAllListeners();
        if (skipCountdownButton != null)
            skipCountdownButton.onClick.RemoveAllListeners();
    }

    #region Level Start Methods
    private IEnumerator LevelStartCountdown()
    {
        Debug.Log("Starting level countdown...");

        Time.timeScale = 0f;
        isLevelStarting = true;

        // Set level info
        if (levelNameText != null)
            levelNameText.text = "LEVEL 1";
        if (levelDescriptionText != null)
            levelDescriptionText.text = "Collect coins and avoid obstacles!";

        // Wait a bit before starting countdown
        yield return new WaitForSecondsRealtime(0.5f);

        // Start countdown
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        // GO!
        if (countdownText != null)
            countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(0.5f);

        // Start the game
        Time.timeScale = 1f;
        isLevelStarting = false;
        if (levelStartPanel != null)
            levelStartPanel.SetActive(false);

        Debug.Log("Level started!");
    }

    public void SkipCountdown()
    {
        if (!isLevelStarting) return;

        Debug.Log("Skipping countdown...");

        StopAllCoroutines();
        Time.timeScale = 1f;
        isLevelStarting = false;

        if (levelStartPanel != null)
            levelStartPanel.SetActive(false);
    }

    public void NextLevel()
    {
        // Logic để load level tiếp theo
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion

    #region Notification & Powerup
    public void ShowNotification(string message, float duration = 2f)
    {
        // Gọi đến GameplayUI
        GameplayUI ui = FindFirstObjectByType<GameplayUI>();
        if (ui != null)
            ui.ShowNotification(message);
    }

    public void ShowPowerup(Sprite icon, float duration)
    {
        // Gọi đến GameplayUI
        GameplayUI ui = FindFirstObjectByType<GameplayUI>();
        if (ui != null)
            ui.ShowPowerup(icon, duration);
    }
    #endregion

    #region Spawn Timing Getters
    public float GetCurrentObstacleSpawnInterval()
    {
        return currentObstacleSpawnInterval;
    }

    public float GetRandomCoinSpawnInterval()
    {
        return Random.Range(minCoinSpawnInterval, maxCoinSpawnInterval);
    }

    public float GetGameTime()
    {
        return Time.time - gameStartTime;
    }

    private string GetFormattedGameTime()
    {
        float gameTime = GetGameTime();
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    #endregion

    #region Coin Management
    public void AddCoin(int amount = 1)
    {
        if (isGameOver || isVictory) return;

        currentCoins += amount;
        totalCoins += amount;

        OnCoinsChanged?.Invoke(currentCoins);
        UpdateCoinUI();

        // Thông báo nhỏ khi nhặt coin
        if (amount > 1)
            ShowNotification($"+{amount} coins!", 1f);

        // Play coin sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCoinSound();

        // Lưu tổng coin
        SaveTotalCoins();
    }

    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public int GetTotalCoins()
    {
        return totalCoins;
    }

    public int GetHighCoins()
    {
        return PlayerPrefs.GetInt("HighCoins", 0);
    }
    #endregion

    #region Lives Management
    public void LoseLife()
    {
        if (isGameOver) return;

        currentLives--;
        OnLivesChanged?.Invoke(currentLives);
        UpdateLivesUI();

        // Play damage sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDamageSound();

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Hiển thị thông báo khi mất mạng
            ShowNotification($"Lives: {currentLives}", 1.5f);
        }
    }

    public void AddLife()
    {
        if (currentLives < maxLives)
        {
            currentLives++;
            OnLivesChanged?.Invoke(currentLives);
            UpdateLivesUI();

            // Hiển thị thông báo khi được thêm mạng
            ShowNotification("Extra Life!", 1.5f);
        }
    }

    public int GetLives()
    {
        return currentLives;
    }
    #endregion

    #region Game State Management
    public void PauseGame()
    {
        if (!CanPause()) return;

        Debug.Log("Game paused");

        isGamePaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        OnGamePaused?.Invoke();

        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    public void ResumeGame()
    {
        if (!isGamePaused) return;

        Debug.Log("Game resumed");

        isGamePaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        OnGameResumed?.Invoke();

        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    public void Victory()
    {
        if (isGameOver || isVictory) return;

        Debug.Log("Victory!");

        isVictory = true;
        Time.timeScale = 0f;

        // Update Victory UI
        if (victoryScoreText != null)
            victoryScoreText.text = currentCoins.ToString();

        if (victoryTimeText != null)
            victoryTimeText.text = GetFormattedGameTime();

        // Update stars
        UpdateStarRating();

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        // Play victory sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayVictorySound();

        OnVictory?.Invoke();
        SaveHighCoins();
    }

    public void GameOver()
    {
        if (isGameOver || isVictory) return;

        Debug.Log("Game Over!");

        isGameOver = true;
        Time.timeScale = 0f;

        // Update Game Over UI
        if (finalScoreText != null)
            finalScoreText.text = currentCoins.ToString();

        if (bestScoreText != null)
            bestScoreText.text = GetHighCoins().ToString();

        if (newRecordIndicator != null)
            newRecordIndicator.SetActive(currentCoins > GetHighCoins());

        // Save score
        SimpleLeaderboard.SaveScore(currentCoins);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Play game over sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOverSound();

        OnGameOver?.Invoke();
        SaveHighCoins();
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");

        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Force hide panels trước khi restart
        ForceHideAllGameplayPanels();
        
        // Reset timescale
        Time.timeScale = 1f;
        
        // Reset states để chuẩn bị cho game mới
        isGameOver = false;
        isGamePaused = false;
        isVictory = false;
        isLevelStarting = true;
        isInitialized = false;
        
        // Clear UI references
        ClearUIReferences();

        // Load lại scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Debug.Log("Going to main menu...");

        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Force hide panels
        ForceHideAllGameplayPanels();
        
        // Reset timescale
        Time.timeScale = 1f;
        
        // Reset states để chuẩn bị về main menu
        isGameOver = false;
        isGamePaused = false;
        isVictory = false;
        isLevelStarting = false;
        isInitialized = false;
        
        // Clear UI references
        ClearUIReferences();

        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }

    private void UpdateStarRating()
    {
        if (starIcons == null || starIcons.Length == 0) return;

        // Tính số sao dựa trên coins và thời gian
        int stars = 1; // Mặc định 1 sao

        // Ví dụ logic tính sao:
        if (currentCoins >= 50) stars = 2;
        if (currentCoins >= 100) stars = 3;

        // Hiển thị số sao tương ứng
        for (int i = 0; i < starIcons.Length; i++)
        {
            if (starIcons[i] != null)
                starIcons[i].SetActive(i < stars);
        }
    }
    #endregion

    #region UI Updates
    void UpdateUI()
    {
        UpdateCoinUI();
        UpdateLivesUI();
        UpdateTimeUI();
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = currentCoins.ToString();
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = currentLives.ToString();
    }

    void UpdateTimeUI()
    {
        if (timeText != null)
            timeText.text = GetFormattedGameTime();
    }
    #endregion

    #region Save/Load System
    void SaveHighCoins()
    {
        int highCoins = PlayerPrefs.GetInt("HighCoins", 0);
        if (currentCoins > highCoins)
        {
            PlayerPrefs.SetInt("HighCoins", currentCoins);
            PlayerPrefs.Save();
        }
    }

    void SaveTotalCoins()
    {
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();
    }

    public void SpendTotalCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            SaveTotalCoins();
        }
    }
    #endregion

    #region Getters cho các script khác
    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public bool IsVictory()
    {
        return isVictory;
    }

    public bool CanPlay()
    {
        return !isGamePaused && !isGameOver && !isVictory && !isLevelStarting;
    }

    public float GetMaxSpeed()
    {
        return maxSpeedThisGame;
    }
    #endregion

    // Thêm method này vào GameManager
    void RefreshUIReferences()
    {
        Debug.Log("Refreshing UI references...");

        // Tìm lại tất cả UI references sau khi scene reload
        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");

        if (pausePanel == null)
            pausePanel = GameObject.Find("PausePanel");

        if (victoryPanel == null)
            victoryPanel = GameObject.Find("VictoryPanel");

        if (levelStartPanel == null)
            levelStartPanel = GameObject.Find("LevelStartPanel");

        if (notificationPanel == null)
            notificationPanel = GameObject.Find("NotificationPanel");

        if (powerupPanel == null)
            powerupPanel = GameObject.Find("PowerupPanel");

        // Tìm lại UI text references
        if (coinText == null)
        {
            GameObject coinTextObj = GameObject.Find("CoinText");
            if (coinTextObj != null)
                coinText = coinTextObj.GetComponent<TextMeshProUGUI>();
        }

        if (livesText == null)
        {
            GameObject livesTextObj = GameObject.Find("LivesText");
            if (livesTextObj != null)
                livesText = livesTextObj.GetComponent<TextMeshProUGUI>();
        }

        if (timeText == null)
        {
            GameObject timeTextObj = GameObject.Find("TimeText");
            if (timeTextObj != null)
                timeText = timeTextObj.GetComponent<TextMeshProUGUI>();
        }

        // Tìm lại button references
        RefreshButtonReferences();

        // Tìm lại Game Over UI references
        RefreshGameOverUIReferences();

        // Tìm lại Level Start UI references
        RefreshLevelStartUIReferences();

        // Tìm lại Victory UI references
        RefreshVictoryUIReferences();

        Debug.Log("UI references refreshed!");
    }

    void RefreshButtonReferences()
    {
        // Tìm tất cả buttons trong scene
        Button[] allButtons = FindObjectsOfType<Button>();

        foreach (Button btn in allButtons)
        {
            string btnName = btn.name.ToLower();

            if (restartButton == null && (btnName.Contains("restart") || btnName.Contains("tryagain")))
                restartButton = btn;

            if (mainMenuButton == null && (btnName.Contains("mainmenu") || btnName.Contains("menu")))
                mainMenuButton = btn;

            if (pauseButton == null && btnName.Contains("pause"))
                pauseButton = btn;

            if (resumeButton == null && btnName.Contains("resume"))
                resumeButton = btn;

            if (victoryRestartButton == null && btnName.Contains("victoryrestart"))
                victoryRestartButton = btn;

            if (victoryMainMenuButton == null && btnName.Contains("victorymenu"))
                victoryMainMenuButton = btn;

            if (nextLevelButton == null && btnName.Contains("nextlevel"))
                nextLevelButton = btn;

            if (skipCountdownButton == null && btnName.Contains("skip"))
                skipCountdownButton = btn;
        }
    }

    void RefreshGameOverUIReferences()
    {
        if (finalScoreText == null)
        {
            GameObject obj = GameObject.Find("FinalScoreText");
            if (obj != null) finalScoreText = obj.GetComponent<TextMeshProUGUI>();
        }

        if (bestScoreText == null)
        {
            GameObject obj = GameObject.Find("BestScoreText");
            if (obj != null) bestScoreText = obj.GetComponent<TextMeshProUGUI>();
        }

        if (newRecordIndicator == null)
            newRecordIndicator = GameObject.Find("NewRecordIndicator");
    }

    void RefreshLevelStartUIReferences()
    {
        if (levelNameText == null)
        {
            GameObject obj = GameObject.Find("LevelNameText");
            if (obj != null) levelNameText = obj.GetComponent<TextMeshProUGUI>();
        }

        if (levelDescriptionText == null)
        {
            GameObject obj = GameObject.Find("LevelDescriptionText");
            if (obj != null) levelDescriptionText = obj.GetComponent<TextMeshProUGUI>();
        }

        if (countdownText == null)
        {
            GameObject obj = GameObject.Find("CountdownText");
            if (obj != null) countdownText = obj.GetComponent<TextMeshProUGUI>();
        }
    }

    void RefreshVictoryUIReferences()
    {
        if (victoryScoreText == null)
        {
            GameObject obj = GameObject.Find("VictoryScoreText");
            if (obj != null) victoryScoreText = obj.GetComponent<TextMeshProUGUI>();
        }

        if (victoryTimeText == null)
        {
            GameObject obj = GameObject.Find("VictoryTimeText");
            if (obj != null) victoryTimeText = obj.GetComponent<TextMeshProUGUI>();
        }

        // Tìm star icons
        if (starIcons == null || starIcons.Length == 0)
        {
            GameObject[] stars = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                stars[i] = GameObject.Find($"Star{i + 1}");
            }
            starIcons = stars;
        }
    }

    // Thêm method debug để check references
    [ContextMenu("Debug UI References")]
    void DebugUIReferences()
    {
        Debug.Log("=== UI REFERENCES DEBUG ===");
        Debug.Log($"gameOverPanel: {(gameOverPanel != null ? "OK" : "NULL")}");
        Debug.Log($"pausePanel: {(pausePanel != null ? "OK" : "NULL")}");
        Debug.Log($"victoryPanel: {(victoryPanel != null ? "OK" : "NULL")}");
        Debug.Log($"levelStartPanel: {(levelStartPanel != null ? "OK" : "NULL")}");
        Debug.Log($"restartButton: {(restartButton != null ? "OK" : "NULL")}");
        Debug.Log($"mainMenuButton: {(mainMenuButton != null ? "OK" : "NULL")}");
        Debug.Log($"coinText: {(coinText != null ? "OK" : "NULL")}");
        Debug.Log($"livesText: {(livesText != null ? "OK" : "NULL")}");
    }

    // Method để reset hoàn toàn khi về main menu
    public void ResetToMainMenu()
    {
        Debug.Log("Resetting GameManager to MainMenu state");

        // Stop tất cả coroutines
        StopAllCoroutines();

        // Reset tất cả states
        isGameOver = false;
        isGamePaused = false;
        isVictory = false;
        isLevelStarting = false;
        isInitialized = false;

        // Reset timescale
        Time.timeScale = 1f;

        // Clear UI references (vì sẽ load scene mới)
        ClearUIReferences();
    }

    // Method để chuẩn bị cho game mới
    public void PrepareForNewGame()
    {
        Debug.Log("Preparing for new game");

        // Reset states
        isGameOver = false;
        isGamePaused = false;
        isVictory = false;
        isLevelStarting = true;
        isInitialized = false;

        // Reset timescale
        Time.timeScale = 1f;

        // Clear UI references
        ClearUIReferences();
    }

    // Method để clear UI references
    void ClearUIReferences()
    {
        Debug.Log("Clearing UI references");

        // Clear panel references
        gameOverPanel = null;
        pausePanel = null;
        victoryPanel = null;
        levelStartPanel = null;
        notificationPanel = null;
        powerupPanel = null;

        // Clear text references
        coinText = null;
        livesText = null;
        timeText = null;
        finalScoreText = null;
        bestScoreText = null;
        levelNameText = null;
        levelDescriptionText = null;
        countdownText = null;
        victoryScoreText = null;
        victoryTimeText = null;

        // Clear button references
        restartButton = null;
        mainMenuButton = null;
        pauseButton = null;
        resumeButton = null;
        victoryRestartButton = null;
        victoryMainMenuButton = null;
        nextLevelButton = null;
        skipCountdownButton = null;

        // Clear other references
        newRecordIndicator = null;
        starIcons = null;
    }

    // Thêm method này vào GameManager để force hide tất cả panels
    public void ForceHideAllGameplayPanels()
    {
        Debug.Log("Force hiding all gameplay panels");
        
        // Tìm và ẩn tất cả panels bằng tên
        string[] panelNames = {
            "GameOverPanel", "PausePanel", "VictoryPanel", 
            "LevelStartPanel", "NotificationPanel", "PowerupPanel"
        };
        
        foreach (string panelName in panelNames)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel != null && panel.activeInHierarchy)
            {
                Debug.Log($"Force hiding: {panelName}");
                panel.SetActive(false);
            }
        }
        
        // Cách khác: Tìm tất cả objects có "Panel" trong tên
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("panel") && 
                !obj.name.ToLower().Contains("main") && 
                obj.activeInHierarchy)
            {
                Debug.Log($"Force hiding panel: {obj.name}");
                obj.SetActive(false);
            }
        }
    }
}