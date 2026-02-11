using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controls the victory screen panel.
/// Displays the final score when the player completes the last level.
/// Hidden by default and only shown after all levels are beaten.
/// Plays a victory sound effect when shown.
/// </summary>
public class VictoryScreen : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip winSound;
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
    /// Shows the victory screen with the player's final score.
    /// Stops the game audio and plays the win sound effect.
     /// </summary>
     /// <param name="score">The final score to display.</param>
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
        AudioSource music = FindObjectOfType<AudioSource>();
        if (music != null)
        {    
            music.Stop();
        }    
        if (winSound != null)
        {
            AudioSource.PlayClipAtPoint(winSound, Camera.main.transform.position);
        }
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Restarts from level 1 with fresh lives.
    /// Hooked up to the play again button in the Inspector.
    /// </summary>
    public void OnPlayAgainButton()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetLives();
        }

        // Load the first game scene (build index 1, after MainMenu at index 0)
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Loads the main menu scene.
    /// Hooked up to the menu button in the Inspector.
    /// </summary>
    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}