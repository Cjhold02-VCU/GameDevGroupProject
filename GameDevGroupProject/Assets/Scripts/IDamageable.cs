using UnityEngine;

// This interface allows enemies to attack the player OR other destructible objects
// without needing to know exactly what script is attached to them.
public interface IDamageable
{
    void TakeDamage(float damageAmount);
}