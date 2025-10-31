using UnityEngine;

public class CameraComponent : MonoBehaviour
{
    [Header("Perspective Settings")]
    public static float focalLenth = 800f; // from 400 → 800 gives stronger depth illusion
    public Vector2 vanishingPoint = Vector2.zero;

    void OnValidate()
    {
        focalLenth = Mathf.Max(1f, focalLenth); // prevent invalid 0
    }
}
