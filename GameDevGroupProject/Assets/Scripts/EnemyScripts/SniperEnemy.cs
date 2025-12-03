using UnityEngine;
using UnityEngine.AI;

public class SniperEnemy : EnemyBase
{
    [Header("Sniper Specifics")]
    public float walkPointRange = 2f;

    [Header("Combat Settings")]
    public float timeBetweenAttacks = 10f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 100f;
    public float projectileDamage = 20f;
    public Transform shootOrigin;

    [Header("Detection Settings")]
    [Tooltip("Layers considered obstacles (e.g., walls).")]
    public LayerMask obstructionMask;
    private Vector3 walkPoint;
    private bool walkPointSet;
    private float fireCooldownTimer;

    protected override void Update()
    {
        base.Update();

        if (fireCooldownTimer > 0)
            fireCooldownTimer -= Time.deltaTime;

        // Decide behavior based on line of sight
        if (PlayerInSight())
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= agent.stoppingDistance * 2f) // close enough to attack
                Attack();
            else
                Chase();
        }
        else
        {
            Patrol();
        }
    }

    private bool PlayerInSight()
    {
        Vector3 origin = shootOrigin != null ? shootOrigin.position : transform.position;
        Vector3 direction = (player.position - origin).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, sightRange, ~0))
        {
            // If the ray hits the player directly, we have line of sight
            if (hit.transform.CompareTag("Player"))
                return true;
        }
        return false;
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

        if (NavMesh.SamplePosition(walkPoint, out _, 1.0f, NavMesh.AllAreas))
            walkPointSet = true;
    }

    protected override void Chase()
    {
        if (agent.isStopped) agent.isStopped = false;
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
            Debug.LogError("SniperEnemy is trying to fire, but projectilePrefab is not set!", this);
            return;
        }
        
        // Determine the spawn position and rotation.
        Vector3 spawnPos = shootOrigin != null ? shootOrigin.position : transform.position;
        Quaternion spawnRot = Quaternion.LookRotation((player.position - spawnPos).normalized);

        // Instantiate the projectile.
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, spawnRot);

        EnemyProjectile projectileScript = projectileObj.GetComponent<EnemyProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(projectileSpeed, projectileDamage);
        }
        else
        {
            Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = projectileObj.transform.forward * projectileSpeed;
        }
    }
}