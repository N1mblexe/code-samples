
using UnityEngine;

interface ICameraEffector
{  
    public Vector3 GetPositionOffset();
    public virtual Vector3 GetRotationOffset() { return Vector3.zero; }
}