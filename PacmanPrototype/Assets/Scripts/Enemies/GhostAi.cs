using UnityEngine;

/// <summary>
/// Controls ghost enemy movement and behavior.
/// Ghosts alternate between chasing the player and wandering randomly
/// on a timer. Each ghost can have a different start delay so they
/// don't all chase at the same time, keeping them spread across the maze.
/// When a power up is active, ghosts enter a frightened state
/// where they move randomly and can be defeated by the player.
/// Wraps position to the opposite side of the grid when moving past the edge.
/// Uses ghost specific walkability checks to pass through ghost gates.
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

    // Chase and wander cycle
    private bool isChasing = false;
    private float behaviorTimer = 0f;
    private bool hasStarted = false;
    private float startTimer = 0f;

    // Reusable array to avoid allocating every frame
    private readonly Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

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

        // Only cycle chase and wander when not frightened
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
    /// Moves the ghost tile by tile toward a target direction.
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
            // Teleport instantly when wrapping to the opposite side
            float distanceToTarget = Vector2.Distance(rb.position, targetPosition);
            if (distanceToTarget > GridManager.Instance.GetGridWidth() / 2f)
            {
                rb.MovePosition(targetPosition);
                isMoving = false;
                return;
            }

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
    /// Chase mode picks the direction closest to the player.
    /// Wander and frightened modes pick a random valid direction.
    /// Ghosts avoid reversing unless it is the only option.
    /// Uses IsGhostWalkable so ghosts can pass through ghost gates.
    /// </summary>
    private void ChooseDirection()
    {
        Vector2 reverseDirection = -currentDirection;
        bool useRandomMovement = isFrightened || !isChasing;

        if (useRandomMovement)
        {
            ShuffleDirections(directions);
        }

        Vector2 bestDirection = Vector2.zero;
        float bestDistance = float.MaxValue;

        foreach (Vector2 direction in directions)
        {
            if (direction == reverseDirection && currentDirection != Vector2.zero)
            {
                continue;
            }

            Vector2 nextGridPos = GridManager.Instance.GetNextGridPosition(targetPosition, direction);

            // Wrap to opposite side if moving past grid edge
            if (!GridManager.Instance.IsWithinGridBounds(nextGridPos))
            {
                nextGridPos = GridManager.Instance.WrapGridPosition(nextGridPos);
            }

            if (!GridManager.Instance.IsGhostWalkable(nextGridPos))
            {
                continue;
            }

            if (useRandomMovement)
            {
                bestDirection = direction;
                break;
            }

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

        // Allow reversing as a last resort if no forward direction is valid
        if (bestDirection == Vector2.zero && currentDirection != Vector2.zero)
        {
            Vector2 reverseGridPos = GridManager.Instance.GetNextGridPosition(targetPosition, reverseDirection);

            if (!GridManager.Instance.IsWithinGridBounds(reverseGridPos))
            {
                reverseGridPos = GridManager.Instance.WrapGridPosition(reverseGridPos);
            }

            if (GridManager.Instance.IsGhostWalkable(reverseGridPos))
            {
                bestDirection = reverseDirection;
            }
        }

        if (bestDirection != Vector2.zero)
        {
            currentDirection = bestDirection;
            Vector2 nextPos = GridManager.Instance.GetNextGridPosition(targetPosition, currentDirection);

            // Apply wrapping to the actual target position
            if (!GridManager.Instance.IsWithinGridBounds(nextPos))
            {
                nextPos = GridManager.Instance.WrapGridPosition(nextPos);
            }

            targetPosition = nextPos;
            isMoving = true;
        }
    }

    /// <summary>
    /// Shuffles the direction array in place for random movement.
    /// </summary>
    private void ShuffleDirections(Vector2[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2 temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    /// <summary>
    /// Enters frightened state, reversing direction and moving randomly.
    /// </summary>
    private void EnterFrightenedState()
    {
        isFrightened = true;
        currentDirection = -currentDirection;
        Debug.Log($"{gameObject.name} entered frightened state");
    }

    /// <summary>
    /// Exits frightened state, resuming the chase and wander cycle.
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
    /// Handles ghost death, awards points, and schedules respawn.
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
    /// Red means chasing, yellow means wandering, blue means frightened.
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