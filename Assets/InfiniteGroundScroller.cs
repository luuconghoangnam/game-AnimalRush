using UnityEngine;

public class InfiniteGroundScroller : MonoBehaviour
{
    [Header("Ground Settings")]
    public int groundCount = 3; // Số lượng sprite cần tạo
    public float moveSpeed = 5f; // Tốc độ di chuyển (nếu không dùng GameManager)
    public bool useGameManagerSpeed = true; // Sử dụng tốc độ từ GameManager

    [Header("Parallax Effect")]
    [Range(0.1f, 1.0f)]
    public float parallaxEffect = 1.0f; // 1.0 = tốc độ đầy đủ, 0.5 = nửa tốc độ, v.v.

    [Header("Debug")]
    public bool showGizmos = true;
    public bool forceMove = true; // Luôn di chuyển dù game đang ở trạng thái nào

    private GameObject[] groundClones;
    private float spriteWidth;
    private Transform cameraTransform;
    private float screenLeftEdge;
    private float screenRightEdge;
    private SpriteRenderer originalRenderer;
    private BoxCollider2D originalCollider;

    void Start()
    {
        // Lấy tham chiếu đến camera
        cameraTransform = Camera.main.transform;

        // Lấy SpriteRenderer và BoxCollider2D từ GameObject hiện tại
        originalRenderer = GetComponent<SpriteRenderer>();
        originalCollider = GetComponent<BoxCollider2D>();

        if (originalRenderer == null)
        {
            Debug.LogError("InfiniteGroundScroller: Không tìm thấy SpriteRenderer trên GameObject này!");
            return;
        }

        // Tính kích thước sprite
        spriteWidth = originalRenderer.bounds.size.x;

        // Tạo các ground clones
        CreateGroundClones();
    }

    void CreateGroundClones()
    {
        groundClones = new GameObject[groundCount];

        // GameObject đầu tiên chính là game object hiện tại
        groundClones[0] = gameObject;

        // Tính toán vị trí bắt đầu để bao phủ toàn bộ màn hình và hơn
        float startX = cameraTransform.position.x - Camera.main.orthographicSize * Camera.main.aspect - spriteWidth;

        // Đặt vị trí cho GameObject đầu tiên
        transform.position = new Vector3(startX, transform.position.y, transform.position.z);

        // Tạo các clone khác
        for (int i = 1; i < groundCount; i++)
        {
            // Tạo clone
            GameObject clone = Instantiate(gameObject, transform.parent);
            clone.name = "GroundClone_" + i;

            // Đặt vị trí
            clone.transform.position = new Vector3(startX + (i * spriteWidth), transform.position.y, transform.position.z);

            // Xóa script InfiniteGroundScroller trên clone
            InfiniteGroundScroller script = clone.GetComponent<InfiniteGroundScroller>();
            if (script != null && script != this)
            {
                Destroy(script);
            }

            // Thêm vào mảng
            groundClones[i] = clone;
        }

        Debug.Log("Đã tạo " + groundCount + " ground clones với chiều rộng mỗi sprite = " + spriteWidth);
    }

    void Update()
    {
        // Cập nhật tốc độ cơ bản
        float baseSpeed = 0;

        if (useGameManagerSpeed && GameManager.Instance != null)
        {
            baseSpeed = GameManager.Instance.currentGameSpeed;
            // Debug để kiểm tra tốc độ từ GameManager
            if (Time.frameCount % 60 == 0) // Mỗi giây log một lần
                Debug.Log("Game speed: " + GameManager.Instance.currentGameSpeed);
        }
        else
        {
            baseSpeed = moveSpeed;
        }

        // Áp dụng hiệu ứng parallax
        float effectiveSpeed = baseSpeed * parallaxEffect;

        // Cập nhật các biên của màn hình
        screenLeftEdge = cameraTransform.position.x - Camera.main.orthographicSize * Camera.main.aspect;
        screenRightEdge = cameraTransform.position.x + Camera.main.orthographicSize * Camera.main.aspect;

        // Kiểm tra điều kiện di chuyển
        bool shouldMove = forceMove ||
                          GameManager.Instance == null ||
                          GameManager.Instance.currentState == GameManager.GameState.Playing;

        if (shouldMove)
        {
            // Di chuyển tất cả mặt đất
            for (int i = 0; i < groundClones.Length; i++)
            {
                if (groundClones[i] == null) continue;

                // Di chuyển sang trái với tốc độ dựa trên hiệu ứng parallax
                groundClones[i].transform.position -= new Vector3(effectiveSpeed * Time.deltaTime, 0, 0);

                // Kiểm tra xem mặt đất đã ra khỏi màn hình bên trái chưa
                if (groundClones[i].transform.position.x + spriteWidth < screenLeftEdge)
                {
                    // Tìm ground ở xa nhất bên phải
                    float rightmostX = FindRightmostGroundX();

                    // Đặt ground này sang bên phải, nối tiếp với ground xa nhất hiện tại
                    groundClones[i].transform.position = new Vector3(
                        rightmostX + spriteWidth - 0.01f, // -0.01f để tránh khoảng hở nhỏ giữa các sprite
                        groundClones[i].transform.position.y,
                        groundClones[i].transform.position.z);

                    Debug.Log("Di chuyển ground clone " + i + " sang vị trí x = " +
                             (rightmostX + spriteWidth - 0.01f));
                }
            }
        }
        else
        {
            Debug.Log("Ground không di chuyển vì không thỏa điều kiện di chuyển");
        }
    }

    // Tìm vị trí x của ground xa nhất bên phải
    float FindRightmostGroundX()
    {
        float rightmostX = float.MinValue;
        foreach (GameObject ground in groundClones)
        {
            if (ground == null) continue;

            if (ground.transform.position.x > rightmostX)
            {
                rightmostX = ground.transform.position.x;
            }
        }
        return rightmostX;
    }

    // Hiển thị các đường viền màn hình để debug
    void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying || cameraTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(screenLeftEdge, transform.position.y - 5, 0),
            new Vector3(screenLeftEdge, transform.position.y + 5, 0)
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(screenRightEdge, transform.position.y - 5, 0),
            new Vector3(screenRightEdge, transform.position.y + 5, 0)
        );
    }
}