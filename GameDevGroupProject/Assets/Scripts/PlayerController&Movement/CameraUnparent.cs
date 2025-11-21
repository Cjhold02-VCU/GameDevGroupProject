using UnityEngine;

public class CameraUnparent : MonoBehaviour
{
    public Transform targetPos; // Assign the "CameraPos" child here

    void Start()
    {
        // Detach this object from the Player so it moves independently
        transform.parent = null;
    }

    void LateUpdate()
    {
        // Smoothly follow the target position
        // We use LateUpdate to ensure the player has finished moving for the frame
        transform.position = targetPos.position;
    }
}