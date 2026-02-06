using UnityEngine;

/// <summary>
/// Controls ghost enemy movement and behavior.
/// Ghosts alternate between chasing the player and wandering randomly
/// on a timer. Each ghost can have a different start delay so they
/// don't all chase at the same time, keeping them spread across the maze.
/// When a power-up is active, ghosts enter a frightened state
/// where they move randomly and can be defeated by the player.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class GhostAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float frightenedSpeed = 2f;

    [Header("Behavior Timing")]
    [SerializeField] private float chaseDuration = 10f;
    [SerializeField] private float wanderDuration = 5f;
    [SerializeField] private float startDelay = 0f;

    [Header("Ghost Settings")]
    [SerializeField] private int pointsWhenDefeated = 200;
    [SerializeField] private float respawnDelay = 3f;

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Vector2 startPosition;

    private Vector2 currentDirection = Vector2.zero;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isFrightened = false;
    private bool isDead = false;

    // Chase/wander cycle
    private bool isChasing = false;
    private float behaviorTimer = 0f;
    private bool hasStarted = false;
    private float startTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;

        startPosition = transform.position;
    }

    private void Start()
    {
        targetPosition = GridManager.Instance.SnapToGrid(transform.position);
        transform.position = targetPosition;

        // Auto-find the player if not assigned in the Inspector
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        // Wait for the start delay before the ghost begins moving
        if (!hasStarted)
        {
            startTimer += Time.deltaTime;
            if (startTimer >= startDelay)
            {
                hasStarted = true;
            }
            return;
        }

        if (GameManager.Instance.IsPowerUpActive() && !isFrightened)
        {
            EnterFrightenedState();
        }
        else if (!GameManager.Instance.IsPowerUpActive() && isFrightened)
        {
            ExitFrightenedState();
        }

        // Only cycle chase/wander when not frightened
        if (!isFrightened)
        {
            UpdateBehaviorTimer();
        }
    }

    private void FixedUpdate()
    {
        if (isDead || !hasStarted)
        {
            return;
        }

        HandleMovement();
    }

    /// <summary>
    /// Alternates between chase and wander modes on a timer.
    /// </summary>
    private void UpdateBehaviorTimer()
    {
        behaviorTimer += Time.deltaTime;

        if (isChasing && behaviorTimer >= chaseDuration)
        {
            isChasing = false;
            behaviorTimer = 0f;
        }
        else if (!isChasing && behaviorTimer >= wanderDuration)
        {
            isChasing = true;
            behaviorTimer = 0f;
        }
    }

    /// <summary>
    /// Moves the ghost tile-by-tile toward a target direction.
    /// Picks a new direction each time it reaches a tile center.
    /// </summary>
    private void HandleMovement()
    {
        if (!isMoving)
        {
            ChooseDirection();
        }

        if (isMoving)
        {
            float speed = isFrightened ? frightenedSpeed : moveSpeed;
            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
            {
                rb.MovePosition(targetPosition);
                isMoving = false;
            }
        }
    }

    /// <summary>
    /// Picks the best direction at the current tile.
    /// Chase mode: picks the direction closest to the player.
    /// Wander/Frightened mode: picks a random valid direction.
    /// Ghosts avoid reversing unless it's the only option.
    /// </summary>
    private void ChooseDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 reverseDirection = -currentDirection;

        // Wander and frightened both use random movement
        bool useRandomMovement = isFrightened || !isChasing;

        if (useRandomMovement)
        {
            ShuffleDirections(directions);
        }

        Vector2 bestDirection = Vector2.zero;
        float bestDistance = float.MaxValue;

        foreach (Vector2 direction in directions)
        {
            // Don't reverse unless there's no other choice
            if (direction == reverseDirection && currentDirection != Vector2.zero)
            {
                continue;
            }

            Vector2 nextGridPos = GridManager.Instance.GetNextGridPosition(targetPosition, direction);

            if (!GridManager.Instance.IsWalkable(nextGridPos) || !GridManager.Instance.IsWithinGridBounds(nextGridPos))
            {
                continue;
            }

            if (useRandomMovement)
            {
                // Random — take the first valid shuffled direction
                bestDirection = direction;
                break;
            }

            // Chase — pick whichever direction gets closest to the player
            if (playerTransform != null)
            {
                Vector2 nextWorldPos = GridManager.Instance.GridToWorldPosition(nextGridPos);
                float distance = Vector2.Distance(nextWorldPos, playerTransform.position);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDirection = direction;
                }
            }
            else
            {
                bestDirection = direction;
                break;
            }
        }

        // Last resort — allow reversing if stuck
        if (bestDirection == Vector2.zero && currentDirection != Vector2.zero)
        {
            Vector2 reverseGridPos = GridManager.Instance.GetNextGridPosition(targetPosition, reverseDirection);

            if (GridManager.Instance.IsWalkable(reverseGridPos) && GridManager.Instance.IsWithinGridBounds(reverseGridPos))
            {
                bestDirection = reverseDirection;
            }
        }

        if (bestDirection != Vector2.zero)
        {
            currentDirection = bestDirection;
            targetPosition = GridManager.Instance.GetNextGridPosition(targetPosition, currentDirection);
            isMoving = true;
        }
    }

    /// <summary>
    /// Fisher-Yates shuffle for randomizing direction order.
    /// </summary>
    private void ShuffleDirections(Vector2[] directions)
    {
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2 temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }
    }

    /// <summary>
    /// Enters frightened state — reverses direction and moves randomly.
    /// </summary>
    private void EnterFrightenedState()
    {
        isFrightened = true;
        currentDirection = -currentDirection;
        Debug.Log($"{gameObject.name} entered frightened state");
    }

    /// <summary>
    /// Exits frightened state — resumes chase/wander cycle.
    /// </summary>
    private void ExitFrightenedState()
    {
        isFrightened = false;
        Debug.Log($"{gameObject.name} exited frightened state");
    }

    /// <summary>
    /// Called by PlayerController when the player collides with this ghost.
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
    /// Checks if the ghost can be killed by the Tron trail.
    /// </summary>
    public bool CanBeKilledByTrail()
    {
        return isFrightened && !isDead;
    }

    /// <summary>
    /// Called by PlayerController when the Tron trail touches this ghost.
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
    /// Handles ghost death — awards points and schedules respawn.
    /// </summary>
    private void Die()
    {
        isDead = true;
        GameManager.Instance.GhostDefeated(pointsWhenDefeated);

        Debug.Log($"{gameObject.name} was defeated! +{pointsWhenDefeated} points");

        gameObject.SetActive(false);
        Invoke(nameof(Respawn), respawnDelay);
    }

    /// <summary>
    /// Respawns the ghost at its starting position.
    /// </summary>
    private void Respawn()
    {
        transform.position = startPosition;
        targetPosition = startPosition;
        currentDirection = Vector2.zero;
        isMoving = false;
        isDead = false;
        isFrightened = false;
        isChasing = false;
        behaviorTimer = 0f;
        hasStarted = true;

        gameObject.SetActive(true);
        Debug.Log($"{gameObject.name} respawned at start position");
    }

    /// <summary>
    /// Draws a colored wireframe sphere in Scene view for debugging.
    /// Red = chasing, Yellow = wandering, Blue = frightened.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (isFrightened)
        {
            Gizmos.color = Color.blue;
        }
        else if (isChasing)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}