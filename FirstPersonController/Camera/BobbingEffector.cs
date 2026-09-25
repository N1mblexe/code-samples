using UnityEngine;
using Data;

public class BobbingEffector : MonoBehaviour, ICameraEffector
{
    static public BobbingEffector Instance;
    void Awake()
    {
        Instance = this;
    }

    [SerializeField]private bool bobbing = false;
    private float distanceTraveled = 0f; 

    public void SetBobbing(bool bobbing)
    {
        this.bobbing = bobbing;
    }

    [Header("Position Bobbing")]
    [SerializeField] private Vector3 positionAmplitude = new Vector3(0.02f, 0.05f, 0f);

    [Header("Rotation Bobbing")]
    [SerializeField] private Vector3 rotationAmplitude = new Vector3(0.5f, 0f, 0.2f);

    [Header("Movement Timing")]
    [SerializeField] private float stepSpeed = 12f;

    private const float SwaySpeedMultiplier = 0.5f;

    void Update()
    {
        if (bobbing)
        {
            distanceTraveled += Movement.playerVelocity.magnitude * Time.deltaTime;
        }
    }

    public Vector3 GetPositionOffset()
    {
        if (!bobbing || !Movement.isGrounded) 
            return Vector3.zero;

        float speed = Movement.playerVelocity.magnitude;

        float swaySpeed = stepSpeed * SwaySpeedMultiplier;

        float xOffset = Mathf.Cos(distanceTraveled * swaySpeed) * positionAmplitude.x;
        float yOffset = Mathf.Sin(distanceTraveled * stepSpeed) * positionAmplitude.y;
        float zOffset = Mathf.Sin(distanceTraveled * stepSpeed) * positionAmplitude.z;

        return new Vector3(xOffset, yOffset, zOffset);
    }

    public Vector3 GetRotationOffset()
    {
        if (!bobbing ||  !Movement.isGrounded) 
            return Vector3.zero;

        float speed = Movement.playerVelocity.magnitude;

        float swaySpeed = stepSpeed * SwaySpeedMultiplier;

        float pitchOffset = Mathf.Cos(distanceTraveled * stepSpeed) * rotationAmplitude.x;
        float yawOffset = Mathf.Cos(distanceTraveled * swaySpeed) * rotationAmplitude.y;
        float rollOffset = Mathf.Cos(distanceTraveled * swaySpeed) * rotationAmplitude.z;

        return new Vector3(pitchOffset, yawOffset, rollOffset);
    }
}