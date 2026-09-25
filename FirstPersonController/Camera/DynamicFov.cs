using Data;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DynamicFOV : MonoBehaviour
{
    [Header("FOV Settings")]
    public float baseFOV = 60f;
    public float maxFOV = 70f;

    [Header("Speed Thresholds")]
    public float speedForMaxFOV = 6f;

    [Header("Smoothing")]
    public float fovSmoothSpeed = 5f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        float currentSpeed = Movement.playerSpeed;

        float speedFactor = Mathf.Clamp01(currentSpeed / speedForMaxFOV);

        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedFactor);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSmoothSpeed * Time.deltaTime);
    }
}