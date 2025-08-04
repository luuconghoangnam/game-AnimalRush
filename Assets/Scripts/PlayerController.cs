using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Setup")]
    public PlayerData playerData;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Rigidbody2D rb;
    public Collider2D playerCollider;
    public AudioSource audioSource;

    [Header("Base Animation Controller")]
    public RuntimeAnimatorController baseAnimatorController; // Animator Controller chung
    private AnimatorOverrideController overrideController;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask;

    [Header("Input Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    [Header("Jump Settings")]
    public float jumpHeight = 10f; // Độ cao nhảy có thể điều chỉnh

    [Header("Ground Flow Settings")]
    public bool enableGroundFlow = true; // Bật/tắt hiệu ứng bị cuốn theo ground
    public float baseGroundFlowSpeed = 2f; // Tốc độ bị cuốn cơ bản
    public bool syncWithParallax = true; // Đồng bộ với tốc độ parallax

    [Header("Dynamic Speed Settings")]
    public bool enableDynamicSpeed = true; // Bật/tắt tăng tốc player theo thời gian
    public float speedMultiplierRate = 1.0f; // Tỷ lệ tăng speed so với parallax (1.0 = cùng tỷ lệ)
    public float maxSpeedMultiplier = 3f; // Tối đa player speed có thể tăng

    [Header("Debug")]
    public bool showSpeedDebug = false;

    // Private variables
    private bool isGrounded;
    private bool canDoubleJump;
    private bool isAlive = true;
    private float currentMoveSpeed;
    private float baseMoveSpeed; // Lưu moveSpeed gốc từ playerData
    private bool hasShieldActive = false;
    private bool facingRight = true;
    private float horizontalInput;

    // Animation hash IDs
    private int runHash;
    private int jumpHash;
    private int idleHash;
    private int dieHash;

    void Start()
    {
        InitializePlayer();
        CacheAnimationHashes();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanPlay() || !isAlive) return;

        UpdateDynamicSpeed();
        HandleInput();
        CheckGrounded();
        HandleAnimation();
        HandleSpecialAbilities();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanPlay() || !isAlive) return;

        HandleMovement();
    }

    void InitializePlayer()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData is missing!");
            return;
        }

        // Setup sprite
        if (spriteRenderer != null && playerData.playerSprite != null)
            spriteRenderer.sprite = playerData.playerSprite;

        // Setup animator với override controller
        SetupAnimator();

        // Setup stats
        baseMoveSpeed = playerData.moveSpeed; // Lưu speed gốc
        currentMoveSpeed = baseMoveSpeed;

        // Reset abilities
        canDoubleJump = playerData.hasDoubleJump;
        hasShieldActive = playerData.hasShield;

        // Reset facing direction
        facingRight = true;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // Spawn special effect nếu có
        if (playerData.specialEffect != null)
        {
            GameObject effect = Instantiate(playerData.specialEffect, transform);
        }

        if (showSpeedDebug)
        {
            Debug.Log($"[PLAYER] Initialized - Base Speed: {baseMoveSpeed}, Current Speed: {currentMoveSpeed}");
        }
    }

    void UpdateDynamicSpeed()
    {
        if (!enableDynamicSpeed) return;

        // Lấy speed multiplier từ ParallaxController
        ParallaxController parallaxController = FindFirstObjectByType<ParallaxController>();
        if (parallaxController != null)
        {
            float parallaxMultiplier = parallaxController.GetCurrentSpeedMultiplier();

            // Tính player speed multiplier dựa trên parallax
            float playerSpeedMultiplier = 1f + ((parallaxMultiplier - 1f) * speedMultiplierRate);
            playerSpeedMultiplier = Mathf.Min(playerSpeedMultiplier, maxSpeedMultiplier);

            // Cập nhật current move speed
            float newMoveSpeed = baseMoveSpeed * playerSpeedMultiplier;

            // Chỉ update nếu có thay đổi đáng kể
            if (Mathf.Abs(currentMoveSpeed - newMoveSpeed) > 0.01f)
            {
                currentMoveSpeed = newMoveSpeed;

                if (showSpeedDebug && Time.frameCount % 60 == 0) // Log mỗi giây
                {
                    Debug.Log($"[PLAYER] Speed updated - Parallax: {parallaxMultiplier:F2}x, Player: {playerSpeedMultiplier:F2}x, Speed: {currentMoveSpeed:F1}");
                }
            }
        }
    }

    void SetupAnimator()
    {
        if (animator == null || baseAnimatorController == null)
            return;

        // Tạo AnimatorOverrideController từ base controller
        overrideController = new AnimatorOverrideController(baseAnimatorController);

        // Override animations nếu có
        if (playerData.idleAnimation != null)
            overrideController["PlayerIdle"] = playerData.idleAnimation;

        if (playerData.runAnimation != null)
            overrideController["PlayerRun"] = playerData.runAnimation;

        if (playerData.jumpAnimation != null)
            overrideController["PlayerJump"] = playerData.jumpAnimation;

        if (playerData.dieAnimation != null)
            overrideController["PlayerDie"] = playerData.dieAnimation;

        // Áp dụng override controller
        animator.runtimeAnimatorController = overrideController;
    }

    void CacheAnimationHashes()
    {
        runHash = Animator.StringToHash("PlayerRun");
        jumpHash = Animator.StringToHash("PlayerJump");
        idleHash = Animator.StringToHash("PlayerIdle");
        dieHash = Animator.StringToHash("PlayerDie");
    }

    void HandleInput()
    {
        // Movement input
        horizontalInput = 0f;

        if (Input.GetKey(leftKey) || Input.GetKey(KeyCode.LeftArrow))
            horizontalInput = -1f;
        else if (Input.GetKey(rightKey) || Input.GetKey(KeyCode.RightArrow))
            horizontalInput = 1f;

        // Jump input
        if (Input.GetKeyDown(jumpKey))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (playerData.hasDoubleJump && canDoubleJump)
            {
                Jump();
                canDoubleJump = false;
            }
        }

        // Handle sprite flipping
        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            Flip();
        }
    }

    void HandleMovement()
    {
        // Horizontal movement based on input với current speed (đã tăng theo thời gian)
        float moveVelocity = horizontalInput * currentMoveSpeed;

        // Thêm ground flow effect (bị cuốn theo ground)
        if (enableGroundFlow && isGrounded)
        {
            float currentGroundFlowSpeed = GetCurrentGroundFlowSpeed();
            moveVelocity -= currentGroundFlowSpeed; // Bị đẩy về bên trái
        }

        rb.linearVelocity = new Vector2(moveVelocity, rb.linearVelocity.y);

        // Debug speed info
        if (showSpeedDebug && Time.frameCount % 120 == 0) // Log mỗi 2 giây
        {
            float groundFlow = enableGroundFlow && isGrounded ? GetCurrentGroundFlowSpeed() : 0f;
            Debug.Log($"[PLAYER] Movement - Input: {horizontalInput:F1}, Speed: {currentMoveSpeed:F1}, GroundFlow: {groundFlow:F1}, Final Velocity: {moveVelocity:F1}");
        }
    }

    void Jump()
    {
        // Sử dụng jumpHeight từ Inspector, fallback về playerData nếu cần
        float jumpForce = jumpHeight > 0 ? jumpHeight : playerData.jumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        // Play jump sound
        if (audioSource != null && playerData.specialSound != null)
            audioSource.PlayOneShot(playerData.specialSound);
    }

    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        // Reset double jump khi chạm đất
        if (isGrounded && !wasGrounded)
        {
            canDoubleJump = playerData.hasDoubleJump;
        }
    }

    void HandleAnimation()
    {
        if (animator == null) return;

        if (!isGrounded)
        {
            animator.Play(jumpHash);
        }
        else if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            animator.Play(runHash);
        }
        else
        {
            animator.Play(idleHash);
        }
    }

    void HandleSpecialAbilities()
    {
        // Speed boost
        if (playerData.hasSpeedBoost)
        {
            // Logic speed boost có thể thêm sau
        }

        // Magnet effect
        if (playerData.hasMagnetEffect)
        {
            AttractCoins();
        }
    }

    void AttractCoins()
    {
        // Tìm tất cả coin trong bán kính
        Collider2D[] coins = Physics2D.OverlapCircleAll(transform.position, 3f);

        foreach (var coin in coins)
        {
            if (coin.CompareTag("Coin"))
            {
                // Di chuyển coin về phía player
                Vector2 direction = (transform.position - coin.transform.position).normalized;
                coin.transform.position = Vector2.MoveTowards(coin.transform.position, transform.position, 5f * Time.deltaTime);
            }
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (!isAlive) return;

        // Trừ mạng
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();

            // Kiểm tra còn mạng không
            if (GameManager.Instance.GetLives() <= 0)
            {
                Die(); // Chết khi hết mạng
            }
            else
            {
                // Còn mạng thì respawn
                StartCoroutine(RespawnAfterDamage());
            }
        }
    }

    public void Die()
    {
        if (!isAlive) return;

        isAlive = false;

        if (animator != null)
            animator.Play(dieHash);

        // Game over
        Debug.Log("Player died - Game Over!");
    }

    System.Collections.IEnumerator RespawnAfterDamage()
    {
        // Tạm thời vô địch
        isAlive = false;

        // Hiệu ứng nhấp nháy (optional)
        StartCoroutine(BlinkEffect());

        // Chờ 1 giây
        yield return new WaitForSeconds(1f);

        // Respawn
        isAlive = true;

        Debug.Log($"Player respawned! Lives remaining: {GameManager.Instance.GetLives()}");
    }

    System.Collections.IEnumerator BlinkEffect()
    {
        // Nhấp nháy 3 lần
        for (int i = 0; i < 6; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1, 1, 1, i % 2 == 0 ? 0.5f : 1f);
            yield return new WaitForSeconds(0.2f);
        }

        // Khôi phục màu bình thường
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    void Respawn()
    {
        if (GameManager.Instance != null && GameManager.Instance.GetLives() > 0)
        {
            isAlive = true;
            transform.position = Vector3.zero; // Hoặc spawn point
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;

        switch (other.tag)
        {
            case "Coin":
                CollectCoin(other.gameObject);
                break;

            case "Enemy":
                if (!hasShieldActive)
                    TakeDamage(1);
                break;

            case "LifeItem":
                CollectLife(other.gameObject);
                break;

            case "Obstacle":
                if (!hasShieldActive)
                    TakeDamage(1);
                break;
        }
    }

    void CollectCoin(GameObject coin)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoin();
        Destroy(coin);

        // Play coin sound nếu có
        // AudioManager có thể thêm sau
    }

    void CollectLife(GameObject lifeItem)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddLife();
        Destroy(lifeItem);
    }

    // Public methods để các script khác có thể gọi
    public void ActivateShield(float duration)
    {
        if (playerData.hasShield)
        {
            hasShieldActive = true;
            Invoke(nameof(DeactivateShield), duration);
        }
    }

    void DeactivateShield()
    {
        hasShieldActive = false;
    }

    public void SpeedBoost(float multiplier, float duration)
    {
        if (playerData.hasSpeedBoost)
        {
            // Tạm thời tắt dynamic speed để áp dụng speed boost
            bool wasEnabled = enableDynamicSpeed;
            enableDynamicSpeed = false;

            currentMoveSpeed = baseMoveSpeed * multiplier;

            // Sau duration, khôi phục dynamic speed
            System.Action resetAction = () => {
                enableDynamicSpeed = wasEnabled;
                if (wasEnabled)
                {
                    UpdateDynamicSpeed(); // Cập nhật lại speed theo parallax
                }
                else
                {
                    currentMoveSpeed = baseMoveSpeed;
                }
            };

            Invoke(nameof(ResetSpeedBoost), duration);
        }
    }

    void ResetSpeedBoost()
    {
        enableDynamicSpeed = true;
        UpdateDynamicSpeed();
    }

    void ResetSpeed()
    {
        currentMoveSpeed = baseMoveSpeed;
    }

    void Flip()
    {
        // Đổi hướng facing
        facingRight = !facingRight;

        // Lật sprite bằng cách scale X
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    float GetCurrentGroundFlowSpeed()
    {
        if (!syncWithParallax)
            return baseGroundFlowSpeed;

        // Lấy speed multiplier từ ParallaxController nếu có
        ParallaxController parallaxController = FindFirstObjectByType<ParallaxController>();
        if (parallaxController != null)
        {
            float parallaxMultiplier = parallaxController.GetCurrentSpeedMultiplier();
            return baseGroundFlowSpeed * parallaxMultiplier;
        }

        return baseGroundFlowSpeed;
    }

    // Public methods để get speed info
    public float GetCurrentMoveSpeed()
    {
        return currentMoveSpeed;
    }

    public float GetBaseMoveSpeed()
    {
        return baseMoveSpeed;
    }

    public float GetSpeedMultiplier()
    {
        return currentMoveSpeed / baseMoveSpeed;
    }

    // Public method để force update speed (useful for debugging)
    public void ForceUpdateSpeed()
    {
        UpdateDynamicSpeed();
    }

    // Gizmos để debug
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (playerData != null && playerData.hasMagnetEffect)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 3f);
        }

        // Show speed info in scene view
        if (Application.isPlaying && enableDynamicSpeed)
        {
            Gizmos.color = Color.cyan;
            float speedRatio = currentMoveSpeed / baseMoveSpeed;
            Gizmos.DrawRay(transform.position, Vector3.right * speedRatio * 2f);
        }
    }
}