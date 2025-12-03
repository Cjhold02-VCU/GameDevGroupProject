using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This is the abstract base class for all enemies in the game.
/// It provides the core functionality that every enemy shares, such as health,
/// AI state detection (sight/attack range), and the damage/death sequence.
/// It cannot be attached to a GameObject directly. Instead, you create child
/// scripts (like RangerEnemy, SwarmEnemy) that inherit from this.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))] // Ensures any enemy has a NavMeshAgent
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Base Stats")]
    [Tooltip("Maximum health for this enemy.")]
    public float maxHealth = 100f;
    [SerializeField, Tooltip("Current health of the enemy.")]
    protected float currentHealth;

    [Header("Base AI Behavior")]
    [Tooltip("The range at which the enemy will detect the player and start chasing.")]
    public float sightRange;
    [Tooltip("The range at which the enemy will stop chasing and start attacking.")]
    public float attackRange;

    // We don't need whatIsGround, but whatIsPlayer is crucial.
    public LayerMask whatIsPlayer;

    [Header("Base References")]
    [Tooltip("The NavMeshAgent component for movement.")]
    [SerializeField] protected NavMeshAgent agent;
    [Tooltip("The player transform. Found automatically.")]
    protected Transform player;

    // State flags for the AI
    protected bool playerInSightRange, playerInAttackRange;
    private bool isDying = false;

    #region Unity Lifecycle Methods

    // 'virtual' allows child classes to add to this method without replacing it.
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        // Safely find the player by tag.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            // If the player isn't found, this enemy disables itself to prevent errors.
            Debug.LogError($"CRITICAL: Enemy '{gameObject.name}' could not find the Player. Make sure the Player object has the 'Player' tag.", this);
            enabled = false;
            return;
        }
    }

    protected virtual void Update()
    {
        // Don't run any logic if dying or if the player doesn't exist.
        if (isDying || player == null) return;

        // Check the current state based on distance to the player.
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // --- Core AI State Machine ---
        if (!playerInSightRange && !playerInAttackRange)
        {
            Patrol();
        }
        if (playerInSightRange && !playerInAttackRange)
        {
            Chase();
        }
        if (playerInAttackRange && playerInSightRange)
        {
            Attack();
        }
    }

    #endregion

    #region Core AI Behaviors (Abstract)

    /// <summary>
    /// Defines the enemy's behavior when it has not detected the player.
    /// This MUST be implemented by child classes.
    /// </summary>
    protected abstract void Patrol();

    /// <summary>
    /// Defines the enemy's behavior when it has detected the player but is not in attack range.
    /// This MUST be implemented by child classes.
    /// </summary>
    protected abstract void Chase();

    /// <summary>
    /// Defines the enemy's behavior when it is in attack range of the player.
    /// This MUST be implemented by child classes.
    /// </summary>
    protected abstract void Attack();

    #endregion

    #region Health and Damage

    // This method is 'public' because other scripts (like projectiles) need to call it.
    public void TakeDamage(float damageAmount)
    {
        if (isDying) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 'virtual' allows child classes to have special death effects (like exploding).
    protected virtual void Die()
    {
        isDying = true;

        // Safely shut down the NavMeshAgent to prevent memory leaks.
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        agent.enabled = false;

        // Disable colliders so the dead body doesn't block things.
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }

        // This will automatically notify the WaveManager thanks to EnemyNotifier.
        // Destroy the object after a short delay to ensure all cleanup is processed.
        Destroy(gameObject, 0.1f);
    }

    #endregion

    #region Gizmos

    // This is a great tool for your team to visualize the AI's ranges in the editor.
    protected virtual void OnDrawGizmosSelected()
    {
        // Draw the sight range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Draw the attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #endregion
}