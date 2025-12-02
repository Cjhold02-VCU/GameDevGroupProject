using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour, IDamageable
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public float health;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    private float fireCooldown = 0f;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    // Hitscan Attack Stats
    public float damage = 10;
    public float shootRange = 50f;
    [Tooltip("Cone half-angle in degrees")]
    public float spreadAngleDegrees = 5f;
    public Transform shootOrigin; // optional: assign a child transform on the enemy for muzzle position

    // State flag to prevent multiple death calls
    private bool isDying = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError($"CRITICAL: Enemy '{gameObject.name}' could not find the Player. Make sure the Player object in your scene has the 'Player' tag.", this);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // Don't run update logic if the player is gone or we are dying
        if (player == null || isDying) return;

        // Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (agent.isStopped)
            agent.isStopped = false;

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, Vector3.down, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        if (agent.isStopped)
            agent.isStopped = false;

        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;
        transform.LookAt(player);

        if (fireCooldown > 0f) fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            ShootAtPlayer();
            fireCooldown = timeBetweenAttacks;
        }
    }

    private void ShootAtPlayer()
    {
        Vector3 origin = (shootOrigin != null) ? shootOrigin.position : transform.position;
        Vector3 dirToPlayer = (player.position - origin).normalized;
        Vector3 shotDir = GetDirectionWithSpread(dirToPlayer, spreadAngleDegrees);

        if (Physics.Raycast(origin, shotDir, out RaycastHit hit, shootRange))
        {
            Debug.DrawLine(origin, hit.point, Color.red, 0.5f);

            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        // Don't take damage if already dying
        if (isDying) return;

        health -= damageAmount;

        if (health <= 0)
        {
            // --- NEW SAFE DESTRUCTION ---
            isDying = true; // Set flag to stop Update() and prevent multiple calls

            // 1. Disable the NavMeshAgent. This is the crucial step to stop background jobs.
            if (agent.isOnNavMesh) // Only disable if it's active on a NavMesh
            {
                agent.isStopped = true;
                agent.ResetPath(); // Clear any existing path
            }
            agent.enabled = false; // Fully disable the component

            // 2. Disable colliders so the dead body doesn't block shots or the player
            foreach (Collider col in GetComponents<Collider>())
            {
                col.enabled = false;
            }

            // 3. The EnemyNotifier's OnDestroy() will still be called correctly.
            //    We can now safely destroy the GameObject.
            Destroy(gameObject, 0.1f); // A tiny delay can sometimes help ensure engine cleanup.
            // --- END SAFE DESTRUCTION ---
        }
    }

    // (This is a helper function to get spread, it doesn't need to change)
    private Vector3 GetDirectionWithSpread(Vector3 forward, float halfAngleDeg)
    {
        float halfAngleRad = halfAngleDeg * Mathf.Deg2Rad;
        float u = Random.value;
        float v = Random.value;
        float cosTheta = Mathf.Lerp(1f, Mathf.Cos(halfAngleRad), u);
        float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
        float phi = 2f * Mathf.PI * v;
        Vector3 localDir = new Vector3(sinTheta * Mathf.Cos(phi), sinTheta * Mathf.Sin(phi), cosTheta);
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, forward.normalized);
        return rot * localDir;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}