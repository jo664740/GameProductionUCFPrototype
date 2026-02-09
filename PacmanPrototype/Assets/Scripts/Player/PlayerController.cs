using UnityEngine;

/// <summary>
/// Handles player movement on a grid based system with Pac Man style controls.
/// Supports Tron style trail mechanics during power up mode.
/// Wraps position to the opposite side of the grid when moving past the edge.
/// Includes brief invincibility after respawning to prevent instant double deaths.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Respawn Settings")]
    [SerializeField] private float invincibilityDuration = 2f;

    [Header("Trail Settings")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float trailKillRadius = 0.5f;
    [SerializeField] private LayerMask ghostLayer;

    private Rigidbody2D rb;
    private Vector2 currentDirection = Vector2.zero;
    private Vector2 inputDirection = Vector2.zero;
    private Vector2 targetPosition;
    private Vector2 startPosition;
    private bool isMoving = false;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
    }

    private void Start()
    {
        targetPosition = GridManager.Instance.SnapToGrid(transform.position);
        transform.position = targetPosition;
        startPosition = targetPosition;
    }

    private void Update()
    {
        HandleInput();

        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
            }
        }

        if (GameManager.Instance.IsPowerUpActive() && trailRenderer != null)
        {
            if (!trailRenderer.enabled)
            {
                trailRenderer.enabled = true;
            }
            CheckTronTrailCollisions();
        }
        else if (trailRenderer != null && trailRenderer.enabled)
        {
            trailRenderer.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        if (UnityEngine.InputSystem.Keyboard.current.wKey.isPressed || UnityEngine.InputSystem.Keyboard.current.upArrowKey.isPressed)
        {
            inputDirection = Vector2.up;
        }
        else if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed || UnityEngine.InputSystem.Keyboard.current.downArrowKey.isPressed)
        {
            inputDirection = Vector2.down;
        }
        else if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed || UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed)
        {
            inputDirection = Vector2.left;
        }
        else if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed || UnityEngine.InputSystem.Keyboard.current.rightArrowKey.isPressed)
        {
            inputDirection = Vector2.right;
        }
    }

    private void HandleMovement()
    {
        if (!isMoving)
        {
            if (inputDirection != Vector2.zero)
            {
                Vector2 nextPosition = GridManager.Instance.GetNextGridPosition(targetPosition, inputDirection);

                // Wrap to opposite side if moving past grid edge
                if (!GridManager.Instance.IsWithinGridBounds(nextPosition))
                {
                    nextPosition = GridManager.Instance.WrapGridPosition(nextPosition);
                }

                if (GridManager.Instance.IsWalkable(nextPosition))
                {
                    currentDirection = inputDirection;
                    targetPosition = nextPosition;
                    isMoving = true;
                }
            }
        }

        if (isMoving)
        {
            // Check if we need to teleport (target is on the opposite side of the grid)
            float distanceToTarget = Vector2.Distance(rb.position, targetPosition);
            if (distanceToTarget > GridManager.Instance.GetGridWidth() / 2f)
            {
                rb.MovePosition(targetPosition);
                isMoving = false;
                return;
            }

            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
            {
                rb.MovePosition(targetPosition);
                isMoving = false;
            }

            if (currentDirection != Vector2.zero)
            {
                float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg - 90f;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    /// <summary>
    /// Resets the player to their starting position with brief invincibility.
    /// Called by GameManager when the player loses a life.
    /// </summary>
    public void Respawn()
    {
        transform.position = startPosition;
        rb.MovePosition(startPosition);
        targetPosition = startPosition;
        currentDirection = Vector2.zero;
        inputDirection = Vector2.zero;
        isMoving = false;

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        Debug.Log($"Player respawned at {startPosition} with {invincibilityDuration}s invincibility");
    }

    /// <summary>
    /// Returns true if the player is currently invincible after respawning.
    /// </summary>
    public bool IsInvincible()
    {
        return isInvincible;
    }

    private void CheckTronTrailCollisions()
    {
        Collider2D[] hitGhosts = Physics2D.OverlapCircleAll(transform.position, trailKillRadius, ghostLayer);

        foreach (Collider2D ghostCollider in hitGhosts)
        {
            GhostAI ghost = ghostCollider.GetComponent<GhostAI>();
            if (ghost != null && ghost.CanBeKilledByTrail())
            {
                ghost.DieFromTrail();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Collectible"))
        {
            Collectible collectible = collision.GetComponent<Collectible>();
            if (collectible != null)
            {
                collectible.Collect();
            }
        }
        else if (collision.CompareTag("PowerUp"))
        {
            PowerUp powerUp = collision.GetComponent<PowerUp>();
            if (powerUp != null)
            {
                powerUp.Activate();
            }
        }
        else if (collision.CompareTag("Ghost"))
        {
            // Ignore ghost collision while invincible
            if (isInvincible)
            {
                return;
            }

            GhostAI ghost = collision.GetComponent<GhostAI>();
            if (ghost != null)
            {
                ghost.HandlePlayerCollision();
            }
        }
    }

    public Vector2 GetCurrentDirection()
    {
        return currentDirection;
    }

    public Vector2 GetTargetPosition()
    {
        return targetPosition;
    }
}