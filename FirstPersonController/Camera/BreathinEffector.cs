using UnityEngine;

public class BreathingEffector : MonoBehaviour, ICameraEffector 
{
    [Header("Position Rhythm")]
    [SerializeField] private Vector2 positionAmplitude = new Vector2(0.005f, 0.015f);
    
    [Header("Rotation Rhythm (Euler Angles)")]
    [SerializeField] private Vector3 rotationAmplitude = new Vector3(0.5f, 0.2f, 0.1f);

    [Header("Timing")]
    [SerializeField] private float speed = 1.2f;

    [Header("Organic Variance")]
    [SerializeField, Range(0f, 1f)] private float randomness = 0.25f;
    [SerializeField] private float noiseSpeed = 0.8f;

    private const float SecondarySpeedMultiplier = 0.5f;
    
    private const float NoiseOffsetX = 100f;
    private const float NoiseOffsetRotX = 200f;
    private const float NoiseOffsetRotY = 300f;
    private const float NoiseOffsetRotZ = 400f;

    public Vector3 GetPositionOffset() 
    {
        float time = Time.time;

        float baseSineY = Mathf.Sin(time * speed);
        float baseCosX = Mathf.Cos(time * speed * SecondarySpeedMultiplier);

        float noiseY = (Mathf.PerlinNoise(time * noiseSpeed, 0f) * 2f) - 1f;
        float noiseX = (Mathf.PerlinNoise(time * noiseSpeed, NoiseOffsetX) * 2f) - 1f;

        float finalY = Mathf.Lerp(baseSineY, noiseY, randomness) * positionAmplitude.y;
        float finalX = Mathf.Lerp(baseCosX, noiseX, randomness) * positionAmplitude.x;
        
        return new Vector3(finalX, finalY, 0f);
    }

    public Vector3 GetRotationOffset()
    {
        float time = Time.time;

        float baseRotX = Mathf.Sin(time * speed);
        float baseRotY = Mathf.Cos(time * speed * SecondarySpeedMultiplier);
        float baseRotZ = Mathf.Sin(time * speed * SecondarySpeedMultiplier);

        float noiseRotX = (Mathf.PerlinNoise(time * noiseSpeed, NoiseOffsetRotX) * 2f) - 1f;
        float noiseRotY = (Mathf.PerlinNoise(time * noiseSpeed, NoiseOffsetRotY) * 2f) - 1f;
        float noiseRotZ = (Mathf.PerlinNoise(time * noiseSpeed, NoiseOffsetRotZ) * 2f) - 1f;

        float finalRotX = Mathf.Lerp(baseRotX, noiseRotX, randomness) * rotationAmplitude.x;
        float finalRotY = Mathf.Lerp(baseRotY, noiseRotY, randomness) * rotationAmplitude.y;
        float finalRotZ = Mathf.Lerp(baseRotZ, noiseRotZ, randomness) * rotationAmplitude.z;

        return new Vector3(finalRotX, finalRotY, finalRotZ);
    }
}