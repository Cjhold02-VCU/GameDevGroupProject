using System.Collections;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float climbSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Physics Materials")]
    public PhysicsMaterial regularFrictionMaterial;
    public PhysicsMaterial frictionlessMaterial;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Air Jumps")]
    public int maxAirJumps = 1;
    public float airJumpForceBoostPerc;
    public float airJumpCooldown;
    private int airJumpsLeft;
    [HideInInspector]
    public bool readyToAirJump;

    [Header("Bunny Hopping")]
    public float slideBoostForce;
    public float maxSlideJumpSpeed = 30f;
    public float boostedAirDrag = 1f;
    [HideInInspector]
    public bool isBoosted;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode airJumpKey = KeyCode.F;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("References")]
    public Sliding slidingScript;
    public WallRunning wallRunningScript;
    public Climbing climbingScript;
    public CapsuleCollider c_collider;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        climbing,
        crouching,
        sliding,
        air
    }

    public bool sliding;
    public bool crouching;
    public bool wallrunning;
    public bool climbing;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Assign the regular material at start
        c_collider.material = regularFrictionMaterial;

        slidingScript = GetComponent<Sliding>();
        wallRunningScript = GetComponent<WallRunning>();
        climbingScript = GetComponent<Climbing>();

        readyToJump = true;
        readyToAirJump = false;
        airJumpsLeft = maxAirJumps;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * .5f + .2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (sliding)
        {
            // Drag is handled entirely by Sliding.cs
            // Do nothing here.
        }
        else if (grounded) // on the ground
        {
            rb.linearDamping = groundDrag;
        }
        else // in air
        {
            if (isBoosted) // If Boosted and in air, use boosted air drag.
            {
                rb.linearDamping = boostedAirDrag;
            }
            else // Otherwise, no drag in the air   
            {
                rb.linearDamping = 0;
            }
        }

        // Reset air jumps when on the ground, climbing, or wallrunning
        if (grounded || wallrunning || climbing)
        {
            if (airJumpsLeft != maxAirJumps)
                airJumpsLeft = maxAirJumps;
            
            // Turn off readyToAirJump when on ground or wall
            readyToAirJump = false;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // When to Ground Jump:
        // On ground AND NOT sliding
        if (Input.GetKey(jumpKey) && readyToJump && grounded && !sliding)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
            Invoke(nameof(ResetAirJump), airJumpCooldown);
        }

        // When to Slide Jump (Bunny Hop):
        // On ground AND sliding
        else if (Input.GetKey(jumpKey) && readyToJump && grounded && sliding)
        {
            readyToJump = false;

            // Call a new, dedicated function for clarity
            SlideJump();

            Invoke(nameof(ResetJump), jumpCooldown);
            Invoke(nameof(ResetAirJump), airJumpCooldown);
        }

        // When to Air Jump:
        // In Air AND NOT sliding
        else if (Input.GetKey(airJumpKey) && readyToAirJump && !grounded && (airJumpsLeft > 0) && !sliding)
        {
            readyToAirJump = false;

            AirJump();

            Invoke(nameof(ResetAirJump), airJumpCooldown);
        }

        // Start Crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // Stop Crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        // Mode - Climbing
        if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }

        // Mode - Wallrunning
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            // If we are slide-jump boosted, our desired speed is our CURRENT speed.
            if (isBoosted)
            {
                // Set desiredMoveSpeed to a high value to prevent SpeedControl from interfering.
                // We use the absolute cap to ensure consistency.
                desiredMoveSpeed = maxSlideJumpSpeed;
            }
            else
            {
                // Original logic for normal (non-boosted) sliding
                if (OnSlope() && rb.linearVelocity.y < 0.1f)
                    desiredMoveSpeed = slideSpeed;
                else
                    desiredMoveSpeed = sprintSpeed;
            }
        }

        // Mode - Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            Debug.Log("desiredMoveSpeed has changed drastically. Calling SmoothlyLerpMoveSpeed()");

            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (climbingScript.exitingWall) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // If we are sliding while boosted, don't apply regular movement forces.
        if (isBoosted && sliding) return;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        if (!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // If we are boosted, we have a special set of rules
        if (isBoosted)
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Enforce the absolute speed cap for the entire slide-jump chain
            if (flatVel.magnitude > maxSlideJumpSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * maxSlideJumpSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }

            // We only turn off the boost if our speed drops below the 'normal' speed for our current state.
            // This prevents the boost from being cancelled instantly when jumping from a slide.
            if (flatVel.magnitude < 1.0f)
            {
                isBoosted = false;
            }

            // If we are still boosted after that check, we skip ALL other speed limiting.
            // This allows the bunny hop to maintain momentum.
            if (isBoosted)
                return;
        }

        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velicoty if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void SlideJump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Apply normal jump force
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        // --- Apply the Boost Logic ---
        Debug.Log("Slide Jump!");
        isBoosted = true;

        rb.AddForce(moveDirection.normalized * slideBoostForce, ForceMode.Impulse);

        slidingScript.StopSlide();
    }
    private void AirJump()
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * (jumpForce + (jumpForce * airJumpForceBoostPerc)), ForceMode.Impulse);
        
        airJumpsLeft--;
    }
    public void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    public void ResetAirJump()
    {
        readyToAirJump = true;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * .5f + .3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public void StartBoostedSlide()
    {
        c_collider.material = frictionlessMaterial;
    }
    public void StopBoostedSlide()
    {
        c_collider.material = regularFrictionMaterial;
    }
}