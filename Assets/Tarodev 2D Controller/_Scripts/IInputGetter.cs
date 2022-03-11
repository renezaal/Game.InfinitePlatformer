namespace TarodevController {

	using UnityEngine;

	public interface IInputGetter {
		Vector2 GetMove();
		bool GetJumpDown();
		bool GetJumpHeld();
		bool GetDashDown();
		bool GetDashHeld();
	}
}
