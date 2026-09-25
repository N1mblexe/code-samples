using UnityEngine;

public class PlayerMouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 2f;

    [Header("References")]
    public Transform cameraRoot;

    [Header("Sway Settings")]
    [Tooltip("How much the camera drags behind. Negative values drag opposite to movement.")]
    public float swayMultiplier = -1.5f;

    [Tooltip("The absolute maximum angle the camera can sway to prevent neck-breaking.")]
    public float maxSwayAngle = 4f;

    [Tooltip("How fast the rubber band snaps the camera back to the center.")]
    public float smoothness = 8f;

    [Header("Vertical Look Limits")]
    [Tooltip("How far the player can look up (negative pitch).")]
    public float verticalMinAngle = -80f;

    [Tooltip("How far the player can look down (positive pitch).")]
    public float verticalMaxAngle = 80f;

    [Tooltip("Degrees before the limit where soft resistance kicks in.")]
    public float smoothClampZone = 15f;

    private Vector3 cameraSway;
    private float xRotation = 0f;
    private InputManager inputManager;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
        float lookX = inputManager.LookInput.x * mouseSensitivity * Time.deltaTime;
        float lookY = inputManager.LookInput.y * mouseSensitivity * Time.deltaTime;

        float proposedRotation = xRotation - lookY;
        lookY *= GetInputResistance(proposedRotation);
        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, verticalMinAngle, verticalMaxAngle); 

        float swayX = lookX * swayMultiplier * -1f;
        float swayY = lookY * swayMultiplier;

        swayX = Mathf.Clamp(swayX, -maxSwayAngle, maxSwayAngle);
        swayY = Mathf.Clamp(swayY, -maxSwayAngle, maxSwayAngle);

        Vector3 camTargetSway = new Vector3(swayY, 0f, 0f);

        cameraSway = Vector3.Lerp(cameraSway, camTargetSway, smoothness * Time.deltaTime);

        cameraRoot.localRotation = Quaternion.Euler(xRotation + cameraSway.x, 0f, 0f);

        transform.Rotate(Vector3.up * lookX);
        transform.Rotate(Vector3.up * (swayX * Time.deltaTime * smoothness), Space.Self);
    }

    private float GetInputResistance(float proposedRotation)
    {
        float zoneSize = smoothClampZone;

        if (proposedRotation > verticalMaxAngle - zoneSize)
        {
            float distanceToLimit = verticalMaxAngle - proposedRotation;
            float resistance = distanceToLimit / zoneSize;
            return Mathf.Clamp01(resistance); 
        }

        if (proposedRotation < verticalMinAngle + zoneSize)
        {
            float distanceToLimit = proposedRotation - verticalMinAngle;
            float resistance = distanceToLimit / zoneSize;
            return Mathf.Clamp01(resistance);
        }

        return 1f; 
    }
}