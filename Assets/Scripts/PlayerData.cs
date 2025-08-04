using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Basic Info")]
    public string playerName;
    public Sprite playerSprite;
    
    [Header("Animation Settings")]
    public AnimationClip idleAnimation;
    public AnimationClip runAnimation;
    public AnimationClip jumpAnimation;
    public AnimationClip dieAnimation;
    
    [Header("Stats")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int unlockCost = 0; // Số coin cần để mở khóa
    
    [Header("Special Abilities")]
    public bool hasDoubleJump = false;
    public bool hasSpeedBoost = false;
    public bool hasMagnetEffect = false;
    public bool hasShield = false;
    
    [Header("Special Effects")]
    public GameObject specialEffect; // Prefab effect đặc biệt
    public AudioClip specialSound;   // Âm thanh đặc biệt
    
    [Header("Unlock Status")]
    public bool isUnlocked = false;
} 