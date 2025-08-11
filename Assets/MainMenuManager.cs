using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject characterPanel;
    public GameObject leaderboardPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Main Menu Buttons")]
    public Button playButton;      // Nút Play để vào màn chơi
    public Button characterButton;
    public Button leaderboardButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button exitButton;      // Nút Exit để thoát game
    public Button[] backButtons;

    [Header("Display")]
    public TextMeshProUGUI coinsText;

    [Header("Scene Settings")]
    public string gameSceneName = "MainLevel";  // Tên scene màn chơi

    void Start()
    {
        SetupButtons();
        ShowMainMenu();
        UpdateDisplays();

        // RESET GameManager khi vào main menu
        ResetGameManagerState();

        // Play menu music if available
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.backgroundMusic);
    }

    void ResetGameManagerState()
    {
        // Reset GameManager về trạng thái ban đầu
        if (GameManager.Instance != null)
        {
            Debug.Log("Resetting GameManager state in MainMenu");

            // Gọi method reset từ GameManager
            GameManager.Instance.ResetToMainMenu();
        }
    }

    void SetupButtons()
    {
        // Main menu buttons
        playButton?.onClick.AddListener(PlayGame);
        characterButton?.onClick.AddListener(() => ShowPanel(characterPanel));
        leaderboardButton?.onClick.AddListener(() => ShowPanel(leaderboardPanel));
        settingsButton?.onClick.AddListener(() => ShowPanel(settingsPanel));
        creditsButton?.onClick.AddListener(() => ShowPanel(creditsPanel));
        exitButton?.onClick.AddListener(ExitGame);

        // Back buttons
        foreach (var btn in backButtons)
            btn?.onClick.AddListener(ShowMainMenu);
    }

    // Method để vào màn chơi
    public void PlayGame()
    {
        Debug.Log("Loading game scene: " + gameSceneName);

        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // QUAN TRỌNG: Reset GameManager trước khi load scene
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PrepareForNewGame();
        }

        // Load scene màn chơi
        SceneManager.LoadScene(gameSceneName);
    }

    // Method để thoát game
    public void ExitGame()
    {
        Debug.Log("Exiting game");

        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Thoát game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void ShowMainMenu()
    {
        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        HideAllPanels();
        mainMenuPanel?.SetActive(true);
        UpdateDisplays();
    }

    void ShowPanel(GameObject panel)
    {
        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        HideAllPanels();
        panel?.SetActive(true);
    }

    void HideAllPanels()
    {
        mainMenuPanel?.SetActive(false);
        characterPanel?.SetActive(false);
        leaderboardPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
    }

    public void UpdateDisplays()
    {
        if (coinsText != null)
            coinsText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
    }
}