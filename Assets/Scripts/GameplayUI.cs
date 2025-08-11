using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameplayUI : MonoBehaviour
{
    [Header("Coin Display")]
    public TextMeshProUGUI coinText;
    public GameObject coinChangeEffect;
    
    [Header("Lives Display")]
    public TextMeshProUGUI livesText;
    public Image[] heartIcons;
    
    [Header("Time/Score Display")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    
    [Header("Notifications")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;
    public float notificationDuration = 2f;
    
    [Header("Powerup Display")]
    public GameObject powerupPanel;
    public Image powerupIcon;
    public Slider powerupDurationSlider;
    
    private int lastCoinCount = 0;
    private Coroutine notificationCoroutine;
    
    void Start()
    {
        // Subscribe to events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged += UpdateCoinDisplay;
            GameManager.Instance.OnLivesChanged += UpdateLivesDisplay;
        }
        
        // Initial UI update
        UpdateAllUI();
        
        // Hide notification
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
            
        // Hide powerup display
        if (powerupPanel != null)
            powerupPanel.SetActive(false);
    }
    
    void Update()
    {
        // Update time display
        UpdateTimeDisplay();
    }
    
    void UpdateAllUI()
    {
        if (GameManager.Instance != null)
        {
            UpdateCoinDisplay(GameManager.Instance.GetCurrentCoins());
            UpdateLivesDisplay(GameManager.Instance.GetLives());
        }
    }
    
    void UpdateCoinDisplay(int coins)
    {
        if (coinText != null)
            coinText.text = coins.ToString();
            
        // Show coin change effect if coins increased
        if (coins > lastCoinCount && coinChangeEffect != null)
        {
            // Activate effect (could be animation, particle, etc)
            coinChangeEffect.SetActive(true);
            StartCoroutine(DisableAfterDelay(coinChangeEffect, 0.5f));
        }
        
        lastCoinCount = coins;
    }
    
    void UpdateLivesDisplay(int lives)
    {
        if (livesText != null)
            livesText.text = lives.ToString();
            
        // Update heart icons if available
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
                heartIcons[i].enabled = (i < lives);
        }
    }
    
    void UpdateTimeDisplay()
    {
        if (timeText != null && GameManager.Instance != null)
        {
            float gameTime = GameManager.Instance.GetGameTime();
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    public void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null)
            return;
            
        // Stop existing notification if any
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);
            
        // Start new notification
        notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message));
    }
    
    IEnumerator ShowNotificationCoroutine(string message)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        
        yield return new WaitForSeconds(notificationDuration);
        
        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }
    
    public void ShowPowerup(Sprite icon, float duration)
    {
        if (powerupPanel == null || powerupIcon == null || powerupDurationSlider == null)
            return;
            
        powerupIcon.sprite = icon;
        powerupPanel.SetActive(true);
        
        StartCoroutine(PowerupDurationCoroutine(duration));
    }
    
    IEnumerator PowerupDurationCoroutine(float duration)
    {
        float timeLeft = duration;
        
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            powerupDurationSlider.value = timeLeft / duration;
            yield return null;
        }
        
        powerupPanel.SetActive(false);
    }
    
    IEnumerator DisableAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= UpdateCoinDisplay;
            GameManager.Instance.OnLivesChanged -= UpdateLivesDisplay;
        }
    }
}