using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Character Display")]
    public Image characterImage;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterStatsText;
    public TextMeshProUGUI unlockCostText;

    [Header("Navigation")]
    public Button previousButton;
    public Button nextButton;
    public Button selectButton;
    public Button unlockButton;

    [Header("Abilities Display")]
    public GameObject[] abilityIcons;
    public Color unlockedColor = Color.green;
    public Color lockedColor = Color.gray;

    private int currentPlayerIndex = 0;
    private PlayerManager playerManager;

    void Start()
    {
        playerManager = PlayerManager.Instance;
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager not found!");
            return;
        }

        SetupButtons();
        currentPlayerIndex = playerManager.GetCurrentPlayerIndex();
        UpdateCharacterDisplay();
    }

    void OnEnable()
    {
        if (playerManager != null)
            UpdateCharacterDisplay();
    }

    void SetupButtons()
    {
        previousButton?.onClick.AddListener(PreviousCharacter);
        nextButton?.onClick.AddListener(NextCharacter);
        selectButton?.onClick.AddListener(SelectCharacter);
        unlockButton?.onClick.AddListener(UnlockCharacter);
    }

    void PreviousCharacter()
    {
        currentPlayerIndex--;
        if (currentPlayerIndex < 0)
            currentPlayerIndex = playerManager.GetPlayerCount() - 1;
        UpdateCharacterDisplay();
    }

    void NextCharacter()
    {
        currentPlayerIndex++;
        if (currentPlayerIndex >= playerManager.GetPlayerCount())
            currentPlayerIndex = 0;
        UpdateCharacterDisplay();
    }

    void UpdateCharacterDisplay()
    {
        PlayerData player = playerManager.GetPlayer(currentPlayerIndex);
        if (player == null) return;

        // Update basic info
        characterImage.sprite = player.playerSprite;
        characterNameText.text = player.playerName;
        characterStatsText.text = $"Speed: {player.moveSpeed}\nJump: {player.jumpForce}";

        // Update abilities
        UpdateAbilityDisplay(player);

        // Update unlock status
        bool isUnlocked = playerManager.IsPlayerUnlocked(currentPlayerIndex);
        bool canUnlock = playerManager.CanUnlockPlayer(currentPlayerIndex);

        if (isUnlocked)
        {
            selectButton.gameObject.SetActive(true);
            unlockButton.gameObject.SetActive(false);
            unlockCostText.text = "UNLOCKED";
            unlockCostText.color = unlockedColor;
        }
        else
        {
            selectButton.gameObject.SetActive(false);
            unlockButton.gameObject.SetActive(true);
            unlockButton.interactable = canUnlock;

            unlockCostText.text = $"Cost: {player.unlockCost} coins";
            unlockCostText.color = canUnlock ? unlockedColor : lockedColor;
        }
    }

    void UpdateAbilityDisplay(PlayerData player)
    {
        foreach (var icon in abilityIcons)
            icon.SetActive(false);

        int iconIndex = 0;

        if (player.hasDoubleJump && iconIndex < abilityIcons.Length)
        {
            abilityIcons[iconIndex].SetActive(true);
            iconIndex++;
        }

        if (player.hasSpeedBoost && iconIndex < abilityIcons.Length)
        {
            abilityIcons[iconIndex].SetActive(true);
            iconIndex++;
        }

        if (player.hasMagnetEffect && iconIndex < abilityIcons.Length)
        {
            abilityIcons[iconIndex].SetActive(true);
            iconIndex++;
        }

        if (player.hasShield && iconIndex < abilityIcons.Length)
        {
            abilityIcons[iconIndex].SetActive(true);
            iconIndex++;
        }
    }

    void SelectCharacter()
    {
        playerManager.SelectPlayer(currentPlayerIndex);
        Debug.Log($"Selected character: {playerManager.GetCurrentPlayer().playerName}");
    }

    void UnlockCharacter()
    {
        if (playerManager.UnlockPlayer(currentPlayerIndex))
        {
            UpdateCharacterDisplay();
            // Update total coins display in main menu
            MainMenuManager mainMenu = FindFirstObjectByType<MainMenuManager>();
            mainMenu?.UpdateDisplays();
        }
    }
}