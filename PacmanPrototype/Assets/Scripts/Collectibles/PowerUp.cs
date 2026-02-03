using UnityEngine;

/// <summary>
/// Handles power-up item behavior (rubber ducks in your UCF theme).
/// Awards points, activates power-up mode, and enables Tron trail.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class PowerUp : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [SerializeField] private float powerUpDuration = 10f;
    [SerializeField] private int pointValue = 50;
    [SerializeField] private bool enableTronTrail = true;

    private CircleCollider2D circleCollider;
    private bool hasBeenActivated = false;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
    }

    /// <summary>
    /// Called by PlayerController when player touches this power-up.
    /// </summary>
    public void Activate()
    {
        if (hasBeenActivated)
        {
            return;
        }

        hasBeenActivated = true;

        GameManager.Instance.CollectItem(pointValue);
        GameManager.Instance.ActivatePowerUp(powerUpDuration);

        Debug.Log($"Power-up activated! Duration: {powerUpDuration} seconds, Points: {pointValue}");

        Destroy(gameObject);
    }

    /// <summary>
    /// Visualizes the power-up's trigger radius in Scene view.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}