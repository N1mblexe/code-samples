using Data;
using UnityEngine;

public class VerticalMomentumEffector : MonoBehaviour, ICameraEffector
{
    [Header("Airborne Settings")]
    public float verticalLagMultiplier = 0.04f;
    public float maxAirLag = 0.3f;

    [Header("Jump Wind-Up Settings")]
    public float windUpDropMultiplier = 0.15f;
    public float windUpPitchMultiplier = 2f;

    [Header("Impulse Settings")]
    public float landDropMultiplier = 0.05f;
    public float landPitchMultiplier = 1.5f;
    public float jumpPushMultiplier = 0.15f;
    
    [Header("Smoothing")]
    public float impulseDecaySpeed = 7f;
    public float finalSmoothness = 15f;

    private Vector3 currentPosOffset;
    private Vector3 currentRotOffset;
    private Vector3 impulsePos;
    private Vector3 impulseRot;
    
    private bool isWindingUp;

    void OnEnable()
    {
        Movement.OnJumpWindUp += HandleWindUp;
        Movement.OnJumped += HandleJump;
        Movement.OnLanded += HandleLand;
    }

    void OnDisable()
    {
        Movement.OnJumpWindUp -= HandleWindUp;
        Movement.OnJumped -= HandleJump;
        Movement.OnLanded -= HandleLand;
    }

    private void HandleWindUp()
    {
        isWindingUp = true;
    }

    private void HandleJump()
    {
        isWindingUp = false;
        impulsePos.y += jumpPushMultiplier;
        impulseRot.x -= windUpPitchMultiplier * 0.8f;
    }

    private void HandleLand(float impactForce)
    {
        if (impactForce > 2f)
        {
            float weightMultiplier = 1f + (impactForce / 10f); 

            impulsePos.y -= (landDropMultiplier * impactForce) * weightMultiplier;
            impulseRot.x += (landPitchMultiplier * impactForce) * weightMultiplier;

            if (impactForce > 12f)
            {
                impulseRot.z += Random.Range(-2f, 2f) * impactForce;
            }
        }
    }

    void Update()
    {
        Vector3 basePosTarget = Vector3.zero;
        Vector3 baseRotTarget = Vector3.zero;

        if (isWindingUp)
        {
            basePosTarget.y = -windUpDropMultiplier;
            baseRotTarget.x = windUpPitchMultiplier;
        }
        else if (!Movement.isGrounded)
        {
            float lagY = -Movement.playerVelocity.y * verticalLagMultiplier;
            basePosTarget.y = Mathf.Clamp(lagY, -maxAirLag, maxAirLag);
        }

        impulsePos = Vector3.Lerp(impulsePos, Vector3.zero, impulseDecaySpeed * Time.deltaTime);
        impulseRot = Vector3.Lerp(impulseRot, Vector3.zero, impulseDecaySpeed * Time.deltaTime);

        Vector3 finalPosTarget = basePosTarget + impulsePos;
        Vector3 finalRotTarget = baseRotTarget + impulseRot;

        currentPosOffset = Vector3.Lerp(currentPosOffset, finalPosTarget, finalSmoothness * Time.deltaTime);
        currentRotOffset = Vector3.Lerp(currentRotOffset, finalRotTarget, finalSmoothness * Time.deltaTime);
    }

    public Vector3 GetPositionOffset() => currentPosOffset;
    public Vector3 GetRotationOffset() => currentRotOffset;
}