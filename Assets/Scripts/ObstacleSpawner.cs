using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    public static ObstacleSpawner Instance;

    [Header("Spawn Settings")]
    public ObstacleData[] availableObstacles; // Tất cả chướng ngại có thể spawn
    public Transform spawnPoint; // Điểm spawn cố định (kéo GameObject vào đây)
    public float spawnDistance = 15f; // Chỉ dùng khi tạo spawnPoint mặc định

    [Header("Simple Spawn Timing")]
    public float minSpawnInterval = 5f; // Tối thiểu 5s
    public float maxSpawnInterval = 10f; // Tối đa 10s
    public bool enableTimeDecrease = true; // Có giảm thời gian spawn theo thời gian không
    public float decreaseRate = 0.1f; // Giảm 0.1s mỗi lần
    public float decreaseInterval = 30f; // Giảm mỗi 30s

    [Header("Height Settings")]
    public float groundHeight = 0f;
    public float lowHeight = 2f;
    public float mediumHeight = 4f;
    public float highHeight = 6f;

    [Header("Debug")]
    public bool showDebugInfo = true;

    // Private variables
    private float nextSpawnTime = 0f;
    private float currentMinInterval;
    private float currentMaxInterval;
    private float gameStartTime;
    private float nextDecreaseTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeSpawner();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanPlay())
            return;

        // Update timing if enabled
        if (enableTimeDecrease)
        {
            UpdateSpawnTiming();
        }

        // Check if it's time to spawn
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomObstacle();
            ScheduleNextSpawn();
        }
    }

    void InitializeSpawner()
    {
        // Setup spawn point if not assigned
        if (spawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("SpawnPoint");
            spawnPoint = spawnPointObj.transform;
            spawnPoint.position = new Vector3(spawnDistance, 0, 0);
            Debug.LogWarning("SpawnPoint not assigned! Created default SpawnPoint at " + spawnPoint.position);
        }

        // Initialize timing
        gameStartTime = Time.time;
        currentMinInterval = minSpawnInterval;
        currentMaxInterval = maxSpawnInterval;
        nextDecreaseTime = gameStartTime + decreaseInterval;

        // DEBUG: Print all available obstacles
        if (showDebugInfo)
        {
            Debug.Log($"=== SIMPLE OBSTACLE SPAWNER INITIALIZED ===");
            Debug.Log($"Total obstacles: {availableObstacles.Length}");
            Debug.Log($"Spawn interval: {currentMinInterval}s - {currentMaxInterval}s");

            for (int i = 0; i < availableObstacles.Length; i++)
            {
                var obs = availableObstacles[i];
                if (obs != null)
                {
                    Debug.Log($"Element {i}: {obs.obstacleName} | Prefab: {(obs.obstaclePrefab != null ? "✅" : "❌")}");
                }
                else
                {
                    Debug.LogError($"Element {i}: NULL OBSTACLE DATA!");
                }
            }
        }

        // Schedule first spawn
        ScheduleNextSpawn();
    }

    void UpdateSpawnTiming()
    {
        // Giảm spawn interval theo thời gian
        if (Time.time >= nextDecreaseTime)
        {
            if (currentMaxInterval > currentMinInterval + 1f) // Đảm bảo max > min
            {
                currentMinInterval = Mathf.Max(currentMinInterval - decreaseRate, 3f); // Tối thiểu 3s
                currentMaxInterval = Mathf.Max(currentMaxInterval - decreaseRate, currentMinInterval + 1f);

                nextDecreaseTime = Time.time + decreaseInterval;

                if (showDebugInfo)
                {
                    Debug.Log($"Spawn interval decreased to {currentMinInterval}s - {currentMaxInterval}s");
                }
            }
        }
    }

    void SpawnRandomObstacle()
    {
        if (availableObstacles.Length == 0)
        {
            if (showDebugInfo)
                Debug.LogWarning("No obstacles available to spawn!");
            return;
        }

        // Lọc obstacles có prefab
        List<ObstacleData> validObstacles = new List<ObstacleData>();

        for (int i = 0; i < availableObstacles.Length; i++)
        {
            ObstacleData obstacle = availableObstacles[i];

            if (obstacle != null && obstacle.obstaclePrefab != null)
            {
                validObstacles.Add(obstacle);
            }
            else if (showDebugInfo)
            {
                Debug.LogWarning($"Element {i}: {(obstacle?.obstacleName ?? "NULL")} - Missing prefab or null data!");
            }
        }

        if (validObstacles.Count == 0)
        {
            if (showDebugInfo)
                Debug.LogError("No valid obstacles found with prefabs!");
            return;
        }

        // Chọn ngẫu nhiên 1 obstacle
        ObstacleData selectedObstacle = validObstacles[Random.Range(0, validObstacles.Count)];

        if (showDebugInfo)
        {
            Debug.Log($"=== SPAWNING OBSTACLE ===");
            Debug.Log($"🎯 Selected: {selectedObstacle.obstacleName} (from {validObstacles.Count} valid options)");
        }

        // Tính toán vị trí spawn
        Vector3 spawnPosition = CalculateSpawnPosition(selectedObstacle);

        // Spawn obstacle
        SpawnObstacleWithController(selectedObstacle, spawnPosition);
    }

    void SpawnObstacleWithController(ObstacleData obstacleData, Vector3 position)
    {
        GameObject obstacleObj = Instantiate(obstacleData.obstaclePrefab, position, Quaternion.identity);

        // Check if this is a flying bird or regular obstacle
        FlyingBirdController birdController = obstacleObj.GetComponent<FlyingBirdController>();
        if (birdController != null)
        {
            // This is a bird - no need to add ObstacleController
            if (showDebugInfo)
                Debug.Log($"✅ Spawned flying bird: {obstacleData.obstacleName} at {position}");
        }
        else
        {
            // This is a regular obstacle - add ObstacleController
            ObstacleController controller = obstacleObj.GetComponent<ObstacleController>();
            if (controller == null)
                controller = obstacleObj.AddComponent<ObstacleController>();

            controller.SetObstacleData(obstacleData);
            if (showDebugInfo)
                Debug.Log($"✅ Spawned obstacle: {obstacleData.obstacleName} ({obstacleData.obstacleType}) at {position}");
        }
    }

    Vector3 CalculateSpawnPosition(ObstacleData obstacle)
    {
        Vector3 basePosition = spawnPoint.position;
        float spawnY = groundHeight;

        switch (obstacle.spawnHeight)
        {
            case SpawnHeight.Ground:
                spawnY = groundHeight;
                break;
            case SpawnHeight.Low:
                spawnY = lowHeight;
                break;
            case SpawnHeight.Medium:
                spawnY = mediumHeight;
                break;
            case SpawnHeight.High:
                spawnY = highHeight;
                break;
            case SpawnHeight.Random:
                float[] heights = { groundHeight, lowHeight, mediumHeight, highHeight };
                spawnY = heights[Random.Range(0, heights.Length)];
                break;
        }

        return new Vector3(basePosition.x, spawnY, basePosition.z);
    }

    void ScheduleNextSpawn()
    {
        // Random interval giữa min và max
        float randomInterval = Random.Range(currentMinInterval, currentMaxInterval);
        nextSpawnTime = Time.time + randomInterval;

        if (showDebugInfo)
            Debug.Log($"Next obstacle spawn in {randomInterval:F1}s (range: {currentMinInterval:F1}s - {currentMaxInterval:F1}s)");
    }

    // Public methods
    public void StopSpawning()
    {
        nextSpawnTime = float.MaxValue;
        if (showDebugInfo)
            Debug.Log("Obstacle spawning stopped");
    }

    public void ResumeSpawning()
    {
        ScheduleNextSpawn();
        if (showDebugInfo)
            Debug.Log("Obstacle spawning resumed");
    }

    public void SpawnSpecificObstacle(int index)
    {
        if (index >= 0 && index < availableObstacles.Length && availableObstacles[index] != null)
        {
            Vector3 position = CalculateSpawnPosition(availableObstacles[index]);
            SpawnObstacleWithController(availableObstacles[index], position);
        }
    }

    public void ForceSpawnRandom()
    {
        SpawnRandomObstacle();
        ScheduleNextSpawn();
    }

    // Debug info
    void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 1f);

            // Draw height levels
            Gizmos.color = Color.green;
            float x = spawnPoint.position.x;
            Gizmos.DrawLine(new Vector3(x - 2, groundHeight, 0), new Vector3(x + 2, groundHeight, 0));
            Gizmos.DrawLine(new Vector3(x - 2, lowHeight, 0), new Vector3(x + 2, lowHeight, 0));
            Gizmos.DrawLine(new Vector3(x - 2, mediumHeight, 0), new Vector3(x + 2, mediumHeight, 0));
            Gizmos.DrawLine(new Vector3(x - 2, highHeight, 0), new Vector3(x + 2, highHeight, 0));
        }
    }
}