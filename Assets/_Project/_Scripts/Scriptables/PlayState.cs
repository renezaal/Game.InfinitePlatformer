namespace Spellenbakkerij {
	using UnityEngine;
	using UnityEngine.Events;

	public class PlayState : ScriptableObject {
		[SerializeField]
		private float time;
		public float Time { get => this.time; }

		private float maxStamina;
		private float stamina;
		private float carreerScore;
		private float funds;
		private float relationScore;
		private float happinessScore;
	}
}
