using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A sniper enemy that stays at a distance, uses a laser sight to track the player,
/// and fires a hitscan shot after a delay. Inherits from EnemyBase.
/// </summary>
public class SniperEnemy : EnemyBase
{
    [Header("Sniper Behavior")]
    [Tooltip("How far the sniper will move from its post when patrolling.")]
    public float patrolRange = 5f;
    [Tooltip("The ideal distance the sniper wants to keep from the player.")]
    public float idealEngagementDistance = 30f;

    [Header("Hitscan Attack")]
    [Tooltip("The damage dealt by the sniper's hitscan shot.")]
    public float hitscanDamage = 30f;
    [Tooltip("Time between shots.")]
    public float timeBetweenAttacks = 5f;
    [Tooltip("Point where the laser and shot originate from (e.g., the gun's barrel).")]
    public Transform shootOrigin;
    [Tooltip("The prefab for the visible laser shot effect.")]
    public GameObject laserShotEffectPrefab;
    [Tooltip("Layers the visual effect will collide with (e.g., walls). This should EXCLUDE the Player layer.")] // <-- NEW
    public LayerMask laserVisualHitMask; // <-- NEW

    [Header("Aiming System")]
    [Tooltip("The LineRenderer component for the laser sight.")]
    public LineRenderer aimLaser;
    [Tooltip("How fast the laser sight tracks the player's movement. Higher is faster.")]
    public float aimTrackingSpeed = 5f;
    [Tooltip("The time the laser is on the player before firing.")]
    public float chargeUpTime = 2f;

    // --- Private State Variables ---
    private Coroutine attackCoroutine;
    private Vector3 currentAimPosition;
    private float fireCooldownTimer;

    // --- Patroling ---
    private Vector3 walkPoint;
    private bool walkPointSet;

    protected override void Awake()
    {
        base.Awake();

        if (aimLaser == null)
            aimLaser = GetComponent<LineRenderer>();

        if (aimLaser != null)
            aimLaser.enabled = false;
        else
            Debug.LogError("SniperEnemy needs a LineRenderer component for its laser sight!", this);

        currentAimPosition = transform.position + transform.forward * 10f;
    }

    protected override void Update()
    {
        base.Update();

        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

        if (!(playerInAttackRange && playerInSightRange))
        {
            if (aimLaser != null && aimLaser.enabled)
            {
                StopAttack();
            }
        }
    }

    // ... (Patrol, SearchWalkPoint, and Chase methods are unchanged) ...
    protected override void Patrol()
    {
        StopAttack();
        if (agent.isStopped) agent.isStopped = false;
        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet) agent.SetDestination(walkPoint);
        if (Vector3.Distance(transform.position, walkPoint) < 1f) walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-patrolRange, patrolRange);
        float randomX = Random.Range(-patrolRange, patrolRange);
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        if (NavMesh.SamplePosition(walkPoint, out _, 1.0f, NavMesh.AllAreas))
            walkPointSet = true;
    }

    protected override void Chase()
    {
        StopAttack();
        if (agent.isStopped) agent.isStopped = false;
        Vector3 directionToPlayer = transform.position - player.position;
        Vector3 targetPosition = player.position + directionToPlayer.normalized * idealEngagementDistance;
        agent.SetDestination(targetPosition);
    }

    protected override void Attack()
    {
        if (fireCooldownTimer <= 0f && attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(AttackSequence());
        }
        else if (attackCoroutine == null)
        {
            FacePlayer();
        }
    }

    private IEnumerator AttackSequence()
    {
        agent.isStopped = true;
        if (aimLaser != null) aimLaser.enabled = true;
        float chargeTimer = 0f;

        while (chargeTimer < chargeUpTime)
        {
            if (!playerInAttackRange)
            {
                StopAttack();
                yield break;
            }

            FacePlayer();
            UpdateAimLaser();

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        if (aimLaser != null) aimLaser.enabled = false;
        FireHitscanShot();

        fireCooldownTimer = timeBetweenAttacks;
        attackCoroutine = null;
    }

    private void FireHitscanShot()
    {
        // --- SETUP ---
        Vector3 origin = shootOrigin != null ? shootOrigin.position : transform.position;
        Vector3 direction = (currentAimPosition - origin).normalized;

        // --- 1. DAMAGE CALCULATION RAYCAST ---
        // This raycast can hit anything, including the player.
        if (Physics.Raycast(origin, direction, out RaycastHit damageHit, sightRange))
        {
            Debug.Log($"Sniper shot hit: {damageHit.collider.name} on layer {LayerMask.LayerToName(damageHit.collider.gameObject.layer)}", damageHit.collider.gameObject);

            // Change GetComponent to GetComponentInParent. This will find the IDamageable script
            // even if the ray hits a child collider of the main player object.
            IDamageable target = damageHit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(hitscanDamage);
            }
        }

        // --- 2. VISUAL EFFECT RAYCAST ---
        Vector3 visualEndPoint = origin + direction * sightRange; // Default end point if visual ray hits nothing.

        // This raycast uses the new LayerMask to IGNORE the player.
        if (Physics.Raycast(origin, direction, out RaycastHit visualHit, sightRange, laserVisualHitMask))
        {
            // The laser beam will end at the point where it hits a wall, etc.
            visualEndPoint = visualHit.point;
        }

        // --- 3. INSTANTIATE THE VISUAL EFFECT ---
        if (laserShotEffectPrefab != null)
        {
            GameObject shotEffectObj = Instantiate(laserShotEffectPrefab, origin, Quaternion.identity);
            LaserShotEffect effect = shotEffectObj.GetComponent<LaserShotEffect>();
            if (effect != null)
            {
                // Show the laser from the origin to the visual end point.
                effect.Show(origin, visualEndPoint);
            }
        }
    }

    private void StopAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        if (aimLaser != null && aimLaser.enabled)
        {
            aimLaser.enabled = false;
        }
    }

    private void FacePlayer()
    {
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * agent.angularSpeed);
    }

    private void UpdateAimLaser()
    {
        if (aimLaser == null) return;

        Vector3 playerTargetPos = player.position + new Vector3(0, 1, 0);
        currentAimPosition = Vector3.Lerp(currentAimPosition, playerTargetPos, Time.deltaTime * aimTrackingSpeed);

        Vector3 origin = shootOrigin != null ? shootOrigin.position : transform.position;
        aimLaser.SetPosition(0, origin);
        aimLaser.SetPosition(1, currentAimPosition);
    }

    protected override void Die()
    {
        StopAttack();
        base.Die();
    }
}