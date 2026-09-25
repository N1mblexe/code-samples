using UnityEngine;
using UnityEngine.Rendering.PostProcessing; 

[RequireComponent(typeof(PostProcessVolume))]
public class DoFController : MonoBehaviour
{
    [Header("Lerp Settings")]
    [Tooltip("Higher values pull focus faster. Lower values are slower and more cinematic.")]
    public float focusSpeed = 10f; 

    private PostProcessVolume _volume;
    private DepthOfField _dof;
    private float _targetDistance;

    void Awake()
    {
        _volume = GetComponent<PostProcessVolume>();
        
        if (!_volume.profile.TryGetSettings(out _dof))
        {
            Debug.LogWarning("No Depth of Field override found on the PostProcessProfile!");
        }
        else
        {
            _targetDistance = _dof.focusDistance.value;
        }
    }

    void Update()
    {
        if (_dof == null) return;

        if (Mathf.Abs(_dof.focusDistance.value - _targetDistance) > 0.01f)
        {
            _dof.focusDistance.value = Mathf.Lerp(_dof.focusDistance.value, _targetDistance, Time.deltaTime * focusSpeed);

            if(_dof.focusDistance.value < 1)
                _dof.focusDistance.value = 1;
        }
    }

    public void SetFocusDistance(float targetDistance)
    {
        _targetDistance = targetDistance;
    }
}