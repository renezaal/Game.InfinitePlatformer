using System.Collections;
using System.Collections.Generic;

using TarodevController;

using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Spellenbakkerij {
	public class InputGetter : PlayerControllerInputGetter {
		private PlayerControls _playerControls;

		private void Awake() {
			this._playerControls = new PlayerControls();
			this._playerControls.Enable();
		}

		public override bool GetDashDown() => this._playerControls.Platformer.Dash.WasPressedThisFrame();
		public override bool GetJumpDown() => this._playerControls.Platformer.Jump.WasPressedThisFrame();
		public override bool GetJumpHeld() => this._playerControls.Platformer.Jump.IsPressed();

		public override float GetHorizontalAxis() {
			float x = this._playerControls.Platformer.Move.ReadValue<Vector2>().x;

			// Transform the move input to make only forward movements possible.
			return (x / 2f) + 0.5f;
		}
		public override float GetVerticalAxis() => this._playerControls.Platformer.Move.ReadValue<Vector2>().y;
	}
}
