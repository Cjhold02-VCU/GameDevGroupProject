using UnityEngine;
using UnityEngine.AI;
public class EnemyScript : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public float health;
    public GameObject projectile;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    private float fireCooldown = 0f;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    public int damage = 10;
    public float shootRange = 50f;
    [Tooltip("Cone half-angle in degrees")]
    public float spreadAngleDegrees = 5f;
    public Transform shootOrigin; // optional: assign a child transform on the enemy for muzzle position


    private void Awake()
    {
        player = GameObject.Find("PlayerObj").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        
        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }
    
    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Walkpoint Reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    public void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);


        if (Physics.Raycast(walkPoint, Vector3.down, 2f, whatIsGround))
            walkPointSet = true;   
    }   

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        // Make sure enemy doesnt move
        agent.SetDestination(transform.position);
        // Face the player
        transform.LookAt(player);
        // Count down cooldown
        if (fireCooldown > 0f) fireCooldown -= Time.deltaTime;


        if (fireCooldown <= 0f)
        {
            ShootAtPlayer();
            fireCooldown = timeBetweenAttacks;
        }


    }

    private Vector3 GetDirectionWithSpread(Vector3 forward, float halfAngleDeg)
    {
        // Convert cone half-angle to radians
        float halfAngleRad = halfAngleDeg * Mathf.Deg2Rad;

        // Sample a random point inside a cone by sampling a random direction on a unit sphere
        // then spherically interpolate toward the forward direction to constrain it inside the cone.
        float u = Random.value; // 0..1
        float v = Random.value; // 0..1

        // Uniform sample of direction within cone:
        float cosTheta = Mathf.Lerp(1f, Mathf.Cos(halfAngleRad), u);
        float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
        float phi = 2f * Mathf.PI * v;

        // Direction in local cone coordinates
        Vector3 localDir = new Vector3(sinTheta * Mathf.Cos(phi), sinTheta * Mathf.Sin(phi), cosTheta);

        // Build rotation that maps Vector3.forward to the desired forward vector
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, forward.normalized);
        return rot * localDir;
    }
    private void ShootAtPlayer()
    {
        Vector3 origin = (shootOrigin != null) ? shootOrigin.position : transform.position;
        Vector3 dirToPlayer = (player.position - origin).normalized;
        Vector3 shotDir = GetDirectionWithSpread(dirToPlayer, spreadAngleDegrees);

        // Exclude the enemy itself by using a layer mask, or check hit.collider != this.collider
        if (Physics.Raycast(origin, shotDir, out RaycastHit hit, shootRange))
        {
            Debug.DrawLine(origin, hit.point, Color.red, 0.5f);
            Debug.Log("Enemy shot hit: " + hit.collider.name + " (tag: " + hit.collider.tag + ")");

            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if (target != null)
            {
                target.TakeDamage(damage);
                Debug.Log("Hit a damageable target: " + hit.collider.name);
            }
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

}
