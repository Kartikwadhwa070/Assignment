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
            StartCoroutine(AnimateLetterDisplay());
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

            StartCoroutine(AnimateCompletionPanel());
        }

        ShowMessage("Letter completed! Well done!", 3.0f);
    }

    public void ShowGameComplete()
    {
        if (gameCompletePanel != null)
        {
            gameCompletePanel.SetActive(true);
            StartCoroutine(AnimateGameCompletePanel());
        }
    }

    void UpdateProgressBar()
    {
        if (progressSlider != null)
        {
            LetterTracer tracer = FindObjectOfType<LetterTracer>();
            if (tracer != null)
            {
                progressSlider.value = tracer.CompletionPercentage;
            }
        }
    }

    void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    private System.Collections.IEnumerator AnimateLetterDisplay()
    {
        if (letterDisplayText == null) yield break;

        Vector3 originalScale = letterDisplayText.transform.localScale;
        letterDisplayText.transform.localScale = Vector3.zero;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            progress = 1f - (1f - progress) * (1f - progress);
            letterDisplayText.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }

        letterDisplayText.transform.localScale = originalScale;
    }

    private System.Collections.IEnumerator AnimateCompletionPanel()
    {
        if (completionPanel == null) yield break;

        Vector3 originalScale = Vector3.one;
        completionPanel.transform.localScale = Vector3.zero;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            progress = 1f - (1f - progress) * (1f - progress) * (1f - progress);
            completionPanel.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }

        completionPanel.transform.localScale = originalScale;
    }

    private System.Collections.IEnumerator AnimateGameCompletePanel()
    {
        if (gameCompletePanel == null) yield break;

        Vector3 originalScale = Vector3.one;
        gameCompletePanel.transform.localScale = Vector3.zero;

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (progress < 0.5f)
            {
                progress = 2f * progress * progress;
            }
            else
            {
                progress = 1f - 2f * (1f - progress) * (1f - progress);
            }

            gameCompletePanel.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }

        gameCompletePanel.transform.localScale = originalScale;
    }

    System.Collections.IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        if (messageText == null) yield break;

        messageText.text = message;
        messageText.color = Color.yellow;

        Vector3 originalScale = Vector3.one;
        messageText.transform.localScale = Vector3.zero;

        float animDuration = 0.2f;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animDuration;
            progress = progress * progress;
            messageText.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }

        messageText.transform.localScale = originalScale;
        yield return new WaitForSeconds(duration);

        elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animDuration;
            progress = 1f - (1f - progress) * (1f - progress);
            messageText.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            yield return null;
        }

        messageText.transform.localScale = Vector3.zero;
        messageText.text = "";
        messageText.transform.localScale = originalScale;
        messageCoroutine = null;
    }
}
