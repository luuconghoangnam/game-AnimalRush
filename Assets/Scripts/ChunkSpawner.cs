using UnityEngine;
using System.Collections.Generic;

public class ChunkSpawner : MonoBehaviour
{
    [Header("Chunk Settings")]
    public GameObject[] chunkPrefabs;
    public float chunkLength = 20f;
    public int initialChunks = 5;

    [Header("Obstacle Settings")]
    public GameObject[] obstaclesPrefabs;
    public GameObject[] enemyPrefabs;
    public GameObject coinPrefab;

    private Transform playerTransform;
    private float spawnX = 0;
    private List<GameObject> activeChunks = new List<GameObject>();

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("ChunkSpawner: Player not found! Make sure your player has the 'Player' tag.");
            return;
        }

        // Khởi tạo các chunk đầu tiên
        for (int i = 0; i < initialChunks; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Kiểm tra danh sách chunk có phần tử không
        if (activeChunks.Count == 0) return;

        // Kiểm tra và loại bỏ các chunk null (đã bị phá hủy)
        activeChunks.RemoveAll(chunk => chunk == null);

        // Kiểm tra lại sau khi đã loại bỏ
        if (activeChunks.Count == 0) return;

        // Nếu người chơi đã di chuyển qua 1 nửa chunk cuối cùng, tạo chunk mới
        float playerX = playerTransform.position.x;
        GameObject lastChunk = activeChunks[activeChunks.Count - 1];

        if (lastChunk == null)
        {
            // Tạo chunk mới nếu chunk cuối cùng không tồn tại
            SpawnChunk();
            return;
        }

        float lastChunkEndX = lastChunk.transform.position.x + chunkLength;

        if (playerX > lastChunkEndX - (2 * chunkLength))
        {
            SpawnChunk();

            // Xóa chunk đầu tiên nếu đã đi quá xa
            while (activeChunks.Count > initialChunks && activeChunks.Count > 0)
            {
                GameObject oldestChunk = activeChunks[0];
                if (oldestChunk != null)
                {
                    activeChunks.RemoveAt(0);
                    Destroy(oldestChunk);
                }
                else
                {
                    activeChunks.RemoveAt(0);
                }
            }
        }
    }

    void SpawnChunk()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("ChunkSpawner: No chunk prefabs assigned!");
            return;
        }

        // Chọn ngẫu nhiên một chunk
        int randomIndex = Random.Range(0, chunkPrefabs.Length);
        GameObject chunkPrefab = chunkPrefabs[randomIndex];

        // Tạo chunk mới
        GameObject newChunk = Instantiate(chunkPrefab, new Vector3(spawnX, 0, 0), Quaternion.identity, transform);
        activeChunks.Add(newChunk);

        // Thêm chướng ngại vật và đồng xu
        SpawnObstaclesInChunk(newChunk);

        // Cập nhật vị trí cho chunk tiếp theo
        spawnX += chunkLength;
    }

    void SpawnObstaclesInChunk(GameObject chunk)
    {
        if (chunk == null) return;

        // Điểm bắt đầu và kết thúc của chunk
        float chunkStartX = chunk.transform.position.x;
        float chunkEndX = chunkStartX + chunkLength;

        // Số lượng chướng ngại vật (tăng theo tốc độ game)
        int obstacleCount = Mathf.RoundToInt(2 + (GameManager.Instance != null ? GameManager.Instance.currentGameSpeed / 5 : 0));

        // Sinh chướng ngại vật ngẫu nhiên
        if (obstaclesPrefabs != null && obstaclesPrefabs.Length > 0)
        {
            for (int i = 0; i < obstacleCount; i++)
            {
                float obstacleX = Random.Range(chunkStartX + 2, chunkEndX - 2);
                int obstacleType = Random.Range(0, obstaclesPrefabs.Length + 1);

                if (obstacleType < obstaclesPrefabs.Length)
                {
                    // Tạo chướng ngại vật thông thường
                    Instantiate(
                        obstaclesPrefabs[obstacleType],
                        new Vector3(obstacleX, 0, 0),
                        Quaternion.identity,
                        chunk.transform
                    );
                }
                else if (enemyPrefabs != null && enemyPrefabs.Length > 0 && Random.value < 0.3f) // 30% cơ hội tạo kẻ địch
                {
                    // Tạo kẻ địch
                    int enemyIndex = Random.Range(0, enemyPrefabs.Length);
                    Instantiate(
                        enemyPrefabs[enemyIndex],
                        new Vector3(obstacleX, 1, 0),
                        Quaternion.identity,
                        chunk.transform
                    );
                }
            }
        }

        // Sinh xu
        if (coinPrefab != null)
        {
            int coinCount = Random.Range(5, 10);
            for (int i = 0; i < coinCount; i++)
            {
                float coinX = Random.Range(chunkStartX, chunkEndX);
                float coinY = Random.Range(1.5f, 3f);

                Instantiate(
                    coinPrefab,
                    new Vector3(coinX, coinY, 0),
                    Quaternion.identity,
                    chunk.transform
                );
            }
        }
    }
}