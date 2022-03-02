namespace Spellenbakkerij {
	using UnityEngine;
	using UnityEngine.Events;

	public class PlayState : ScriptableObject {
		private float playerProgress;

		public float PlayerProgress { get => playerProgress; private set => playerProgress=value; }
		private UnityEvent<float> playerProgressEvent;
		public void Test() {
			//playerProgressEvent.AddListener())
		}
	}
}
