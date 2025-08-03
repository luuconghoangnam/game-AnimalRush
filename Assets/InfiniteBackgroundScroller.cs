using UnityEngine;
using System.Collections.Generic;

public class InfiniteBackgroundScroller : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundLayer
    {
        public string layerName = "Layer";
        public Sprite sprite;
        public float parallaxEffect = 0.5f;
        public string sortingLayerName = "Background";
        public int sortingOrder = 0;
        public float yOffset = 0f;
        public int spriteCount = 3;

        [HideInInspector]
        public GameObject layerContainer;
        [HideInInspector]
        public List<GameObject> backgrounds = new List<GameObject>();
        [HideInInspector]
        public float spriteWidth;
    }

    [Header("Layer Settings")]
    public List<BackgroundLayer> backgroundLayers = new List<BackgroundLayer>();

    [Header("General Settings")]
    public bool useGameSpeed = true;

    private Transform cameraTransform;
    private float lastCameraX;

    void Start()
    {
        // Lấy tham chiếu đến camera
        cameraTransform = Camera.main.transform;
        lastCameraX = cameraTransform.position.x;

        // Tạo background cho từng layer
        foreach (BackgroundLayer layer in backgroundLayers)
        {
            CreateBackgroundsForLayer(layer);
        }
    }

    void CreateBackgroundsForLayer(BackgroundLayer layer)
    {
        // Tạo container cho layer này nếu chưa có
        if (layer.layerContainer == null)
        {
            layer.layerContainer = new GameObject(layer.layerName);
            layer.layerContainer.transform.parent = transform;
        }

        // Xóa backgrounds cũ trong layer này nếu có
        foreach (Transform child in layer.layerContainer.transform)
        {
            Destroy(child.gameObject);
        }
        layer.backgrounds.Clear();

        // Tính toán kích thước sprite
        SpriteRenderer sr = new GameObject("TempSprite").AddComponent<SpriteRenderer>();
        sr.sprite = layer.sprite;
        layer.spriteWidth = sr.bounds.size.x;
        Destroy(sr.gameObject);

        // Tạo các đối tượng background cho layer này
        for (int i = 0; i < layer.spriteCount; i++)
        {
            // Tạo gameobject cho background
            GameObject bg = new GameObject($"{layer.layerName}_BG_{i}");
            bg.transform.parent = layer.layerContainer.transform;

            // Thêm SpriteRenderer và gán sprite
            SpriteRenderer renderer = bg.AddComponent<SpriteRenderer>();
            renderer.sprite = layer.sprite;

            // Thiết lập vị trí
            float startX = cameraTransform.position.x - Camera.main.orthographicSize * Camera.main.aspect - layer.spriteWidth;
            bg.transform.position = new Vector3(startX + (i * layer.spriteWidth), layer.yOffset, 0);

            // Thiết lập sorting
            renderer.sortingLayerName = layer.sortingLayerName;
            renderer.sortingOrder = layer.sortingOrder;

            // Thêm vào danh sách để quản lý
            layer.backgrounds.Add(bg);
        }
    }

    void Update()
    {
        // Xử lý từng layer riêng biệt
        foreach (BackgroundLayer layer in backgroundLayers)
        {
            float moveSpeed = 0;

            if (useGameSpeed && GameManager.Instance != null)
            {
                // Sử dụng tốc độ từ GameManager
                moveSpeed = GameManager.Instance.currentGameSpeed * layer.parallaxEffect;
            }
            else
            {
                // Dựa vào chuyển động của camera
                float deltaX = cameraTransform.position.x - lastCameraX;
                moveSpeed = deltaX * layer.parallaxEffect * 60; // Nhân với 60 để độc lập với framerate
            }

            // Tính các giới hạn màn hình
            float screenLeftEdge = cameraTransform.position.x - Camera.main.orthographicSize * Camera.main.aspect;

            // Di chuyển tất cả backgrounds trong layer này
            for (int i = 0; i < layer.backgrounds.Count; i++)
            {
                GameObject bg = layer.backgrounds[i];

                // Di chuyển background sang trái
                bg.transform.position -= new Vector3(moveSpeed * Time.deltaTime, 0, 0);

                // Kiểm tra xem background đã ra khỏi màn hình chưa
                if (bg.transform.position.x + layer.spriteWidth < screenLeftEdge)
                {
                    // Tìm background ở xa nhất bên phải
                    float rightmostX = FindRightmostBackgroundX(layer);

                    // Đặt background này phía sau background xa nhất
                    bg.transform.position = new Vector3(rightmostX + layer.spriteWidth, layer.yOffset, 0);
                }
            }
        }

        lastCameraX = cameraTransform.position.x;
    }

    float FindRightmostBackgroundX(BackgroundLayer layer)
    {
        float rightmostX = float.MinValue;
        foreach (GameObject bg in layer.backgrounds)
        {
            if (bg.transform.position.x > rightmostX)
            {
                rightmostX = bg.transform.position.x;
            }
        }
        return rightmostX;
    }
}