using UnityEngine;
using TMPro;

public class PlayerAmmoUI : MonoBehaviour
{
    [SerializeField] private ProjectileLauncher weapon;
    [SerializeField] private TextMeshProUGUI ammoText;

    private void OnEnable()
    {
        if (weapon != null)
            weapon.OnAmmoChanged.AddListener(UpdateAmmoText);
    }

    private void OnDisable()
    {
        if (weapon != null)
            weapon.OnAmmoChanged.RemoveListener(UpdateAmmoText);
    }

    private void UpdateAmmoText(int current)
    {
        ammoText.text = $"Ammo: {current} / {weapon.magazineSize}";
        Debug.Log($"UI updated: {current} / {weapon.magazineSize}");

    }

    
}