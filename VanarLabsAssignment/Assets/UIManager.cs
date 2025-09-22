using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI letterDisplayText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI messageText;
    public Button restartButton;
    public Button nextButton;
    public Slider progressSlider;

    [Header("Completion UI")]
    public GameObject completionPanel;
    public TextMeshProUGUI completionText;
    public ParticleSystem celebrationEffect;

    [Header("Game Complete UI")]
    public GameObject gameCompletePanel;
    public Button playAgainButton;
    public Button quitButton;

    private Coroutine messageCoroutine;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        InitializeUI();
        SetupButtons();
    }

    void Update()
    {
        UpdateProgressBar();
    }

    void InitializeUI()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);

        if (gameCompletePanel != null)
            gameCompletePanel.SetActive(false);

        ClearMessage();
    }

    void SetupButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(() => gameManager?.RestartCurrentLetter());

        if (nextButton != null)
            nextButton.onClick.AddListener(() => gameManager?.NextLetter());

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(() => gameManager?.RestartGame());

        if (quitButton != null)
            quitButton.onClick.AddListener(() => gameManager?.QuitGame());
    }

    public void UpdateLetterDisplay(string letter)
    {
        if (letterDisplayText != null)
        {
            letterDisplayText.text = $"Letter: {letter}";
            letterDisplayText.transform.localScale = Vector3.zero;
            LeanTween.scale(letterDisplayText.gameObject, Vector3.one, 0.5f)
                .setEase(LeanTweenType.easeOutBack);
        }
    }

    public void ShowInstructions()
    {
        if (instructionText != null)
        {
            instructionText.text = "Trace the letter starting from the green dot!";
            instructionText.color = Color.white;
        }

        ClearMessage();
    }

    public void ShowMessage(string message, float duration = 2.0f)
    {
        if (messageText != null)
        {
            if (messageCoroutine != null)
                StopCoroutine(messageCoroutine);

            messageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
        }
    }

    public void ShowCompletionMessage()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);

            if (completionText != null)
                completionText.text = $"Great job tracing letter {gameManager.CurrentLetter}!";

            if (celebrationEffect != null)
                celebrationEffect.Play();

            completionPanel.transform.localScale = Vector3.zero;
            LeanTween.scale(completionPanel, Vector3.one, 0.5f)
                .setEase(LeanTweenType.easeOutBack);
        }

        ShowMessage("Letter completed! Well done!", 3.0f);
    }

    public void ShowGameComplete()
    {
        if (gameCompletePanel != null)
        {
            gameCompletePanel.SetActive(true);
            gameCompletePanel.transform.localScale = Vector3.zero;
            LeanTween.scale(gameCompletePanel, Vector3.one, 0.5f)
                .setEase(LeanTweenType.easeOutBack);
        }
    }

    void UpdateProgressBar()
    {
        if (progressSlider != null && gameManager != null && gameManager.GetComponent<LetterTracer>() != null)
        {
            LetterTracer tracer = gameManager.GetComponent<LetterTracer>();
            progressSlider.value = tracer.CompletionPercentage;
        }
    }

    void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    System.Collections.IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        messageText.text = message;
        messageText.color = Color.yellow;
        messageText.transform.localScale = Vector3.zero;
        LeanTween.scale(messageText.gameObject, Vector3.one, 0.2f);

        yield return new WaitForSeconds(duration);

        LeanTween.scale(messageText.gameObject, Vector3.zero, 0.2f)
            .setOnComplete(() => messageText.text = "");

        messageCoroutine = null;
    }
}
