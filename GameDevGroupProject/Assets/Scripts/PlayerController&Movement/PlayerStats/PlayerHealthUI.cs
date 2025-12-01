using UnityEngine;
using TMPro; 

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerStatsManager playerStats;
    [SerializeField] private TextMeshProUGUI healthText;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged.AddListener(UpdateHealthText);
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged.RemoveListener(UpdateHealthText);
        }
    }

    private void UpdateHealthText(float currentHealth)
    {
        healthText.text = $"Health: {currentHealth}";
    }
}
