using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    
    [Header("Player Settings")]
    public PlayerData[] availablePlayers; // Tất cả nhân vật có thể chọn
    public PlayerController playerController; // Reference đến PlayerController trong scene
    
    [Header("Current Player")]
    public int currentPlayerIndex = 0; // Thay đổi số này để chọn player mặc định khác
    
    void Awake()
    {
        // Singleton pattern
        // PlayerPrefs.DeleteKey("SelectedPlayer");

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
        LoadSelectedPlayer();
        ApplyCurrentPlayer();
    }
    
    void LoadSelectedPlayer()
    {
        // Load nhân vật đã chọn từ PlayerPrefs
        currentPlayerIndex = PlayerPrefs.GetInt("SelectedPlayer", 0);
        
        // Đảm bảo index hợp lệ
        if (currentPlayerIndex >= availablePlayers.Length)
            currentPlayerIndex = 0;
    }
    
    public void SelectPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= availablePlayers.Length)
        {
            Debug.LogError("Invalid player index: " + playerIndex);
            return;
        }
        PlayerData selectedPlayer = availablePlayers[playerIndex];
        
        // Kiểm tra xem nhân vật đã được mở khóa chưa
        if (!IsPlayerUnlocked(playerIndex))
        {
            Debug.Log("Player " + selectedPlayer.playerName + " is not unlocked yet!");
            return;
        }
        
        currentPlayerIndex = playerIndex;
        SaveSelectedPlayer();
        ApplyCurrentPlayer();
    }
    
    public bool IsPlayerUnlocked(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= availablePlayers.Length)
            return false;
            
        PlayerData player = availablePlayers[playerIndex];
        
        // Nhân vật đầu tiên luôn được mở khóa
        if (playerIndex == 0)
            return true;
            
        // Kiểm tra trong PlayerPrefs
        return PlayerPrefs.GetInt("Player_" + playerIndex + "_Unlocked", 0) == 1;
    }
    
    public bool CanUnlockPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= availablePlayers.Length)
            return false;
            
        if (IsPlayerUnlocked(playerIndex))
            return true;
            
        PlayerData player = availablePlayers[playerIndex];
        return GameManager.Instance.GetTotalCoins() >= player.unlockCost;
    }
    
    public bool UnlockPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= availablePlayers.Length)
            return false;
            
        if (IsPlayerUnlocked(playerIndex))
            return true;
            
        PlayerData player = availablePlayers[playerIndex];
        
        // Kiểm tra có đủ coin không
        if (GameManager.Instance.GetTotalCoins() >= player.unlockCost)
        {
            // Trừ coin
            GameManager.Instance.SpendTotalCoins(player.unlockCost);
            
            // Mở khóa nhân vật
            PlayerPrefs.SetInt("Player_" + playerIndex + "_Unlocked", 1);
            PlayerPrefs.Save();
            
            Debug.Log("Unlocked player: " + player.playerName);
            return true;
        }
        
        return false;
    }
    
    void ApplyCurrentPlayer()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController reference is missing!");
            return;
        }
        
        if (currentPlayerIndex < 0 || currentPlayerIndex >= availablePlayers.Length)
        {
            Debug.LogError("Invalid current player index!");
            return;
        }
        
        // Áp dụng PlayerData cho controller
        playerController.playerData = availablePlayers[currentPlayerIndex];
        
        // Reinitialize player với data mới
        playerController.SendMessage("InitializePlayer", SendMessageOptions.DontRequireReceiver);
    }
    
    void SaveSelectedPlayer()
    {
        PlayerPrefs.SetInt("SelectedPlayer", currentPlayerIndex);
        PlayerPrefs.Save();
    }
    
    // Getters để UI có thể sử dụng
    public PlayerData GetCurrentPlayer()
    {
        if (currentPlayerIndex >= 0 && currentPlayerIndex < availablePlayers.Length)
            return availablePlayers[currentPlayerIndex];
        return null;
    }
    
    public PlayerData GetPlayer(int index)
    {
        if (index >= 0 && index < availablePlayers.Length)
            return availablePlayers[index];
        return null;
    }
    
    public int GetPlayerCount()
    {
        return availablePlayers.Length;
    }
    
    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }
    
    // Method để reset tất cả unlock (for testing)
    [ContextMenu("Reset All Unlocks")]
    public void ResetAllUnlocks()
    {
        for (int i = 0; i < availablePlayers.Length; i++)
        {
            PlayerPrefs.DeleteKey("Player_" + i + "_Unlocked");
        }
        PlayerPrefs.Save();
        Debug.Log("All player unlocks have been reset!");
    }
}