using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float jumpForce = 12f;
    public LayerMask groundLayer;

    [Header("Character Settings")]
    public CharacterData characterData;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isGrounded = false;
    private bool canDoubleJump = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Nếu không có component, thêm chúng vào
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider2D>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();

        // Thiết lập rigidbody
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        // Áp dụng các thông số từ CharacterData nếu có
        ApplyCharacterData();
    }

    void Update()
    {
        CheckGrounded();

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) &&
           (isGrounded || (characterData != null && characterData.canDoubleJump && canDoubleJump)))
        {
            Jump();
        }

        UpdateAnimation();
    }

    public void ApplyCharacterData()
    {
        if (characterData == null) return;

        // Áp dụng các thuộc tính từ CharacterData
        jumpForce = characterData.jumpForce;

        if (spriteRenderer != null && characterData.characterSprite != null)
            spriteRenderer.sprite = characterData.characterSprite;

        if (animator != null && characterData.animatorController != null)
            animator.runtimeAnimatorController = characterData.animatorController;
    }

    void CheckGrounded()
    {
        if (boxCollider == null) return;

        float extraHeight = 0.1f;
        isGrounded = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f,
            Vector2.down,
            extraHeight,
            groundLayer
        );

        // Reset double jump khi chạm đất
        if (isGrounded)
            canDoubleJump = true;
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
        else if (canDoubleJump && characterData != null && characterData.canDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.8f);
            canDoubleJump = false;
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("SpeedY", rb.linearVelocity.y);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            int coinValue = 1;

            // Áp dụng hệ số nhân xu nếu có
            if (characterData != null && characterData.coinMultiplier > 1)
                coinValue = Mathf.RoundToInt(coinValue * characterData.coinMultiplier);

            if (GameManager.Instance != null)
                GameManager.Instance.AddCoins(coinValue);

            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Obstacle") || other.CompareTag("Enemy"))
        {
            // Xử lý va chạm với chướng ngại vật
            bool shouldDie = true;

            // Nếu nhân vật có shield, sử dụng shield thay vì chết
            if (characterData != null && characterData.hasShield)
            {
                // Logic xử lý shield
                shouldDie = false;
                // characterData.hasShield = false; // Đã dùng shield
            }

            if (shouldDie && GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }
}