using UnityEngine;

/// <summary>
/// Manages grid-based movement system for Pac-Man style gameplay.
/// Handles position snapping, movement validation, and grid visualization.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Vector2 gridOrigin = Vector2.zero;
    
    [Header("Grid Boundaries")]
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    
    [Header("Collision Detection")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.4f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector2 SnapToGrid(Vector2 position)
    {
        float snappedX = Mathf.Round((position.x - gridOrigin.x) / gridSize) * gridSize + gridOrigin.x;
        float snappedY = Mathf.Round((position.y - gridOrigin.y) / gridSize) * gridSize + gridOrigin.y;
        
        return new Vector2(snappedX, snappedY);
    }

    public bool IsWalkable(Vector2 gridPosition)
    {
        Collider2D hitCollider = Physics2D.OverlapCircle(gridPosition, wallCheckRadius, wallLayer);
        return hitCollider == null;
    }

    public Vector2 GetNextGridPosition(Vector2 currentPosition, Vector2 direction)
    {
        Vector2 snappedCurrent = SnapToGrid(currentPosition);
        Vector2 nextPosition = snappedCurrent + (direction * gridSize);
        
        return nextPosition;
    }

    public bool IsWithinGridBounds(Vector2 position)
    {
        float minX = gridOrigin.x - (gridWidth / 2f) * gridSize;
        float maxX = gridOrigin.x + (gridWidth / 2f) * gridSize;
        float minY = gridOrigin.y - (gridHeight / 2f) * gridSize;
        float maxY = gridOrigin.y + (gridHeight / 2f) * gridSize;
        
        return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
    }

    public float GetGridDistance(Vector2 positionA, Vector2 positionB)
    {
        Vector2 snappedA = SnapToGrid(positionA);
        Vector2 snappedB = SnapToGrid(positionB);
        
        return Vector2.Distance(snappedA, snappedB) / gridSize;
    }

    public Vector2[] GetValidAdjacentPositions(Vector2 position)
    {
        Vector2 snappedPosition = SnapToGrid(position);
        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };
        
        int validCount = 0;
        Vector2[] adjacentPositions = new Vector2[4];
        
        foreach (Vector2 direction in directions)
        {
            Vector2 adjacentPos = snappedPosition + (direction * gridSize);
            
            if (IsWalkable(adjacentPos) && IsWithinGridBounds(adjacentPos))
            {
                adjacentPositions[validCount] = adjacentPos;
                validCount++;
            }
        }
        
        Vector2[] validPositions = new Vector2[validCount];
        for (int i = 0; i < validCount; i++)
        {
            validPositions[i] = adjacentPositions[i];
        }
        
        return validPositions;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        float halfWidth = (gridWidth / 2f) * gridSize;
        float halfHeight = (gridHeight / 2f) * gridSize;
        
        for (int x = 0; x <= gridWidth; x++)
        {
            float xPos = gridOrigin.x - halfWidth + (x * gridSize);
            Vector2 startPos = new Vector2(xPos, gridOrigin.y - halfHeight);
            Vector2 endPos = new Vector2(xPos, gridOrigin.y + halfHeight);
            Gizmos.DrawLine(startPos, endPos);
        }
        
        for (int y = 0; y <= gridHeight; y++)
        {
            float yPos = gridOrigin.y - halfHeight + (y * gridSize);
            Vector2 startPos = new Vector2(gridOrigin.x - halfWidth, yPos);
            Vector2 endPos = new Vector2(gridOrigin.x + halfWidth, yPos);
            Gizmos.DrawLine(startPos, endPos);
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gridOrigin, 0.2f);
    }

    public float GetGridSize() => gridSize;
    public Vector2 GetGridOrigin() => gridOrigin;
}