using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Events")]
    // Useful for your UI teammate to update Health Bars without hard dependency
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;
        // Initialize UI
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        Debug.Log($"Player took {damageAmount} damage. Current Health: {currentHealth}");

        // Notify Listeners (like UI)
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        OnDeath?.Invoke();

        // Notify the GameManager to handle the Game Loop logic
        GameManager.Instance.PlayerDied();
    }
}