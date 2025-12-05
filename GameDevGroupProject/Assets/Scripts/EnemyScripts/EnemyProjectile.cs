using UnityEngine;

/// <summary>
/// Controls the behavior of a projectile fired by an enemy.
/// It moves forward and deals damage to any IDamageable it hits.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody rb;

    [Tooltip("How long the projectile lives before being automatically destroyed.")]
    public float lifetime = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Make sure the Rigidbody doesn't use gravity unless you want arcing shots.
        rb.useGravity = false;
    }

    void Start()
    {
        // Automatically destroy the projectile after its lifetime expires to prevent leaks.
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Initializes the projectile's speed and damage. Called by the enemy that fires it.
    /// </summary>
    public void Initialize(float speed, float damage)
    {
        // Set its forward velocity.
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }

        // You could store the damage here if needed, but for now we'll pass it on collision.
        this.damage = damage;
    }

    private float damage; // Stored from Initialize

    private void OnCollisionEnter(Collision collision)
    {
        // Try to find a damageable component on the object we hit.
        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();

        // If it's the player (or anything else that can take damage)...
        if (damageable != null)
        {
            // Deal damage.
            damageable.TakeDamage(damage);
        }

        // You can add an impact effect here if you want.
        // Instantiate(impactVFX, transform.position, Quaternion.identity);

        // Destroy the projectile on any impact.
        Destroy(gameObject);
    }
}