using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 5f;

    void Start() => Destroy(gameObject, lifetime);

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Bullet collided with {other.name}");
        var enemy = other.GetComponent<EnemyScript>() ?? other.GetComponentInParent<EnemyScript>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Damaged {enemy.name} for {damage}");
            Destroy(gameObject);
            return;
        }
        // Optionally destroy on hitting environment:
        Destroy(gameObject);
    }
}