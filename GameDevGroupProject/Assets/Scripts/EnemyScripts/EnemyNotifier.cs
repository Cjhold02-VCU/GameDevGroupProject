using DG.Tweening.Core.Easing;
using UnityEngine;

public class EnemyNotifier : MonoBehaviour
{
    // Call this when your enemy dies (e.g., inside their TakeDamage or Die function)
    // Since I don't know your teammate's code, they can either call this method manually
    // or you can put this in OnDestroy() as a failsafe.

    private bool hasNotified = false;

    public void NotifyDeath()
    {
        if (hasNotified) return;

        hasNotified = true;
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyKilled();
        }
    }

    // Failsafe: If the object is destroyed and we haven't notified, do it now.
    private void OnDestroy()
    {
        NotifyDeath();
    }
}