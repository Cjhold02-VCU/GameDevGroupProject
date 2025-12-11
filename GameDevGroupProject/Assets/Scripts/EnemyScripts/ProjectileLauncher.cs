using UnityEngine;
using UnityEngine.Events;

public class ProjectileLauncher : MonoBehaviour
{
    // --- We are simplifying this. No more SphereCast option. ---
    [Header("Hitscan Settings")]
    public float range = 75f;
    public float damage = 25f;
    public LayerMask hitMask = ~0; // default: all layers
    public GameObject impactEffectPrefab; // optional VFX prefab for impact
    public float impactForce = 0.1f; // force applied to rigidbodies on hit
    // public float hitSphereCastRadius = 0f; // REMOVED

    [Header("Gun Stats")]
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    private int bulletsLeft, bulletsShot;
    private bool shooting, readyToShoot, reloading;

    [Header("References")]
    public Camera fpsCam;
    public Transform attackPoint;
    public GameObject muzzleFlash;

    [Header("Events")]
    public UnityEvent<int> OnAmmoChanged;

    private bool allowInvoke = true;

    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        OnAmmoChanged?.Invoke(bulletsLeft);
    }

    private void Update()
    {
        MyInput();
    }

    private void MyInput()
    {
        shooting = allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
            Reload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        // We play the sound effect here. The name must match what you set in the inspector.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("Gunshot");
        }

        // --- SIMPLIFIED RAYCAST LOGIC ---

        // 1. Calculate direction with spread
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 spreadDirection = ray.direction; // Start with the center direction

        // Add random spread
        float spreadX = Random.Range(-spread, spread);
        float spreadY = Random.Range(-spread, spread);
        spreadDirection += new Vector3(spreadX, spreadY, 0);

        // 2. Perform the Raycast
        if (Physics.Raycast(fpsCam.transform.position, spreadDirection, out RaycastHit hit, range, hitMask))
        {
            // --- HANDLE THE HIT ---
            IDamageable damageableTarget = hit.collider.GetComponent<IDamageable>();
            if (damageableTarget != null)
                damageableTarget.TakeDamage(damage);

            Rigidbody rb = hit.rigidbody ?? hit.collider.attachedRigidbody;
            if (rb != null)
                rb.AddForce(-hit.normal * impactForce, ForceMode.Impulse);

            if (impactEffectPrefab != null)
                Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        // --- END OF SIMPLIFIED LOGIC ---

        // Muzzle flash - This can stay the same
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        bulletsLeft--;
        bulletsShot++;

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
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("Reload");
        }

        Invoke(nameof(ReloadFinish), reloadTime);
    }

    private void ReloadFinish()
    {
        bulletsLeft = magazineSize;
        reloading = false;
        OnAmmoChanged?.Invoke(bulletsLeft);
    }

    public int GetBulletsLeft() => bulletsLeft;
}