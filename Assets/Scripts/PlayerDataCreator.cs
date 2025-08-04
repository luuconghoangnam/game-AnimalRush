using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerDataCreator : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Quick Setup")]
    public string playerName = "New Player";
    public Sprite playerSprite;
    public AnimationClip[] playerAnimations = new AnimationClip[4]; // Idle, Run, Jump, Die
    
    [Header("Stats")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int unlockCost = 0;
    
    [Header("Abilities")]
    public bool hasDoubleJump = false;
    public bool hasSpeedBoost = false;
    public bool hasMagnetEffect = false;
    public bool hasShield = false;
    
    [ContextMenu("Create Player Data")]
    void CreatePlayerData()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogError("Player name is required!");
            return;
        }
        
        // Tạo PlayerData asset
        PlayerData newPlayerData = ScriptableObject.CreateInstance<PlayerData>();
        
        // Setup basic info
        newPlayerData.playerName = playerName;
        newPlayerData.playerSprite = playerSprite;
        
        // Setup animations
        if (playerAnimations.Length >= 4)
        {
            newPlayerData.idleAnimation = playerAnimations[0];
            newPlayerData.runAnimation = playerAnimations[1];
            newPlayerData.jumpAnimation = playerAnimations[2];
            newPlayerData.dieAnimation = playerAnimations[3];
        }
        
        // Setup stats
        newPlayerData.moveSpeed = moveSpeed;
        newPlayerData.jumpForce = jumpForce;
        newPlayerData.unlockCost = unlockCost;
        
        // Setup abilities
        newPlayerData.hasDoubleJump = hasDoubleJump;
        newPlayerData.hasSpeedBoost = hasSpeedBoost;
        newPlayerData.hasMagnetEffect = hasMagnetEffect;
        newPlayerData.hasShield = hasShield;
        
        // Lưu asset
        string path = "Assets/PlayerData_" + playerName.Replace(" ", "_") + ".asset";
        AssetDatabase.CreateAsset(newPlayerData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select asset trong Project window
        Selection.activeObject = newPlayerData;
        EditorGUIUtility.PingObject(newPlayerData);
        
        Debug.Log("Created PlayerData: " + path);
    }
    
    [ContextMenu("Auto-Find Animations")]
    void AutoFindAnimations()
    {
        if (playerSprite == null)
        {
            Debug.LogError("Player sprite is required for auto-finding animations!");
            return;
        }
        
        string spritePath = AssetDatabase.GetAssetPath(playerSprite);
        string spriteFolder = System.IO.Path.GetDirectoryName(spritePath);
        
        // Tìm animations trong cùng thư mục
        string[] animationGUIDs = AssetDatabase.FindAssets("t:AnimationClip", new[] { spriteFolder });
        
        foreach (string guid in animationGUIDs)
        {
            string animPath = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
            
            if (clip.name.ToLower().Contains("idle"))
                playerAnimations[0] = clip;
            else if (clip.name.ToLower().Contains("run"))
                playerAnimations[1] = clip;
            else if (clip.name.ToLower().Contains("jump"))
                playerAnimations[2] = clip;
            else if (clip.name.ToLower().Contains("die"))
                playerAnimations[3] = clip;
        }
        
        Debug.Log("Auto-found animations for " + playerName);
    }
#endif
} 