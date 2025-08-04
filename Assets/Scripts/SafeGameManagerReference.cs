using UnityEngine;

/// <summary>
/// Utility class để đảm bảo GameManager được khởi tạo an toàn
/// </summary>
public static class SafeGameManagerReference
{
    public static bool CanPlay()
    {
        return GameManager.Instance != null && GameManager.Instance.CanPlay();
    }
    
    public static bool IsGameManagerReady()
    {
        return GameManager.Instance != null;
    }
    
    public static void SafeAddCoin()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoin();
    }
    
    public static void SafeLoseLife()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoseLife();
    }
    
    public static void SafeAddLife()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddLife();
    }
    
    public static int GetLives()
    {
        return GameManager.Instance != null ? GameManager.Instance.GetLives() : 0;
    }
} 