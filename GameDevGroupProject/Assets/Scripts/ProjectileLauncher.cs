using UnityEngine;
using TMPro;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.UIElements;


public class ProjectileLauncher : MonoBehaviour
{
    // Hitscan Settings
    // Hitscan settings
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

    int bulletsLeft, bulletsShot;

    // bools
    bool shooting, readyToShoot, reloading;

    // References
    public Camera fpsCam;
    public Transform attackPoint;

    // Graphics
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;

    // BUG FIXING
    public bool allowInvoke = true;

    private void Awake()
    {
        // Make sure the magazine is full
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        MyInput();

        // Set ammo display, if it exists
        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);

    }

    private void MyInput()
    {   
        // Check if allowed to hold down button and take corresponding input
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        // Reloading
        // If press R and bullets left is less than mag size and not currently reloading
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) 
            Reload();

        // Reload automatically when trying to shoot without ammo
        // if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload;
        // Could be left out for hardcore element

        // If readyToShoot and shooting and not reloading and bullets left > 0
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        

        // Find the exact hit position
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Check if ray hits target
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, range, hitMask))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(range);

        // Calculate direction from attackpoint to targetPoint
        // Formula for a vector from point A to point B is always A - B
        Vector3 directionWithoutSpread = (targetPoint - attackPoint.position).normalized;

        // calculate spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // Calculate new direction with spread
        // Just add spread as x and y component to direction vector
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        // Perform raycast (or spherecast if configured)
        bool didHit = false;
        RaycastHit finalHit;

        if (hitSphereCastRadius > 0f)
        {
            if (Physics.SphereCast(attackPoint.position, hitSphereCastRadius, directionWithSpread, out finalHit, range, hitMask))
                didHit = true;
        }
        else
        {
            if (Physics.Raycast(attackPoint.position, directionWithSpread, out finalHit, range, hitMask))
                didHit = true;
        }

        // Spawn muzzle flash if assigned
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        // Handle hit
        if (didHit)
        {
            // Apply physics impact force if rigidbody present
            if (finalHit.rigidbody != null)
            {
                finalHit.rigidbody.AddForce(-finalHit.normal * impactForce, ForceMode.Impulse);
            }
            else
            {
                var rb = finalHit.collider.attachedRigidbody;
                if (rb != null)
                    rb.AddForce(-finalHit.normal * impactForce, ForceMode.Impulse);
            }

            // Spawn impact effect
            if (impactEffectPrefab != null)
            {
                Instantiate(impactEffectPrefab, finalHit.point, Quaternion.LookRotation(finalHit.normal));
            }
        }

        // Instantiate Muzzle Flash, if there is one
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        bulletsLeft--;
        bulletsShot++;

        // Invoke resetShot function (if not already invoked)
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        // If more than one bulletsPerTap make sure to repeat shoot function
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }


    private void ResetShot()
    {
        // Allow shooting and invoking again
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinish", reloadTime);
    }

    private void ReloadFinish()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
