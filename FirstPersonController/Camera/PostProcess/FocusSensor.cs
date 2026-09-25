using UnityEngine;
using UnityEngine.Events;

public class FocusSensor : MonoBehaviour
{
    [Header("Sensor Settings")]
    public LayerMask focusLayer = ~0;
    public float maxDistance = 100f;
    [Tooltip("How often to check focus (0.1s is usually smooth enough and saves performance)")]
    public float updateInterval = 0.1f; 
    
    [Header("FPS Tuning")]
    public bool useSphereCast = true;
    public float sphereRadius = 0.5f;

    [Header("Events")]
    public UnityEvent<float> OnFocusDistanceCalculated;

    private float _timer;
    private float _lastSentDistance = -1f;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= updateInterval)
        {
            _timer = 0f;
            CalculateFocusDistance();
        }
    }

    private void CalculateFocusDistance()
    {
        float targetDistance = maxDistance;
        Ray ray = new Ray(transform.position, transform.forward);

        if (useSphereCast)
        {
            if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, maxDistance, focusLayer))
                targetDistance = hit.distance;
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, focusLayer))
                targetDistance = hit.distance;
        }

        if (Mathf.Abs(_lastSentDistance - targetDistance) > 0.1f)
        {
            _lastSentDistance = targetDistance;
            OnFocusDistanceCalculated?.Invoke(targetDistance);
        }
    }
}