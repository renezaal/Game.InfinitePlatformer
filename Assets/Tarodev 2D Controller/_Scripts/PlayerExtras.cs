using System;
using UnityEngine;

namespace TarodevController
{
    public struct FrameInput
    {
        public float X, Y;
        public bool JumpDown;
        public bool JumpHeld;
        public bool DashDown;
    }

    public interface IPlayerController
    {
        public FrameInput Input { get; }
        public Vector3 RawMovement { get; }
        public bool Grounded { get; }

        public event Action<bool> OnGroundedChanged;
        public event Action OnJumping, OnDoubleJumping;
        public event Action<bool> OnDashingChanged;
        public event Action<bool> OnCrouchingChanged;
    }

    public struct RayRange
    {
        public RayRange(float x1, float y1, float x2, float y2, Vector2 dir)
        {
            Start = new Vector2(x1, y1);
            End = new Vector2(x2, y2);
            Dir = dir;
        }

        public readonly Vector2 Start, End, Dir;
    }
}