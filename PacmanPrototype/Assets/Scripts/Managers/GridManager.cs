using UnityEngine;

/// <summary>
/// Manages the grid system for the game.
/// Handles grid to world position conversion, collision detection,
/// and position wrapping for tunnel teleportation.
/// Supports ghost gate walls that block the player but not ghosts.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Vector2 gridOrigin = Vector2.zero;
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;

    [Header("Collision Detection")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private LayerMask ghostWallLayerMask;
    [SerializeField] private float wallCheckRadius = 0.4f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log($"GridManager initialized. Wall layer mask value: {wallLayerMask.value}");
    }

    /// <summary>
    /// Converts grid coordinates to world position.
    /// </summary>
    public Vector2 GridToWorldPosition(Vector2 gridPosition)
    {
        return gridOrigin + gridPosition * gridSize;
    }

    /// <summary>
    /// Converts world position to grid coordinates.
    /// </summary>
    public Vector2 WorldToGridPosition(Vector2 worldPosition)
    {
        return (worldPosition - gridOrigin) / gridSize;
    }

    /// <summary>
    /// Rounds a world position to the nearest grid cell center.
    /// </summary>
    public Vector2 SnapToGrid(Vector2 worldPosition)
    {
        Vector2 gridPos = WorldToGridPosition(worldPosition);
        gridPos.x = Mathf.Round(gridPos.x);
        gridPos.y = Mathf.Round(gridPos.y);
        return GridToWorldPosition(gridPos);
    }

    /// <summary>
    /// Checks if a grid position is walkable for the player.
    /// Blocks on both walls and ghost gates.
    /// </summary>
    public bool IsWalkable(Vector2 gridPosition)
    {
        Vector2 worldPosition = GridToWorldPosition(gridPosition);

        Debug.Log($"Checking position: Grid={gridPosition}, World={worldPosition}, Layer={wallLayerMask.value}");

        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, wallCheckRadius, wallLayerMask);

        if (hitCollider != null)
        {
            Debug.Log($"WALL DETECTED at {worldPosition}! Collider: {hitCollider.gameObject.name}");
        }
        else
        {
            Debug.Log($"No wall at {worldPosition}, movement allowed");
        }

        return hitCollider == null;
    }

    /// <summary>
    /// Checks if a grid position is walkable for ghosts.
    /// Only blocks on walls, ghosts can pass through ghost gates.
    /// </summary>
    public bool IsGhostWalkable(Vector2 gridPosition)
    {
        Vector2 worldPosition = GridToWorldPosition(gridPosition);
        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, wallCheckRadius, ghostWallLayerMask);
        return hitCollider == null;
    }

    /// <summary>
    /// Checks if a grid position is within the grid bounds.
    /// </summary>
    public bool IsWithinGridBounds(Vector2 gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }

    /// <summary>
    /// Wraps a grid position to the opposite side of the grid.
    /// Used for tunnel teleportation when moving past the grid edge.
    /// </summary>
    public Vector2 WrapGridPosition(Vector2 gridPosition)
    {
        float wrappedX = gridPosition.x;
        float wrappedY = gridPosition.y;

        if (wrappedX < 0)
        {
            wrappedX = gridWidth - 1;
        }
        else if (wrappedX >= gridWidth)
        {
            wrappedX = 0;
        }

        if (wrappedY < 0)
        {
            wrappedY = gridHeight - 1;
        }
        else if (wrappedY >= gridHeight)
        {
            wrappedY = 0;
        }

        return new Vector2(wrappedX, wrappedY);
    }

    /// <summary>
    /// Calculates the next grid position based on current position and movement direction.
    /// </summary>
    public Vector2 GetNextGridPosition(Vector2 currentPosition, Vector2 direction)
    {
        Vector2 currentGridPos = WorldToGridPosition(currentPosition);
        Vector2 nextGridPos = currentGridPos + direction;

        nextGridPos.x = Mathf.Round(nextGridPos.x);
        nextGridPos.y = Mathf.Round(nextGridPos.y);

        return nextGridPos;
    }

    public int GetGridWidth() => gridWidth;
    public int GetGridHeight() => gridHeight;

    /// <summary>
    /// Visualizes the grid in the Scene view for debugging.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        for (int x = 0; x <= gridWidth; x++)
        {
            Vector2 start = GridToWorldPosition(new Vector2(x, 0));
            Vector2 end = GridToWorldPosition(new Vector2(x, gridHeight));
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            Vector2 start = GridToWorldPosition(new Vector2(0, y));
            Vector2 end = GridToWorldPosition(new Vector2(gridWidth, y));
            Gizmos.DrawLine(start, end);
        }
    }
}