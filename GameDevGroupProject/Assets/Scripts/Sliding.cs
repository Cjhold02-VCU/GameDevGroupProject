using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        if (Input.GetKeyUp(slideKey) && pm.sliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        pm.sliding = true;

        // Check if we are starting a slide while already boosted (i.e., landing from a slide jump)
        if (pm.isBoosted)
        {
            pm.StartBoostedSlide();
        }

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        if (pm.isBoosted) // Handle Drag when boosted sliding
        {
            // If on a slope, apply boostedAirDrag to create a natural terminal velocity.
            if (pm.OnSlope())
            {
                rb.linearDamping = pm.boostedAirDrag;
            }
            // If on flat ground, use zero drag to perfectly conserve momentum.
            else
            {
                rb.linearDamping = 0;

                slideTimer -= Time.deltaTime;
            }

            // Do not apply any slideForce.
        }
        else // Normal Case (Not Slide Jumping / Boosted)
        {
            // For normal slides, always use the default ground drag.
            rb.linearDamping = pm.groundDrag;

            Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            // sliding normal
            if (!pm.OnSlope() || rb.linearVelocity.y > -.1f)
            {
                rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

                slideTimer -= Time.deltaTime;
            }

            // sliding down a slope
            else
            {
                rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
            }
        }
            
        if (slideTimer <= 0)
            StopSlide();
    }

    public void StopSlide()
    {
        pm.sliding = false;

        // Always revert to regular friction when a slide ends.
        pm.StopBoostedSlide();

        // Reset drag to 0 when sliding stops, letting PlayerMovement's logic take over.
        rb.linearDamping = 0;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}