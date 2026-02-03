using UnityEngine;

/// <summary>
/// Handles collectible item behavior (pellets/dots).
/// Awards points and notifies GameManager when collected.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private int pointValue = 10;
    [SerializeField] private bool playCollectionSound = true;

    private CircleCollider2D circleCollider;
    private bool hasBeenCollected = false;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
    }

    /// <summary>
    /// Called by PlayerController when player touches this collectible.
    /// </summary>
    public void Collect()
    {
        if (hasBeenCollected)
        {
            return;
        }

        hasBeenCollected = true;

        GameManager.Instance.CollectItem(pointValue);

        if (playCollectionSound)
        {
            // TODO: Play collection sound effect
            Debug.Log($"Collected pellet worth {pointValue} points");
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Visualizes the collectible's trigger radius in Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}