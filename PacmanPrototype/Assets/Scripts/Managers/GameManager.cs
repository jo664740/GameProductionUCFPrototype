using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central manager for game state, score tracking, and win/loss conditions.
/// Implements singleton pattern with DontDestroyOnLoad so player lives
/// persist between levels. Score resets each level.
/// Automatically loads the next level when the current one is completed.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private int currentScore = 0;

    [Header("Level Requirements")]
    [SerializeField] private int totalCollectibles = 0;
    [SerializeField] private int totalGhosts = 0;

    [Header("Power Up Settings")]
    [SerializeField] private bool isPowerUpActive = false;
    [SerializeField] private float powerUpTimeRemaining = 0f;

    [Header("Level Progression")]
    [SerializeField] private float nextLevelDelay = 2f;

    [Header("UI References")]
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private VictoryScreen victoryScreen;

    private int collectiblesRemaining;
    private int ghostsRemaining;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called every time a new scene finishes loading.
    /// Resets score and reinitializes the level.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cancel any pending invokes from the previous level
        CancelInvoke();

        // Skip reinitialization for the main menu scene
        if (scene.name == "MainMenu")
        {
            return;
        }

        currentScore = 0;
        isGameOver = false;
        isPowerUpActive = false;
        powerUpTimeRemaining = 0f;

        // Find UI references in the new scene
        gameOverScreen = FindFirstObjectByType<GameOverScreen>();
        victoryScreen = FindFirstObjectByType<VictoryScreen>();

        InitializeLevel();
    }

    private void Update()
    {
        if (isPowerUpActive)
        {
            UpdatePowerUpTimer();
        }
    }

    /// <summary>
    /// Counts collectibles and ghosts in the scene and sets initial values.
    /// </summary>
    private void InitializeLevel()
    {
        collectiblesRemaining = GameObject.FindGameObjectsWithTag("Collectible").Length;
        ghostsRemaining = GameObject.FindGameObjectsWithTag("Ghost").Length;

        totalCollectibles = collectiblesRemaining;
        totalGhosts = ghostsRemaining;

        Debug.Log($"Level initialized: {collectiblesRemaining} collectibles, {ghostsRemaining} ghosts");
    }

    /// <summary>
    /// Called when player collects a collectible item.
    /// </summary>
    public void CollectItem(int points)
    {
        currentScore += points;
        collectiblesRemaining--;

        Debug.Log($"Collected item! Score: {currentScore}, Remaining: {collectiblesRemaining}");

        CheckWinCondition();
    }

    /// <summary>
    /// Activates power up mode for specified duration.
    /// </summary>
    /// <param name="duration">How long the power up lasts in seconds.</param>
    public void ActivatePowerUp(float duration)
    {
        isPowerUpActive = true;
        powerUpTimeRemaining = duration;

        Debug.Log($"Power up activated for {duration} seconds");
    }

    /// <summary>
    /// Counts down power up timer and deactivates when expired.
    /// </summary>
    private void UpdatePowerUpTimer()
    {
        powerUpTimeRemaining -= Time.deltaTime;

        if (powerUpTimeRemaining <= 0f)
        {
            DeactivatePowerUp();
        }
    }

    /// <summary>
    /// Ends power up mode and returns ghosts to normal behavior.
    /// </summary>
    private void DeactivatePowerUp()
    {
        isPowerUpActive = false;
        powerUpTimeRemaining = 0f;

        Debug.Log("Power up deactivated");
    }

    /// <summary>
    /// Called when a ghost is defeated during power up mode.
    /// </summary>
    public void GhostDefeated(int points)
    {
        currentScore += points;
        ghostsRemaining--;

        Debug.Log($"Ghost defeated! Score: {currentScore}, Ghosts remaining: {ghostsRemaining}");

        CheckWinCondition();
    }

    /// <summary>
    /// Called when player loses a life from ghost collision.
    /// </summary>
    public void LoseLife()
    {
        if (isGameOver)
        {
            return;
        }

        playerLives--;
        Debug.Log($"Life lost! Lives remaining: {playerLives}");

        if (playerLives <= 0)
        {
            GameOver();
        }
        else
        {
            RespawnPlayer();
        }
    }

    /// <summary>
    /// Finds the player and calls its Respawn method to reset position
    /// back to where it started with brief invincibility.
    /// </summary>
    private void RespawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.Respawn();
            }
        }
    }

    /// <summary>
    /// Checks if both win conditions are met: all collectibles and all ghosts.
    /// </summary>
    private void CheckWinCondition()
    {
        if (isGameOver)
        {
            return;
        }

        if (collectiblesRemaining <= 0 && ghostsRemaining <= 0)
        {
            WinLevel();
        }
    }

    /// <summary>
    /// Handles level completion. If there is a next level in the build settings,
    /// loads it after a delay. If this is the last level, shows the victory screen.
    /// </summary>
    private void WinLevel()
    {
        isGameOver = true;
        Debug.Log("Level Complete!");

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if there is another level after this one
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"Loading next level in {nextLevelDelay} seconds");
            Invoke(nameof(LoadNextLevel), nextLevelDelay);
        }
        else
        {
            Debug.Log("Final level complete! You Win!");
            if (victoryScreen != null)
            {
                victoryScreen.Show(currentScore);
            }
        }
    }

    /// <summary>
    /// Loads the next scene in the build order.
    /// </summary>
    private void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }

    /// <summary>
    /// Handles game over and shows the game over screen.
    /// </summary>
    private void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");

        if (gameOverScreen != null)
        {
            gameOverScreen.Show(currentScore);
        }
    }

    /// <summary>
    /// Resets lives back to full. Called when returning to main menu
    /// or starting a new game so the player gets a fresh start.
    /// </summary>
    public void ResetLives()
    {
        playerLives = 3;
    }

    public int GetCurrentScore() => currentScore;
    public int GetPlayerLives() => playerLives;
    public bool IsPowerUpActive() => isPowerUpActive;
    public float GetPowerUpTimeRemaining() => powerUpTimeRemaining;
    public bool IsGameOver() => isGameOver;
}