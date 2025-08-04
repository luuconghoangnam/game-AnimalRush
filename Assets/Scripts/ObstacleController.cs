using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [Header("Setup")]
    public ObstacleData obstacleData;

    [Header("Debug")]
    public bool showDebugInfo = false;

    [Header("Destroy Settings")]
    public float destroyDistance = 20f;

    // Components
    private Rigidbody2D rb;
    private Collider2D obstacleCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Movement variables
    private Vector3 startPosition;
    private float currentMoveSpeed;
    private bool isInitialized = false;
    private float totalDistanceTraveled = 0f;

    // Flying bird specific
    private float flyingTimer = 0f;
    private float originalY;

    void Start()
    {
        InitializeObstacle();
    }

    void Update()
    {
        if (!isInitialized || GameManager.Instance == null || !GameManager.Instance.CanPlay())
            return;

        HandleObstacleBehavior();
        CheckIfShouldDestroy();
    }

    void InitializeObstacle()
    {
        if (obstacleData == null)
        {
            Debug.LogError($"[OBSTACLE] {name} has no ObstacleData assigned!");
            return;
        }

        // Get components
        rb = GetComponent<Rigidbody2D>();
        obstacleCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Store start position
        startPosition = transform.position;
        originalY = transform.position.y;
        totalDistanceTraveled = 0f;

        // Calculate movement speed
        currentMoveSpeed = CalculateMovementSpeed();

        // Setup collider based on obstacle type
        SetupCollider();

        // Specific setup based on obstacle type
        SetupObstacleType();

        isInitialized = true;

        if (showDebugInfo)
        {
            Debug.Log($"[OBSTACLE] {obstacleData.obstacleName} initialized at {transform.position} with speed {currentMoveSpeed}");
        }
    }

    void SetupCollider()
    {
        if (obstacleCollider != null)
        {
            // SafeBox (Box type) should be solid platform, others are triggers
            if (obstacleData.obstacleType == ObstacleType.Box && obstacleData.damage == 0)
            {
                // This is SafeBox - solid platform
                obstacleCollider.isTrigger = false;
                gameObject.tag = "Platform"; // Or "SafeBox"

                if (showDebugInfo)
                {
                    Debug.Log($"[OBSTACLE] {obstacleData.obstacleName} setup as PLATFORM (solid collider)");
                }
            }
            else
            {
                // Regular obstacle - trigger for damage detection
                obstacleCollider.isTrigger = true;
                gameObject.tag = "Obstacle";

                if (showDebugInfo)
                {
                    Debug.Log($"[OBSTACLE] {obstacleData.obstacleName} setup as OBSTACLE (trigger collider)");
                }
            }
        }
    }

    void SetupObstacleType()
    {
        switch (obstacleData.obstacleType)
        {
            case ObstacleType.Box:
                // Box có thể là SafeBox (damage = 0) hoặc regular box (damage > 0)
                if (obstacleData.damage == 0)
                {
                    // SafeBox - không cần setup đặc biệt
                    if (showDebugInfo)
                    {
                        Debug.Log($"[SAFEBOX] {obstacleData.obstacleName} configured as safe platform");
                    }
                }
                break;

            case ObstacleType.FlyingBird:
                // Random start phase cho flying animation
                flyingTimer = Random.Range(0f, 2f * Mathf.PI);
                break;

            case ObstacleType.Pit:
                // Pit cần collider lớn hơn để detect player rơi vào
                if (obstacleCollider != null)
                {
                    BoxCollider2D boxCol = obstacleCollider as BoxCollider2D;
                    if (boxCol != null)
                    {
                        Vector2 size = boxCol.size;
                        size.y *= 1.5f; // Tăng chiều cao để dễ detect
                        boxCol.size = size;
                    }
                }
                break;

            case ObstacleType.Enemy:
                // Enemy có thể có animation walking
                if (animator != null)
                {
                    animator.SetBool("IsWalking", true);
                }
                break;
        }
    }

    void HandleObstacleBehavior()
    {
        // Di chuyển cơ bản về phía player (sang trái)
        MoveTowardsPlayer();

        // Hành vi đặc biệt theo loại obstacle
        switch (obstacleData.obstacleType)
        {
            case ObstacleType.Box:
                HandleBoxBehavior();
                break;

            case ObstacleType.FlyingBird:
                HandleFlyingBirdBehavior();
                break;

            case ObstacleType.Pit:
                HandlePitBehavior();
                break;

            case ObstacleType.Enemy:
                HandleEnemyBehavior();
                break;
        }
    }

    void MoveTowardsPlayer()
    {
        // Tính khoảng cách di chuyển trong frame này
        float moveDistance = currentMoveSpeed * Time.deltaTime;

        // Di chuyển sang trái với tốc độ đã tính
        transform.position += Vector3.left * moveDistance;

        // Cộng dồn khoảng cách đã di chuyển
        totalDistanceTraveled += moveDistance;
    }

    void HandleBoxBehavior()
    {
        // Box behavior - SafeBox và regular box đều di chuyển giống nhau
        if (showDebugInfo && Time.frameCount % 120 == 0) // Log mỗi 2 giây
        {
            string boxType = obstacleData.damage == 0 ? "SafeBox" : "DamageBox";
            Debug.Log($"[{boxType}] {name} - Position: {transform.position:F1}, Distance: {totalDistanceTraveled:F1}/{destroyDistance}");
        }
    }

    void HandleFlyingBirdBehavior()
    {
        // Bay lên xuống theo sine wave
        flyingTimer += Time.deltaTime * obstacleData.flyingFrequency;
        float yOffset = Mathf.Sin(flyingTimer) * obstacleData.flyingAmplitude;

        Vector3 pos = transform.position;
        pos.y = originalY + yOffset;
        transform.position = pos;

        // Có thể thêm flapping animation
        if (animator != null)
        {
            animator.SetFloat("FlapSpeed", obstacleData.flyingFrequency);
        }

        if (showDebugInfo && Time.frameCount % 120 == 0) // Log mỗi 2 giây
        {
            Debug.Log($"[BIRD] {name} - Y: {pos.y:F1}, Distance: {totalDistanceTraveled:F1}/{destroyDistance}");
        }
    }

    void HandlePitBehavior()
    {
        // Pit không di chuyển theo Y, chỉ di chuyển theo X
        if (showDebugInfo && Time.frameCount % 120 == 0) // Log mỗi 2 giây
        {
            Debug.Log($"[PIT] {name} - Position: {transform.position:F1}, Distance: {totalDistanceTraveled:F1}/{destroyDistance}");
        }
    }

    void HandleEnemyBehavior()
    {
        // Enemy có thể có logic phức tạp hơn
        // Animation
        if (animator != null)
        {
            animator.SetFloat("MoveSpeed", currentMoveSpeed);
        }

        if (showDebugInfo && Time.frameCount % 120 == 0) // Log mỗi 2 giây
        {
            Debug.Log($"[ENEMY] {name} - Position: {transform.position:F1}, Speed: {currentMoveSpeed:F1}, Distance: {totalDistanceTraveled:F1}/{destroyDistance}");
        }
    }

    void CheckIfShouldDestroy()
    {
        // Destroy khi đã di chuyển đủ khoảng cách
        if (totalDistanceTraveled >= destroyDistance)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[OBSTACLE] {name} ({obstacleData.obstacleType}) destroyed - traveled {totalDistanceTraveled:F1} units (limit: {destroyDistance})");
            }

            // Clean destroy without rewards (since player didn't interact)
            Destroy(gameObject);
        }
    }

    float CalculateMovementSpeed()
    {
        float baseSpeed = obstacleData.moveSpeed;

        if (!obstacleData.syncWithGameSpeed)
            return baseSpeed;

        // Đồng bộ với tốc độ parallax nếu có
        ParallaxController parallaxController = FindFirstObjectByType<ParallaxController>();
        if (parallaxController != null)
        {
            float speedMultiplier = parallaxController.GetCurrentSpeedMultiplier();
            return baseSpeed * speedMultiplier;
        }

        return baseSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ xử lý trigger events cho obstacles gây damage
        if (obstacleCollider.isTrigger && showDebugInfo)
        {
            Debug.Log($"[OBSTACLE] {name} ({obstacleData.obstacleType}) collided with {other.name} (Tag: {other.tag}) at distance {totalDistanceTraveled:F1}");
        }

        if (other.CompareTag("Player") && obstacleCollider.isTrigger)
        {
            // Chỉ gây damage nếu obstacle có damage > 0
            if (obstacleData.damage > 0)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(obstacleData.damage);

                    if (showDebugInfo)
                    {
                        Debug.Log($"[OBSTACLE] {name} damaged player for {obstacleData.damage} damage");
                    }
                }

                // Destroy obstacle nếu có thể phá hủy
                if (obstacleData.canBeDestroyed)
                {
                    DestroyObstacle();
                }
            }
        }
    }

    public void DestroyObstacle()
    {
        // Spawn reward nếu có
        if (obstacleData.rewardPrefab != null)
        {
            Instantiate(obstacleData.rewardPrefab, transform.position, Quaternion.identity);
        }

        // Add coin reward
        if (obstacleData.coinReward > 0 && GameManager.Instance != null)
        {
            for (int i = 0; i < obstacleData.coinReward; i++)
            {
                GameManager.Instance.AddCoin();
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[OBSTACLE] {name} destroyed with rewards - Coins: {obstacleData.coinReward}, Distance: {totalDistanceTraveled:F1}");
        }

        Destroy(gameObject);
    }

    // Public method để set obstacle data từ spawner
    public void SetObstacleData(ObstacleData data)
    {
        obstacleData = data;
        if (!isInitialized)
        {
            InitializeObstacle();
        }
    }

    // Public method để force destroy
    public void ForceDestroy()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[OBSTACLE] {name} force destroyed at distance {totalDistanceTraveled:F1}");
        }
        Destroy(gameObject);
    }

    // Public method để get distance info
    public float GetDistanceTraveled()
    {
        return totalDistanceTraveled;
    }

    public float GetRemainingDistance()
    {
        return Mathf.Max(0, destroyDistance - totalDistanceTraveled);
    }

    // Gizmos để debug
    void OnDrawGizmosSelected()
    {
        if (obstacleData != null)
        {
            // Draw obstacle bounds
            Color gizmoColor = obstacleData.damage == 0 ? Color.green : Color.red;
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, 1f);

            // Draw movement direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Vector3.left * 2f);

            // Draw traveled distance visualization
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(startPosition, transform.position);

                // Draw progress bar
                float progress = totalDistanceTraveled / destroyDistance;
                Gizmos.color = Color.Lerp(Color.green, Color.red, progress);
                Vector3 progressEnd = startPosition + Vector3.left * totalDistanceTraveled;
                Gizmos.DrawWireSphere(progressEnd, 0.5f);

                // Draw destroy point
                Gizmos.color = Color.red;
                Vector3 destroyPoint = startPosition + Vector3.left * destroyDistance;
                Gizmos.DrawWireCube(destroyPoint, Vector3.one * 0.5f);
            }

            // Special gizmos for flying bird
            if (obstacleData.obstacleType == ObstacleType.FlyingBird)
            {
                Gizmos.color = Color.yellow;
                Vector3 pos = transform.position;
                Gizmos.DrawLine(
                    new Vector3(pos.x, pos.y - obstacleData.flyingAmplitude, pos.z),
                    new Vector3(pos.x, pos.y + obstacleData.flyingAmplitude, pos.z)
                );
            }
        }
    }
}