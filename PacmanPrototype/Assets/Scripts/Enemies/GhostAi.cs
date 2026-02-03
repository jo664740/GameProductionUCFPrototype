using UnityEngine;

/// <summary>
/// Placeholder AI for ghost enemies.
/// Handles collision with player and death from Tron trail.
/// Full AI implementation (chase/scatter/frightened states) to be added later.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class GhostAI : MonoBehaviour
{
    [Header("Ghost Settings")]
    [SerializeField] private int pointsWhenDefeated = 200;
    [SerializeField] private float respawnDelay = 3f;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Vector2 startPosition;
    private bool isFrightened = false;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;

        startPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance.IsPowerUpActive() && !isFrightened)
        {
            EnterFrightenedState();
        }
        else if (!GameManager.Instance.IsPowerUpActive() && isFrightened)
        {
            ExitFrightenedState();
        }
    }

    /// <summary>
    /// Enters frightened state when power-up is active.
    /// </summary>
    private void EnterFrightenedState()
    {
        isFrightened = true;
        Debug.Log($"{gameObject.name} entered frightened state");
        // TODO: Change ghost color/appearance to indicate vulnerability
    }

    /// <summary>
    /// Exits frightened state when power-up expires.
    /// </summary>
    private void ExitFrightenedState()
    {
        isFrightened = false;
        Debug.Log($"{gameObject.name} exited frightened state");
        // TODO: Restore normal ghost appearance
    }

    /// <summary>
    /// Called by PlayerController when player collides with this ghost.
    /// </summary>
    public void HandlePlayerCollision()
    {
        if (isDead)
        {
            return;
        }

        if (isFrightened)
        {
            Die();
        }
        else
        {
            GameManager.Instance.LoseLife();
        }
    }

    /// <summary>
    /// Checks if ghost can be killed by Tron trail (only when frightened).
    /// </summary>
    public bool CanBeKilledByTrail()
    {
        return isFrightened && !isDead;
    }

    /// <summary>
    /// Called by PlayerController when Tron trail touches this ghost.
    /// </summary>
    public void DieFromTrail()
    {
        if (!CanBeKilledByTrail())
        {
            return;
        }

        Die();
    }

    /// <summary>
    /// Handles ghost death, awards points, and triggers respawn.
    /// </summary>
    private void Die()
    {
        isDead = true;
        GameManager.Instance.GhostDefeated(pointsWhenDefeated);
        
        Debug.Log($"{gameObject.name} was defeated! +{pointsWhenDefeated} points");

        // TODO: Play death animation/sound
        gameObject.SetActive(false);

        Invoke(nameof(Respawn), respawnDelay);
    }

    /// <summary>
    /// Respawns ghost at starting position after delay.
    /// </summary>
    private void Respawn()
    {
        transform.position = startPosition;
        isDead = false;
        isFrightened = false;
        gameObject.SetActive(true);

        Debug.Log($"{gameObject.name} respawned at start position");
    }

    /// <summary>
    /// Visualizes ghost's detection radius in Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = isFrightened ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}