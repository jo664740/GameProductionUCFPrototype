using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controls the game over screen panel.
/// Displays the final score and provides a retry button.
/// Hidden by default and shown when the player loses all lives.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the game over screen with the player's final score.
    /// </summary>
    /// <param name="score">The final score to display.</param>
    public void Show(int score)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + score;
        }

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Restarts the current scene. Hooked up to the retry button in the Inspector.
    /// </summary>
    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Loads the main menu scene. Hooked up to the menu button in the Inspector.
    /// </summary>
    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}