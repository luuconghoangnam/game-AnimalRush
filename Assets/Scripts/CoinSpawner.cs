using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject coinPrefab; // Prefab coin
    public Transform spawnPoint; // Điểm spawn
    public float spawnDistance = 15f; // Khoảng cách spawn mặc định

    [Header("Spawn Heights")]
    public float[] spawnHeights = { 0f, 2f, 4f, 6f }; // Các độ cao có thể spawn
    public float randomHeightChance = 0.3f; // 30% chance spawn ở độ cao random

    [Header("Coin Settings")]
    public int[] coinValues = { 1, 2, 5 }; // Các giá trị coin khác nhau
    public float[] coinValueWeights = { 0.7f, 0.25f, 0.05f }; // Tỷ lệ xuất hiện

    [Header("Debug")]
    public bool showDebugInfo = false;

    // Private variables
    private float nextSpawnTime = 0f;

    void Start()
    {
        InitializeSpawner();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanPlay())
            return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnCoin();
            ScheduleNextSpawn();
        }
    }

    void InitializeSpawner()
    {
        // Setup spawn point if not assigned
        if (spawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("CoinSpawnPoint");
            spawnPoint = spawnPointObj.transform;
            spawnPoint.position = new Vector3(spawnDistance, 0, 0);
            Debug.LogWarning("CoinSpawner: SpawnPoint not assigned! Created default at " + spawnPoint.position);
        }

        // Schedule first spawn
        ScheduleNextSpawn();

        if (showDebugInfo)
        {
            Debug.Log("CoinSpawner initialized");
        }
    }

    void SpawnCoin()
    {
        if (coinPrefab == null) return;

        // Tính vị trí spawn
        Vector3 spawnPosition = CalculateSpawnPosition();

        // Spawn coin
        GameObject coinObj = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);

        // Setup coin controller
        CoinController coinController = coinObj.GetComponent<CoinController>();
        if (coinController == null)
        {
            coinController = coinObj.AddComponent<CoinController>();
        }

        // Set coin value
        int coinValue = SelectCoinValue();
        coinController.SetCoinValue(coinValue);

        if (showDebugInfo)
        {
            Debug.Log($"Spawned coin with value {coinValue} at {spawnPosition}");
        }
    }

    Vector3 CalculateSpawnPosition()
    {
        Vector3 basePosition = spawnPoint.position;

        // Chọn độ cao spawn
        float spawnY = 0f;

        if (Random.Range(0f, 1f) < randomHeightChance)
        {
            // Random height
            spawnY = spawnHeights[Random.Range(0, spawnHeights.Length)];
        }
        else
        {
            // Ground level
            spawnY = spawnHeights[0];
        }

        return new Vector3(basePosition.x, spawnY, basePosition.z);
    }

    int SelectCoinValue()
    {
        if (coinValues.Length != coinValueWeights.Length)
        {
            Debug.LogError("CoinSpawner: coinValues and coinValueWeights arrays must have same length!");
            return 1;
        }

        // Weighted random selection
        float totalWeight = 0f;
        foreach (float weight in coinValueWeights)
        {
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < coinValues.Length; i++)
        {
            currentWeight += coinValueWeights[i];
            if (randomValue <= currentWeight)
            {
                return coinValues[i];
            }
        }

        return coinValues[0]; // Fallback
    }

    void ScheduleNextSpawn()
    {
        if (GameManager.Instance != null)
        {
            float spawnInterval = GameManager.Instance.GetRandomCoinSpawnInterval();
            nextSpawnTime = Time.time + spawnInterval;

            if (showDebugInfo)
                Debug.Log($"Next coin spawn in {spawnInterval}s");
        }
    }

    // Public methods
    public void StopSpawning()
    {
        nextSpawnTime = float.MaxValue;
    }

    public void ResumeSpawning()
    {
        ScheduleNextSpawn();
    }

    public void SpawnCoinAt(Vector3 position, int value = 1)
    {
        if (coinPrefab == null) return;

        GameObject coinObj = Instantiate(coinPrefab, position, Quaternion.identity);
        CoinController coinController = coinObj.GetComponent<CoinController>();
        if (coinController == null)
        {
            coinController = coinObj.AddComponent<CoinController>();
        }
        coinController.SetCoinValue(value);
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnPoint.position, 1f);

            // Draw spawn heights
            Gizmos.color = Color.cyan;
            float x = spawnPoint.position.x;
            foreach (float height in spawnHeights)
            {
                Gizmos.DrawLine(new Vector3(x - 2, height, 0), new Vector3(x + 2, height, 0));
            }
        }
    }
}