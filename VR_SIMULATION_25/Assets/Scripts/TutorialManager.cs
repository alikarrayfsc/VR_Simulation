using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI instructionText;
    public Button nextButton;
    public Button startGameButton;

    [TextArea(2, 5)]
    public string[] instructions;

    private int currentIndex = 0;

    // Event to notify game has started
    public static event Action OnGameStarted;

    void Start()
    {
        if (instructions.Length == 0)
        {
            Debug.LogWarning("No instructions provided.");
            return;
        }

        tutorialPanel.SetActive(true);
        nextButton.onClick.AddListener(NextInstruction);
        startGameButton.onClick.AddListener(HandleStartGame);

        ShowInstruction(0);
    }

    void ShowInstruction(int index)
    {
        instructionText.text = instructions[index];
        nextButton.gameObject.SetActive(index < instructions.Length - 1);
        startGameButton.gameObject.SetActive(index == instructions.Length - 1);
    }

    void NextInstruction()
    {
        currentIndex++;
        if (currentIndex >= instructions.Length)
            currentIndex = instructions.Length - 1;

        ShowInstruction(currentIndex);
    }

    void HandleStartGame()
    {
        tutorialPanel.SetActive(false);
        GameManager.Instance.StartGame();      // Calls the GameManager's method
        OnGameStarted?.Invoke();               // Notify listeners like RobotController
    }
}
