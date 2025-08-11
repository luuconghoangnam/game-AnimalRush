using UnityEngine;
using TMPro;

public class SimpleLeaderboard : MonoBehaviour
{
    [Header("Simple Display")]
    public TextMeshProUGUI topScoresText;
    public TextMeshProUGUI personalBestText;

    void OnEnable()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        // Hi?n th? top 5 scores ??n gi?n
        int[] topScores = {
            PlayerPrefs.GetInt("Score1", 0),
            PlayerPrefs.GetInt("Score2", 0),
            PlayerPrefs.GetInt("Score3", 0),
            PlayerPrefs.GetInt("Score4", 0),
            PlayerPrefs.GetInt("Score5", 0)
        };

        string scoresText = "TOP SCORES:\n\n";
        for (int i = 0; i < topScores.Length; i++)
        {
            scoresText += $"{i + 1}. {topScores[i]} coins\n";
        }

        if (topScoresText != null)
            topScoresText.text = scoresText;

        // Personal best
        if (personalBestText != null)
            personalBestText.text = $"Your Best: {PlayerPrefs.GetInt("HighCoins", 0)} coins";
    }

    public static void SaveScore(int newScore)
    {
        // Simple score saving
        int[] scores = new int[5];
        for (int i = 0; i < 5; i++)
        {
            scores[i] = PlayerPrefs.GetInt($"Score{i + 1}", 0);
        }

        // Thêm score m?i
        scores[4] = newScore;
        System.Array.Sort(scores);
        System.Array.Reverse(scores);

        // Save l?i
        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetInt($"Score{i + 1}", scores[i]);
        }
        PlayerPrefs.Save();
    }
}