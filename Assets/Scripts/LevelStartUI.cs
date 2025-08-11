using UnityEngine;
using TMPro;
using System.Collections;

public class LevelStartUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject levelStartPanel;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI levelDescriptionText;
    public TextMeshProUGUI countdownText;
    
    [Header("Settings")]
    public int countdownFrom = 3;
    public float delayBeforeStart = 0.5f;
    
    void Start()
    {
        StartCoroutine(ShowStartSequence());
    }
    
    IEnumerator ShowStartSequence()
    {
        // Kh?i t?m d?ng game khi b?t ??u
        Time.timeScale = 0f;
        
        if (levelStartPanel != null)
            levelStartPanel.SetActive(true);
            
        // Set level info
        if (levelNameText != null)
            levelNameText.text = "LEVEL 1"; // Customize theo level
        
        if (levelDescriptionText != null)
            levelDescriptionText.text = "Collect coins, avoid obstacles!";
            
        // ??i m?t chút
        yield return new WaitForSecondsRealtime(delayBeforeStart);
        
        // ??m ng??c
        for (int i = countdownFrom; i > 0; i--)
        {
            if (countdownText != null)
                countdownText.text = i.ToString();
                
            yield return new WaitForSecondsRealtime(1f);
        }
        
        // "GO!"
        if (countdownText != null)
            countdownText.text = "GO!";
            
        yield return new WaitForSecondsRealtime(0.5f);
        
        // B?t ??u game
        Time.timeScale = 1f;
        
        if (levelStartPanel != null)
            levelStartPanel.SetActive(false);
    }
    
    // N?u mu?n skip countdown
    public void SkipCountdown()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        
        if (levelStartPanel != null)
            levelStartPanel.SetActive(false);
    }
}