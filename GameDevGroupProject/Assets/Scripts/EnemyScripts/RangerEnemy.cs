using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A ranged enemy that patrols, chases the player, and fires physical projectiles.
/// Inherits all the core functionality from EnemyBase.
/// </summary>
public class RangerEnemy : EnemyBase
{
    [Header("Ranger Specifics")]
    [Tooltip("How far from its current position the enemy will wander when patrolling.")]
    public float walkPointRange = 10f;

    [Header("Combat Settings")]
    [Tooltip("How often the enemy can fire (in seconds).")]
    public float timeBetweenAttacks = 2f;
    [Tooltip("The projectile prefab (orb) to be fired.")]
    public GameObject projectilePrefab;
    [Tooltip("The speed of the fired projectile.")]
    public float projectileSpeed = 15f;
    [Tooltip("How much damage the projectile deals on hit.")]
    public float projectileDamage = 10f;
    [Tooltip("An optional transform to mark where projectiles spawn from (e.g., a gun barrel). If null, it spawns from the enemy's center.")]
    public Transform shootOrigin;

    // Private variables for behavior
    private Vector3 walkPoint;
    private bool walkPointSet;
    private float fireCooldownTimer;

    protected override void Update()
    {
        // We call the base class Update() first to ensure player detection is handled.
        base.Update();

        // Count down the attack cooldown timer.
        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;
        }
    }

    protected override void Patrol()
    {
        // Tell the agent it's allowed to move.
        if (agent.isStopped)
            agent.isStopped = false;

        // Find a new random walk point if we don't have one.
        if (!walkPointSet)
            SearchWalkPoint();

        // If we have a walk point, set it as the destination.
        if (walkPointSet)
            agent.SetDestination(walkPoint);

        // Check if we've reached the walk point.
        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // Check if the point is on the NavMesh to prevent errors.
        if (NavMesh.SamplePosition(walkPoint, out _, 1.0f, NavMesh.AllAreas))
            walkPointSet = true;
    }

    protected override void Chase()
    {
        // Tell the agent to move and set its destination to the player.
        if (agent.isStopped)
            agent.isStopped = false;

        agent.SetDestination(player.position);
    }

    protected override void Attack()
    {
        // Stop the agent from moving.
        agent.isStopped = true;

        // Look at the player. We only care about the Y-axis rotation.
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0; // Keep the enemy upright.
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * agent.angularSpeed);

        // Check if the cooldown is over.
        if (fireCooldownTimer <= 0f)
        {
            FireProjectile();
            // Reset the cooldown timer.
            fireCooldownTimer = timeBetweenAttacks;
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("RangerEnemy is trying to fire, but projectilePrefab is not set!", this);
            return;
        }

        // Determine the spawn position and rotation.
        Vector3 spawnPos = shootOrigin != null ? shootOrigin.position : transform.position;
        Quaternion spawnRot = Quaternion.LookRotation((player.position - spawnPos).normalized);

        // Instantiate the projectile.
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, spawnRot);

        // Pass necessary info to the projectile script.
        EnemyProjectile projectileScript = projectileObj.GetComponent<EnemyProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(projectileSpeed, projectileDamage);
        }
        else
        {
            // Fallback for projectiles that just have a Rigidbody.
            Debug.LogWarning("Projectile prefab is missing an EnemyProjectile script. Firing with basic physics.", this);
            Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = projectileObj.transform.forward * projectileSpeed;
            }
        }
    }
}