using UnityEngine;
using TMPro;

/// <summary>
/// Displays the in game HUD showing score, lives, and power up timer.
/// Polls GameManager each frame for current values.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI powerUpTimerText;

    private void Update()
    {
        UpdateScore();
        UpdateLives();
        UpdatePowerUpTimer();
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    private void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + GameManager.Instance.GetCurrentScore();
        }
    }

    /// <summary>
    /// Updates the lives display.
    /// </summary>
    private void UpdateLives()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + GameManager.Instance.GetPlayerLives();
        }
    }

    /// <summary>
    /// Shows the power up timer when active, hides it when not.
    /// </summary>
    private void UpdatePowerUpTimer()
    {
        if (powerUpTimerText == null)
        {
            return;
        }

        if (GameManager.Instance.IsPowerUpActive())
        {
            float timeRemaining = GameManager.Instance.GetPowerUpTimeRemaining();
            powerUpTimerText.gameObject.SetActive(true);
            powerUpTimerText.text = "Power Up: " + timeRemaining.ToString("F1") + "s";
        }
        else
        {
            powerUpTimerText.gameObject.SetActive(false);
        }
    }
}