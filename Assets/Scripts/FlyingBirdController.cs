using UnityEngine;

public class FlyingBirdController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public bool syncWithGameSpeed = true;
    
    [Header("Flying Pattern")]
    public float flyingAmplitude = 2f; // Biên ?? bay lên xu?ng
    public float flyingFrequency = 1.5f; // T?n s? bay
    public AnimationCurve flyingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curve cho smooth flying
    
    [Header("Animation")]
    public float flapSpeed = 2f; // T?c ?? v? cánh
    
    [Header("Destroy Settings")]
    public float destroyDistance = 25f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D birdCollider;
    
    // Movement variables
    private Vector3 startPosition;
    private float totalDistanceTraveled = 0f;
    private float currentMoveSpeed;
    private float flyingTimer = 0f;
    private float originalY;
    
    void Start()
    {
        InitializeBird();
    }
    
    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanPlay())
            return;
            
        HandleMovement();
        HandleFlyingPattern();
        HandleAnimation();
        CheckIfShouldDestroy();
    }
    
    void InitializeBird()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        birdCollider = GetComponent<Collider2D>();
        
        // Store start position
        startPosition = transform.position;
        originalY = transform.position.y;
        totalDistanceTraveled = 0f;
        flyingTimer = Random.Range(0f, 2f * Mathf.PI); // Random start phase
        
        // Calculate movement speed
        currentMoveSpeed = CalculateMovementSpeed();
        
        // Setup collider as trigger
        if (birdCollider != null)
        {
            birdCollider.isTrigger = true;
        }
        
        // Setup animation
        if (animator != null)
        {
            animator.SetFloat("FlapSpeed", flapSpeed);
            animator.SetBool("IsFlying", true);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[BIRD] Initialized at {transform.position} with speed {currentMoveSpeed}");
        }
    }
    
    void HandleMovement()
    {
        // Di chuy?n sang trái
        float moveDistance = currentMoveSpeed * Time.deltaTime;
        transform.position += Vector3.left * moveDistance;
        totalDistanceTraveled += moveDistance;
    }
    
    void HandleFlyingPattern()
    {
        // Bay lên xu?ng theo sine wave v?i curve
        flyingTimer += Time.deltaTime * flyingFrequency;
        
        // S? d?ng curve ?? t?o chuy?n ??ng smooth h?n
        float normalizedTime = (Mathf.Sin(flyingTimer) + 1f) * 0.5f; // Normalize to 0-1
        float curveValue = flyingCurve.Evaluate(normalizedTime);
        float yOffset = (curveValue - 0.5f) * 2f * flyingAmplitude; // Convert back to -amplitude to +amplitude
        
        Vector3 pos = transform.position;
        pos.y = originalY + yOffset;
        transform.position = pos;
    }
    
    void HandleAnimation()
    {
        if (animator != null)
        {
            // ?i?u ch?nh t?c ?? v? cánh d?a trên t?c ?? di chuy?n
            float normalizedSpeed = currentMoveSpeed / moveSpeed;
            animator.SetFloat("FlapSpeed", flapSpeed * normalizedSpeed);
            
            // ?i?u ch?nh h??ng bay (flip sprite n?u c?n)
            // Có th? thêm logic ?? bird bay theo h??ng khác nhau
        }
    }
    
    void CheckIfShouldDestroy()
    {
        if (totalDistanceTraveled >= destroyDistance)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[BIRD] Destroyed after traveling {totalDistanceTraveled:F1} units");
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
        if (showDebugInfo)
        {
            Debug.Log($"[BIRD] Collided with {other.name} (Tag: {other.tag})");
        }
        
        if (other.CompareTag("Player"))
        {
            // Gây damage cho player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[BIRD] Damaged player!");
                }
            }
            
            // Bird có th? ???c destroy sau khi va ch?m ho?c không
            // Tùy thu?c vào game design
            // Destroy(gameObject);
        }
    }
    
    // Public methods
    public void SetFlyingPattern(float amplitude, float frequency)
    {
        flyingAmplitude = amplitude;
        flyingFrequency = frequency;
    }
    
    public void ForceDestroy()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[BIRD] Force destroyed at distance {totalDistanceTraveled:F1}");
        }
        Destroy(gameObject);
    }
    
    // Gizmos ?? debug
    void OnDrawGizmosSelected()
    {
        // Draw bird bounds
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw movement direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.left * 2f);
        
        // Draw flying path
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 pos = transform.position;
            Gizmos.DrawLine(
                new Vector3(pos.x, originalY - flyingAmplitude, pos.z),
                new Vector3(pos.x, originalY + flyingAmplitude, pos.z)
            );
            
            // Draw traveled distance
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPosition, transform.position);
            
            // Draw destroy point
            Gizmos.color = Color.red;
            Vector3 destroyPoint = startPosition + Vector3.left * destroyDistance;
            Gizmos.DrawWireCube(destroyPoint, Vector3.one * 0.3f);
        }
    }
}