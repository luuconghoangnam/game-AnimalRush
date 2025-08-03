using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    public int selectedCharacterIndex = 0;
    public Transform spawnPoint;

    void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        // Kiểm tra nếu có prefab và index hợp lệ
        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogError("Không có player prefab nào được gán!");
            return;
        }

        // Đảm bảo index nằm trong phạm vi hợp lệ
        selectedCharacterIndex = Mathf.Clamp(selectedCharacterIndex, 0, playerPrefabs.Length - 1);

        // Lấy thông tin nhân vật đã được chọn từ GameManager nếu có
        if (GameManager.Instance != null)
        {
            selectedCharacterIndex = GameManager.Instance.selectedCharacterIndex;
        }

        // Xác định vị trí spawn
        Vector3 position = spawnPoint != null ? spawnPoint.position : new Vector3(0, 1, 0);

        // Tạo player từ prefab
        GameObject player = Instantiate(playerPrefabs[selectedCharacterIndex], position, Quaternion.identity);
        player.tag = "Player"; // Đảm bảo tag là Player

        // Thông báo cho GameManager biết player đã được spawn
        if (GameManager.Instance != null)
        {
            // GameManager.Instance.OnPlayerSpawned(player);
        }

        Debug.Log("Đã spawn player: " + player.name);
    }
}