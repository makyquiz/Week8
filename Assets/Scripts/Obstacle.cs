using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Vector3 itemPosition;
    public float speed = 10f;
    public static CameraComponent CameraComponent;
    public bool hasDamagedPlayer = false;

    [Header("Depth Illusion Settings")]
    [Range(0f, 0.5f)] public float warpStrength = 0.25f; // fisheye curve amount
    public float scaleMultiplier = 2f; // affects how large they look when close

    void Awake()
    {
        if (CameraComponent == null)
            CameraComponent = FindObjectOfType<CameraComponent>();
    }

    void Update()
    {
        // Move forward in depth
        itemPosition.z -= speed * Time.deltaTime;

        // If it's too far behind camera, destroy it
        if (itemPosition.z < -20f)
        {
            Destroy(gameObject);
            return;
        }

        // Compute depth-based perspective
        float depth = CameraComponent.focalLenth + itemPosition.z;
        if (depth <= 0.01f || float.IsNaN(depth)) return;

        float perspective = CameraComponent.focalLenth / depth;
        perspective = Mathf.Clamp(perspective, 0.01f, 2f);

        // ✅ Fisheye-like warp for curved approach
        float zNorm = Mathf.Clamp01(1f - (itemPosition.z / 300f)); // normalize between spawnZ (300) → 0
        float curve = Mathf.Pow(zNorm, 2f) * warpStrength;

        float warpedX = itemPosition.x * (1f + curve);
        float warpedY = itemPosition.y - curve * 3f; // shift slightly downward as it gets closer

        // Apply final 2D projection
        transform.localScale = Vector3.one * perspective * scaleMultiplier;
        transform.position = new Vector2(warpedX, warpedY) * perspective;
    }
}
