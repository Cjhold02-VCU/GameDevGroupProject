using UnityEngine;

[ExecuteAlways]
public class AutoTextureTiling : MonoBehaviour
{
    public enum Orientation
    {
        Floor_XZ, // Use for floors or flat grounds (uses X and Z scale)
        Wall_XY   // Use for walls or vertical objects (uses X and Y scale)
    }

    [Tooltip("Is this object a Floor (flat) or a Wall (vertical)?")]
    public Orientation orientation = Orientation.Floor_XZ;

    [Tooltip("Set this to 1 for 1:1 world space tiling. Higher values make the texture smaller.")]
    public float textureDensity = 1.0f;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    // Cache values to prevent constant updates
    private Vector3 _lastScale;
    private float _lastDensity;
    private Orientation _lastOrientation;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        // Basic optimization: only update if something changed
        if (transform.lossyScale == _lastScale &&
            textureDensity == _lastDensity &&
            orientation == _lastOrientation) return;

        if (_renderer == null) return;

        _renderer.GetPropertyBlock(_propBlock);

        // We use TransformVector to get the correct size even if the object is rotated.
        // This calculates "How big is this object's X axis in world space?", etc.
        float scaleX = transform.TransformVector(Vector3.right).magnitude;
        float scaleY = transform.TransformVector(Vector3.up).magnitude;
        float scaleZ = transform.TransformVector(Vector3.forward).magnitude;

        float tileX = 1f;
        float tileY = 1f;

        if (orientation == Orientation.Floor_XZ)
        {
            // Floors use X and Z (Width and Length)
            tileX = scaleX * textureDensity;
            tileY = scaleZ * textureDensity;
        }
        else if (orientation == Orientation.Wall_XY)
        {
            // Walls use X and Y (Width and Height)
            tileX = scaleX * textureDensity;
            tileY = scaleY * textureDensity;
        }

        // Apply the Tiling
        _propBlock.SetVector("_BaseMap_ST", new Vector4(tileX, tileY, 0, 0));
        _renderer.SetPropertyBlock(_propBlock);

        // Cache values
        _lastScale = transform.lossyScale;
        _lastDensity = textureDensity;
        _lastOrientation = orientation;
    }
}