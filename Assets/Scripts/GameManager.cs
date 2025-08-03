using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    // Thông số game
    [Header("Game Settings")]
    public float initialGameSpeed = 5f;
    public float maxGameSpeed = 15f;
    public float speedIncreaseRate = 0.1f;
    public float currentGameSpeed;
    public int score = 0;
    public int highScore = 0;

    // Quản lý xu và nhân vật
    [Header("Currency & Characters")]
    public int coins = 0;
    public int selectedCharacterIndex = 0;
    public List<CharacterData> characters = new List<CharacterData>();

    // Trạng thái game
    public enum GameState { MainMenu, Playing, GameOver, Paused, Shop }
    public GameState currentState = GameState.MainMenu;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetGame();
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            // Tăng tốc độ dần theo thời gian
            if (currentGameSpeed < maxGameSpeed)
            {
                currentGameSpeed += speedIncreaseRate * Time.deltaTime;
            }

            // Tăng điểm theo thời gian
            score += Mathf.RoundToInt(Time.deltaTime * currentGameSpeed);
        }
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        ResetGame();
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        
        // Cập nhật high score
        if (score > highScore)
        {
            highScore = score;
            SaveGameData();
        }
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0;
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1;
        }
    }

    public void ResetGame()
    {
        currentGameSpeed = initialGameSpeed;
        score = 0;
        Time.timeScale = 1;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveGameData();
    }

    public bool PurchaseCharacter(int characterIndex, int price)
    {
        if (coins >= price)
        {
            coins -= price;
            characters[characterIndex].isUnlocked = true;
            SaveGameData();
            return true;
        }
        return false;
    }

    public void SelectCharacter(int index)
    {
        if (characters[index].isUnlocked)
        {
            selectedCharacterIndex = index;
            SaveGameData();
        }
    }

    public void SaveGameData()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacterIndex);
        
        // Lưu trạng thái mở khóa nhân vật
        for (int i = 0; i < characters.Count; i++)
        {
            PlayerPrefs.SetInt("Character_" + i + "_Unlocked", characters[i].isUnlocked ? 1 : 0);
        }
        
        PlayerPrefs.Save();
    }

    public void LoadGameData()
    {
        coins = PlayerPrefs.GetInt("Coins", 0);
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        
        InitializeCharacters();
        
        // Load trạng thái mở khóa nhân vật
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].isUnlocked = PlayerPrefs.GetInt("Character_" + i + "_Unlocked", i == 0 ? 1 : 0) == 1;
        }
    }

    private void InitializeCharacters()
    {
        // Khởi tạo danh sách nhân vật (sẽ bổ sung thêm sau)
        if (characters.Count == 0)
        {
            // Nhân vật mặc định (đã mở khóa)
            characters.Add(new CharacterData { 
                characterName = "Ninja Đỏ", 
                price = 0, 
                isUnlocked = true,
                specialAbility = "Không có kỹ năng đặc biệt"
            });
            
            // Thêm các nhân vật khác (chưa mở khóa)
            characters.Add(new CharacterData { 
                characterName = "Ninja Xanh", 
                price = 1000, 
                isUnlocked = false,
                specialAbility = "Nhảy cao hơn"
            });
            
            characters.Add(new CharacterData { 
                characterName = "Ninja Đen", 
                price = 2500, 
                isUnlocked = false,
                specialAbility = "Miễn nhiễm 1 lần va chạm"
            });
            
            characters.Add(new CharacterData { 
                characterName = "Ninja Vàng", 
                price = 5000, 
                isUnlocked = false,
                specialAbility = "Thu thập xu gấp đôi"
            });
        }
    }
}

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public int price;
    public bool isUnlocked;
    public string specialAbility;
    // Có thể bổ sung thêm các thuộc tính khác
}
