using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public Transform layerTransform; // Layer background (GameObject con)
    public float speed;              // Tốc độ di chuyển
    [HideInInspector] public Vector3 startPos;
    [HideInInspector] public float length;
}

public class ParallaxController : MonoBehaviour
{
    public ParallaxLayer[] layers; // Thêm các layer vào đây trong Inspector
    
    [Header("Speed Settings")]
    public float baseSpeedMultiplier = 1f;
    public float maxSpeedMultiplier = 10f;
    public float speedIncreaseRate = 1.1f; // Tăng tốc mỗi giây
    public float speedIncreaseInterval = 5f; // Tăng tốc mỗi 5 giây
    
    private float currentSpeedMultiplier = 1f;
    private float gameTime = 0f;
    private float nextSpeedIncreaseTime = 0f;

    void Start()
    {
        foreach (var layer in layers)
        {
            layer.startPos = layer.layerTransform.position;
            // Lấy chiều dài sprite (giả sử mỗi layer có 2 sprite con liền nhau)
            SpriteRenderer sr = layer.layerTransform.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                layer.length = sr.bounds.size.x;
            else
                layer.length = 10f; // fallback nếu không tìm thấy
        }
        
        // Initialize speed system
        currentSpeedMultiplier = baseSpeedMultiplier;
        nextSpeedIncreaseTime = speedIncreaseInterval;
        
        Debug.Log($"ParallaxController initialized with speed: {currentSpeedMultiplier}x");
    }

    void Update()
    {
        // Chỉ update khi game có thể chơi
        if (GameManager.Instance == null || !GameManager.Instance.CanPlay())
            return;
            
        // Update speed system
        UpdateSpeedSystem();
        
        foreach (var layer in layers)
        {
            // Di chuyển layer sang trái với tốc độ tăng dần
            float currentSpeed = layer.speed * currentSpeedMultiplier;
            layer.layerTransform.position += Vector3.left * currentSpeed * Time.deltaTime;

            // Nếu đã đi hết chiều dài, reset lại vị trí
            if (layer.layerTransform.position.x <= layer.startPos.x - layer.length)
            {
                layer.layerTransform.position = new Vector3(layer.startPos.x, layer.startPos.y, layer.startPos.z);
            }
        }
    }
    
    void UpdateSpeedSystem()
    {
        gameTime += Time.deltaTime;
        
        // Kiểm tra xem có cần tăng tốc không
        if (gameTime >= nextSpeedIncreaseTime)
        {
            IncreaseSpeed();
            nextSpeedIncreaseTime = gameTime + speedIncreaseInterval;
        }
    }
    
    void IncreaseSpeed()
    {
        if (currentSpeedMultiplier < maxSpeedMultiplier)
        {
            float oldSpeed = currentSpeedMultiplier;
            currentSpeedMultiplier = Mathf.Min(currentSpeedMultiplier * speedIncreaseRate, maxSpeedMultiplier);
            Debug.Log($"Parallax speed increased from {oldSpeed:F2}x to {currentSpeedMultiplier:F2}x");
        }
        else
        {
            Debug.Log($"Parallax speed reached maximum: {currentSpeedMultiplier:F2}x");
        }
    }
    
    // Public methods để các script khác có thể sử dụng
    public float GetCurrentSpeedMultiplier()
    {
        return currentSpeedMultiplier;
    }
    
    public float GetGameTime()
    {
        return gameTime;
    }
    
    public void ResetSpeed()
    {
        currentSpeedMultiplier = baseSpeedMultiplier;
        gameTime = 0f;
        nextSpeedIncreaseTime = speedIncreaseInterval;
        Debug.Log($"Parallax speed reset to: {currentSpeedMultiplier}x");
    }
    
    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Clamp(multiplier, baseSpeedMultiplier, maxSpeedMultiplier);
    }
}
