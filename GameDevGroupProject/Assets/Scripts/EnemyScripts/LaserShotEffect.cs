using UnityEngine;

/// <summary>
/// Manages the visual effect for a temporary laser beam.
/// Sets the start and end points of a LineRenderer and destroys itself after a short duration.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LaserShotEffect : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Tooltip("How long the laser beam will be visible in seconds.")]
    public float duration = 0.15f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    /// <summary>
    /// Displays the laser from the start point to the end point.
    /// </summary>
    /// <param name="start">The world-space starting position of the laser.</param>
    /// <param name="end">The world-space ending position of the laser.</param>
    public void Show(Vector3 start, Vector3 end)
    {
        // Set the positions of the LineRenderer
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Schedule the destruction of this effect object.
        Destroy(gameObject, duration);
    }
}