using UnityEngine;

/// <summary>
/// A simple, floating enemy that moves directly towards the player and deals damage on contact.
/// It uses Rigidbody for movement and disables the NavMeshAgent.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SwarmEnemy : EnemyBase
{
    [Header("Swarm Specifics")]
    [Tooltip("How fast the enemy moves directly towards the player.")]
    public float moveSpeed = 8f;
    [Tooltip("The amount of damage dealt on collision with the player.")]
    public float meleeDamage = 5f;

    private Rigidbody rb;

    protected override void Awake()
    {
        // We MUST call the base Awake() first to find the player and set up health.
        base.Awake();

        // Get the Rigidbody component.
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Disable gravity so it can float.
            rb.useGravity = false;
        }

        // --- CRITICAL STEP FOR THIS ENEMY TYPE ---
        // Disable the NavMeshAgent. This enemy does not use pathfinding.
        // Its movement is controlled directly via its Rigidbody.
        agent.enabled = false;
    }

    // This enemy's behavior is simple: always chase the player.
    // So, Patrol() and Chase() will do the same thing. Attack() will do nothing
    // because the attack is handled by physical collision.

    protected override void Patrol()
    {
        MoveTowardsPlayer();
    }

    protected override void Chase()
    {
        MoveTowardsPlayer();
    }

    protected override void Attack()
    {
        // The Swarm enemy is always moving, so it attacks while "chasing".
        // We just need to make sure it keeps moving towards the player.
        MoveTowardsPlayer();
    }

    /// <summary>
    /// The core movement logic for this enemy. Applies velocity to the Rigidbody.
    /// </summary>
    private void MoveTowardsPlayer()
    {
        if (player == null || rb == null) return;

        // Calculate the direction from the enemy to the player.
        Vector3 direction = (player.position - transform.position).normalized;

        // Set the Rigidbody's velocity to move in that direction at the desired speed.
        rb.linearVelocity = direction * moveSpeed;

        // Optional: Make the enemy look where it's going.
        if (rb.linearVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    // The attack is handled by physics collision.
    private void OnCollisionEnter(Collision collision)
    {
        // Check if we collided with an object that can take damage.
        IDamageable damageableTarget = collision.gameObject.GetComponentInParent<IDamageable>();

        // We only want to deal damage if we hit the player.
        // We can check if the damageable component is on a PlayerStatsManager.
        if (damageableTarget != null && collision.gameObject.GetComponentInParent<PlayerStatsManager>() != null)
        {
            // Deal damage to the player.
            damageableTarget.TakeDamage(meleeDamage);

            // After attacking, the swarm enemy destroys itself.
            // We call Die() to ensure all cleanup logic from EnemyBase is run.
            Die();
        }
    }
}