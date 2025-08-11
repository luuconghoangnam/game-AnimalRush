using UnityEngine;
using TMPro;

public class SimpleCredits : MonoBehaviour
{
    [Header("Credits Text")]
    public TextMeshProUGUI creditsText;

    [TextArea(10, 20)]
    public string creditsContent = @"YOUR GAME NAME
Version 1.0

Created by: Your Name

Thanks for playing!

© 2024 Your Studio";

    void Start()
    {
        if (creditsText != null)
            creditsText.text = creditsContent;
    }
}