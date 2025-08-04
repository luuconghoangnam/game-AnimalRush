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
    
    [Header("UI References - Panels")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject victoryPanel;
    
    [Header("UI References - Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button victoryRestartButton;
    public Button victoryMainMenuButton;
    
    // Game State
    private int currentCoins = 0;
    private int totalCoins = 0; // Tổng coin để mở khóa nhân vật
    private int currentLives;
    private bool isGamePaused = false;
    private bool isGameOver = false;
    private bool isVictory = false;
    
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
        InitializeGame();
        SetupUI();
    }

    void Update()
    {
        // Pause game bằng phím ESC
        if (Input.GetKeyDown(KeyCode.Escape))
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
        }
    }

    void InitializeGame()
    {
        currentLives = startingLives;
        currentCoins = 0;
        isGameOver = false;
        isGamePaused = false;
        isVictory = false;
        
        // Initialize spawn timing
        gameStartTime = Time.time;
        currentObstacleSpawnInterval = initialObstacleSpawnInterval;
        nextObstacleSpawnIntervalDecrease = gameStartTime + obstacleSpawnDecreaseInterval;
        
        // Load tổng coin từ PlayerPrefs
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        
        UpdateUI();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
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

    void SetupUI()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (victoryRestartButton != null)
            victoryRestartButton.onClick.AddListener(RestartGame);
        if (victoryMainMenuButton != null)
            victoryMainMenuButton.onClick.AddListener(GoToMainMenu);
    }

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
    #endregion

    #region Coin Management
    public void AddCoin(int amount = 1)
    {
        if (isGameOver || isVictory) return;
        
        currentCoins += amount;
        totalCoins += amount;
        
        OnCoinsChanged?.Invoke(currentCoins);
        UpdateCoinUI();
        
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
        
        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    public void AddLife()
    {
        if (currentLives < maxLives)
        {
            currentLives++;
            OnLivesChanged?.Invoke(currentLives);
            UpdateLivesUI();
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
        if (isGameOver) return;
        
        isGamePaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
            
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        OnGameResumed?.Invoke();
    }

    public void Victory()
    {
        if (isGameOver || isVictory) return;
        
        isVictory = true;
        Time.timeScale = 0f;
        
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
            
        OnVictory?.Invoke();
        
        // Lưu high coins nếu cần
        SaveHighCoins();
    }

    public void GameOver()
    {
        if (isGameOver || isVictory) return;
        
        isGameOver = true;
        Time.timeScale = 0f;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
            
        OnGameOver?.Invoke();
        
        // Lưu high coins nếu cần
        SaveHighCoins();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // Thay "MainMenu" bằng tên scene menu chính của bạn
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region UI Updates
    void UpdateUI()
    {
        UpdateCoinUI();
        UpdateLivesUI();
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
        return !isGamePaused && !isGameOver && !isVictory;
    }
    #endregion
} 