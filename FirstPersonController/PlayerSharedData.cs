using System;
using UnityEngine;

namespace Data
{
    public static class Movement
    {
        static public Vector2 mouseDelta;
        static public Vector3 playerVelocity;
        static public float playerSpeed;
        static public bool isGrounded;

        public static Action OnJumpWindUp;
        public static Action OnJumped;
        public static Action<float> OnLanded;

        public static Action OnMovementStarted;
        public static Action OnMovementStopped;
    }
    public static class Combat
    {
        public static class Charge
        {
            static public int maxChargeMs = 1400;
            static public Vector2 chargePosition;
            static public float mouseDeltaThreshold = .4f;
        }
        public static class Swing
        {
            static public int swingMs = 800;
        }
    }
}