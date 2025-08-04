using UnityEngine;

public enum ObstacleType
{
    Box,         // Hộp cố định trên mặt đất
    FlyingBird,  // Chim bay ở độ cao khác nhau
    Pit,         // Hố trên mặt đất
    Enemy        // Kẻ thù di chuyển
}

public enum SpawnHeight
{
    Ground,      // Spawn trên mặt đất (Box, Pit)
    Low,         // Spawn thấp (nhảy 1 lần qua được)
    Medium,      // Spawn vừa (cần nhảy cao)
    High,        // Spawn cao (cần double jump)
    Random       // Spawn ngẫu nhiên (chỉ dành cho FlyingBird)
}

[CreateAssetMenu(fileName = "New Obstacle Data", menuName = "Game/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    [Header("Basic Info")]
    public string obstacleName;
    public GameObject obstaclePrefab;
    public ObstacleType obstacleType;
    public SpawnHeight spawnHeight;

    [Header("Spawn Settings")]
    public float spawnWeight = 1f; // Tỷ lệ spawn (càng cao càng dễ xuất hiện)
    public int minGameTime = 0; // Thời gian tối thiểu để spawn (giây)
    public int maxGameTime = 999; // Thời gian tối đa để spawn

    [Header("Movement Settings")]
    public float moveSpeed = 3f; // Tốc độ di chuyển về phía player
    public bool syncWithGameSpeed = true; // Đồng bộ với tốc độ game

    [Header("Flying Bird Settings")]
    public float flyingAmplitude = 1f; // Biên độ bay lên xuống
    public float flyingFrequency = 2f; // Tần số bay lên xuống

    [Header("Enemy Settings")]
    public float enemySpeed = 2f;
    public bool canJump = false; // Enemy có thể nhảy qua hố không

    [Header("Collision & Damage")]
    public int damage = 1; // Sát thương gây ra
    public bool canBeDestroyed = false; // Có thể phá hủy được không

    [Header("Rewards (optional)")]
    public int coinReward = 0; // Coin nhận được khi phá hủy
    public GameObject rewardPrefab; // Item rơi ra khi phá hủy
}