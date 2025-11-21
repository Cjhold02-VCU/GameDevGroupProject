using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Group ID for this spawn point (e.g., 'Ground', 'Air', 'Boss'). Leave empty for generic.")]
    public string spawnID;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 2);
    }
}