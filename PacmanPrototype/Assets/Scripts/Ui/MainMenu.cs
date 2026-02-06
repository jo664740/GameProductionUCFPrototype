using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the main menu scene.
/// Provides buttons to start the game or quit the application.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";

    /// <summary>
    /// Loads the game scene. Hooked up to the start button in the Inspector.
    /// </summary>
    public void OnStartButton()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Quits the application. Only works in a built game, not in the editor.
    /// </summary>
    public void OnQuitButton()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();
    }
}