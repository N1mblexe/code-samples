using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Camera
{
    public class Manager : MonoBehaviour
    {
        [SerializeField] private List<ICameraEffector> effectors;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform targetCamera;
        [SerializeField] private float smoothness = 10f;
        private Vector3 targetPosition;

        private bool InitManager()
        {
            if (cameraHolder == null)
                cameraHolder = this.transform;

            if (targetCamera == null)
                targetCamera = cameraHolder.GetComponentInChildren<UnityEngine.Camera>().transform;

            if(effectors == null || effectors.Count == 0)
                effectors = GetComponents<ICameraEffector>().ToList();
            
            if (targetCamera == null || cameraHolder == null || effectors == null || effectors.Count == 0)
                return false;
        
            return true;
        }

        private Vector3 CalculatePositionOffset()
        {
            Vector3 offset = new Vector3(0, 0, 0);

            foreach (var effector in effectors)
                offset += effector.GetPositionOffset();

            return offset;
        }
        private Vector3 CalculateRotationOffset()
        {
            Vector3 offset = new Vector3(0, 0, 0);

            foreach (var effector in effectors)
                offset += effector.GetRotationOffset();

            return offset;
        }

        [SerializeField] private bool applyEffects = true;
        private void ApplyOffsets(Vector3 posOffset, Vector3 rotOffset)
        {
            targetPosition = Vector3.Lerp(targetPosition , posOffset , smoothness * Time.deltaTime);

            targetCamera.localPosition = targetPosition;
            targetCamera.localEulerAngles = rotOffset;
        }

        void LateUpdate()
        {
            if(applyEffects)
                ApplyOffsets(CalculatePositionOffset() , CalculateRotationOffset());
        }
        void Awake()
        {
            if(!InitManager())
                Debug.LogError("Error Initilazing CameraManager");
        }
    }
}