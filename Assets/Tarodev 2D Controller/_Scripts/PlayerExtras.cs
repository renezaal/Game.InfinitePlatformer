using System;

using UnityEngine;

namespace TarodevController {
	public struct FrameInput {
		public float X, Y;
		public bool JumpDown;
		public bool JumpHeld;
		public bool DashDown;
	}

	public interface IPlayerController {
		public FrameInput Input { get; }
		public Vector3 RawMovement { get; }
		public bool Grounded { get; }

		public event Action<bool> OnGroundedChanged;
		public event Action OnJumping, OnDoubleJumping;
		public event Action<bool> OnDashingChanged;
		public event Action<bool> OnCrouchingChanged;
	}

	public interface IPlayerEffector {
		public Vector3 EvaluateEffector();
	}
}