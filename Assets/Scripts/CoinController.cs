using UnityEngine;

public class CoinController : MonoBehaviour
{
    [Header("Coin Settings")]
    public int coinValue = 1;
    public float moveSpeed = 3f;
    public bool syncWithGameSpeed = true;
    
    [Header("Animation")]
    public float rotationSpeed = 180f; // ??/giây
    public float bobAmplitude = 0.2f; // Biên ?? nh?p nhô
    public float bobFrequency = 2f; // T?n s? nh?p nhô
    
    [Header("Collection")]
    public float magnetRange = 3f; // Ph?m vi magnet khi player có magnet effect
    public float collectSpeed = 10f; // T?c ?? bay v? player khi b? magnet
    
    [Header("Destroy Settings")]
    public float destroyDistance = 25f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private Collider2D coinCollider;
    private AudioSource audioSource;
    
    // Movement variables
    private Vector3 startPosition;
    private float totalDistanceTraveled = 0f;
    private float currentMoveSpeed;
    private float bobTimer = 0f;
    private float originalY;
    
    // Collection state
    private bool isBeingMagneted = false;
    private Transform playerTransform;
    
    void Start()
    {
        InitializeCoin();
    }
    
    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanPlay())
            return;
            
        HandleMovement();
        HandleAnimation();
        CheckMagnetEffect();
        CheckIfShouldDestroy();
    }
    
    void InitializeCoin()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        coinCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Store start position
        startPosition = transform.position;
        originalY = transform.position.y;
        totalDistanceTraveled = 0f;
        bobTimer = Random.Range(0f, 2f * Mathf.PI); // Random start phase
        
        // Calculate movement speed
        currentMoveSpeed = CalculateMovementSpeed();
        
        // Setup collider as trigger
        if (coinCollider != null)
        {
            coinCollider.isTrigger = true;
        }
        
        // Find player
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[COIN] Initialized at {transform.position} with speed {currentMoveSpeed}");
        }
    }
    
    void HandleMovement()
    {
        if (isBeingMagneted && playerTransform != null)
        {
            // Bay v? player
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, collectSpeed * Time.deltaTime);
        }
        else
        {
            // Di chuy?n bình th??ng sang trái
            float moveDistance = currentMoveSpeed * Time.deltaTime;
            transform.position += Vector3.left * moveDistance;
            totalDistanceTraveled += moveDistance;
        }
    }
    
    void HandleAnimation()
    {
        // Quay coin
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Nh?p nhô lên xu?ng
        if (!isBeingMagneted)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float yOffset = Mathf.Sin(bobTimer) * bobAmplitude;
            
            Vector3 pos = transform.position;
            pos.y = originalY + yOffset;
            transform.position = pos;
        }
    }
    
    void CheckMagnetEffect()
    {
        if (playerTransform == null) return;
        
        // Ki?m tra player có magnet effect không
        PlayerController player = playerTransform.GetComponent<PlayerController>();
        if (player != null && player.playerData != null && player.playerData.hasMagnetEffect)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= magnetRange && !isBeingMagneted)
            {
                isBeingMagneted = true;
                if (showDebugInfo)
                {
                    Debug.Log($"[COIN] Being magneted by player at distance {distanceToPlayer:F1}");
                }
            }
        }
    }
    
    void CheckIfShouldDestroy()
    {
        if (totalDistanceTraveled >= destroyDistance)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[COIN] Destroyed after traveling {totalDistanceTraveled:F1} units");
            }
            Destroy(gameObject);
        }
    }
    
    float CalculateMovementSpeed()
    {
        float baseSpeed = moveSpeed;
        
        if (!syncWithGameSpeed)
            return baseSpeed;
        
        // ??ng b? v?i t?c ?? parallax
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
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }
    
    void CollectCoin()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[COIN] Collected! Value: {coinValue}");
        }
        
        // Add coin to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin(coinValue);
        }
        
        // Play collect sound
        if (audioSource != null)
        {
            audioSource.Play();
        }
        
        // Destroy coin
        Destroy(gameObject);
    }
    
    // Public methods
    public void SetCoinValue(int value)
    {
        coinValue = value;
    }
    
    public void ForceDestroy()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[COIN] Force destroyed at distance {totalDistanceTraveled:F1}");
        }
        Destroy(gameObject);
    }
    
    // Gizmos ?? debug
    void OnDrawGizmosSelected()
    {
        // Draw coin bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw magnet range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
        
        // Draw movement direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.left * 2f);
        
        // Draw traveled distance visualization
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPosition, transform.position);
            
            // Draw destroy point
            Gizmos.color = Color.red;
            Vector3 destroyPoint = startPosition + Vector3.left * destroyDistance;
            Gizmos.DrawWireCube(destroyPoint, Vector3.one * 0.3f);
        }
    }
}