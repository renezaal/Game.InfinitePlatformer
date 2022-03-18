namespace TarodevController {

	using UnityEngine;

	public abstract class PlayerControllerInputGetter : MonoBehaviour {
		public abstract float GetHorizontalAxis();
		public abstract float GetVerticalAxis();
		public abstract bool GetJumpDown();
		public abstract bool GetJumpHeld();
		public abstract bool GetDashDown();
	}
}