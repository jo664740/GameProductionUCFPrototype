using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the pause menu panel.
/// Toggles pause on and off with the Escape key.
/// Freezes game time while paused.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;

    private bool isPaused = false;

    private void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameManager.Instance.IsGameOver())
            {
                return;
            }

            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    /// <summary>
    /// Pauses the game and shows the pause panel.
    /// </summary>
    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    /// <summary>
    /// Resumes the game and hides the pause panel.
    /// Hooked up to the resume button in the Inspector.
    /// </summary>
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}