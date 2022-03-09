namespace Spellenbakkerij {
	using UnityEngine;
	using UnityEngine.Events;

	public class PlayState : ScriptableObject {
		public float Time = 0;
		public float MaxStamina = 100;
		private float _stamina = 100;
		public float CarreerScore = 0;
		public float Funds = 0;
		public float RelationScore = 0;
		public float HappinessScore = 0;

		public float Stamina { get => this._stamina; }
		public void ModifyStamina(float delta) {
			this._stamina = Mathf.Clamp(this._stamina + delta, 0, this.MaxStamina);
			OnStaminaChanged.Invoke(this._stamina);
		}

		public UnityEvent<float> OnStaminaChanged = new UnityEvent<float>();
	}
}
