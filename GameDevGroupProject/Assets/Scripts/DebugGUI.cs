using UnityEngine;

public class DebugGUI : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement pm;
    public Sliding slidingScript;
    public WallRunning wallRunningScript;
    public Climbing climbingScript;
    public Dashing dashingScript;

    Rigidbody rb;

    public void Start()
    {
        if (pm == null) pm = GetComponent<PlayerMovement>();
        if (slidingScript == null) slidingScript = GetComponent<Sliding>();
        if (wallRunningScript == null) wallRunningScript = GetComponent<WallRunning>();
        if (climbingScript == null) climbingScript = GetComponent<Climbing>();
        if (dashingScript == null) dashingScript = GetComponent<Dashing>();

        rb = GetComponent<Rigidbody>();
    }

    private void OnGUI()
    {
        // Safety checks
        if (pm == null)
        {
            GUILayout.Label("PlayerMovement (pm) is null");
            return;
        }
        if (rb == null)
        {
            GUILayout.Label("Rigidbody is null");
            return;
        }

        GUILayout.Label($"State: {pm.state}");
        GUILayout.Label($"isBoosted?: {pm.isBoosted}");

        // Speed from the Rigidbody (flat speed)
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = flatVel.magnitude;
        GUILayout.Label($"Speed: {speed:F1}");
    }
}
