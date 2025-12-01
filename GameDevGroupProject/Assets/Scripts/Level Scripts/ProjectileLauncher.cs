using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ProjectileLauncher : MonoBehaviour
{
    // Hitscan Settings
    public float range = 75f;
    public float damage = 25f;
    public LayerMask hitMask = ~0; // default: all layers
    public GameObject impactEffectPrefab; // optional VFX prefab for impact
    public float impactForce = 0.1f; // force applied to rigidbodies on hit
    public float hitSphereCastRadius = 0f; // 0 = single ray; >0 = use SphereCast

    // Gun stats
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    private int bulletsLeft, bulletsShot;

    // State flags
    private bool shooting, readyToShoot, reloading;

    // References
    public Camera fpsCam;
    public Transform attackPoint;

    // Graphics
    public GameObject muzzleFlash;

    // Events
    [Header("Events")]
    public UnityEvent<int> OnAmmoChanged; // current ammo only

    // BUG FIXING
    public bool allowInvoke = true;

    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;

        // Initialize UI
        OnAmmoChanged?.Invoke(bulletsLeft);
    }

    private void Update()
    {
        MyInput();
    }

    private void MyInput()
    {
        // Shooting input
        shooting = allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

        // Reload input
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
            Reload();

        // Shooting logic
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        // Find hit position
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint = Physics.Raycast(ray, out hit, range, hitMask) ? hit.point : ray.GetPoint(range);

        // Direction with spread
        Vector3 directionWithoutSpread = (targetPoint - attackPoint.position).normalized;
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        // Perform raycast/spherecast
        bool didHit = false;
        RaycastHit finalHit;
        if (hitSphereCastRadius > 0f)
            didHit = Physics.SphereCast(attackPoint.position, hitSphereCastRadius, directionWithSpread, out finalHit, range, hitMask);
        else
            didHit = Physics.Raycast(attackPoint.position, directionWithSpread, out finalHit, range, hitMask);

        // Muzzle flash
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        // Handle hit
        if (didHit)
        {
            IDamageable damageableTarget = finalHit.collider.GetComponent<IDamageable>();
            if (damageableTarget != null)
                damageableTarget.TakeDamage(damage);

            Rigidbody rb = finalHit.rigidbody ?? finalHit.collider.attachedRigidbody;
            if (rb != null)
                rb.AddForce(-finalHit.normal * impactForce, ForceMode.Impulse);

            if (impactEffectPrefab != null)
                Instantiate(impactEffectPrefab, finalHit.point, Quaternion.LookRotation(finalHit.normal));
        }

        bulletsLeft--;
        bulletsShot++;

        // Notify listeners
        Debug.Log($"Ammo left: {bulletsLeft}");

        OnAmmoChanged?.Invoke(bulletsLeft);

        if (allowInvoke)
        {
            Invoke(nameof(ResetShot), timeBetweenShooting);
            allowInvoke = false;
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinish), reloadTime);
    }

    private void ReloadFinish()
    {
        bulletsLeft = magazineSize;
        reloading = false;

        // Notify listeners
        OnAmmoChanged?.Invoke(bulletsLeft);
    }

    public int GetBulletsLeft() => bulletsLeft;
}