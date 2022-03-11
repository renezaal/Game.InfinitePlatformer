using System.Collections;
using System.Collections.Generic;

using TarodevController;

using UnityEngine;

namespace Spellenbakkerij {
	public class InputGetter : IInputGetter {
		private PlayerControls _playerControls;

		public InputGetter() {
			this._playerControls = new PlayerControls();
			this._playerControls.Enable();
		}

		public bool GetDashDown() => false;
		public bool GetDashHeld() => false;
		public bool GetJumpDown() => this._playerControls.Platformer.Jump.WasPressedThisFrame();
		public bool GetJumpHeld() => this._playerControls.Platformer.Jump.IsPressed();
		public Vector2 GetMove() {
			Vector2 moveInput = this._playerControls.Platformer.Move.ReadValue<Vector2>();

			// Transform the move input to make only forward movements possible.
			moveInput.x = (moveInput.x / 2f) + 0.5f;
			return moveInput;
		}
	}
}
