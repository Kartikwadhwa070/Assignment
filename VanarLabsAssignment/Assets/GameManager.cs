using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public string[] availableLetters = { "A", "B", "C" };
    public int currentLetterIndex = 0;

    [Header("References")]
    public LetterTracer letterTracer;
    public UIManager uiManager;

    // Game state
    private int attemptsCount = 0;
    private float sessionStartTime;

    void Start()
    {
        sessionStartTime = Time.time;
        StartCurrentLetter();
    }

    void StartCurrentLetter()
    {
        if (letterTracer != null)
        {
            letterTracer.ResetLetter();
        }

        if (uiManager != null)
        {
            string currentLetter = availableLetters[currentLetterIndex];
            uiManager.UpdateLetterDisplay(currentLetter);
            uiManager.ShowInstructions();
        }

        attemptsCount = 0;
    }

    public void OnLetterCompleted()
    {
        float completionTime = Time.time - sessionStartTime;

        Debug.Log($"Letter {availableLetters[currentLetterIndex]} completed in {completionTime:F1} seconds with {attemptsCount} attempts!");
        if (uiManager != null)
        {
            uiManager.ShowCompletionMessage();
        }
        Invoke(nameof(NextLetter), 3.0f);
    }

    public void NextLetter()
    {
        currentLetterIndex++;

        if (currentLetterIndex >= availableLetters.Length)
        {
            ShowGameComplete();
            return;
        }

        sessionStartTime = Time.time;
        StartCurrentLetter();
    }

    public void RestartCurrentLetter()
    {
        attemptsCount++;
        sessionStartTime = Time.time;

        if (letterTracer != null)
        {
            letterTracer.ResetLetter();
        }

        if (uiManager != null)
        {
            uiManager.ShowInstructions();
        }
    }

    void ShowGameComplete()
    {
        if (uiManager != null)
        {
            uiManager.ShowGameComplete();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void QuitGame()
    {
        Application.Quit();
    }

    public string CurrentLetter => availableLetters[currentLetterIndex];
    public int AttemptsCount => attemptsCount;
}