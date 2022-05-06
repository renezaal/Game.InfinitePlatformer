using UnityEngine;

namespace Spellenbakkerij {
	public class MainMenu : MonoBehaviour
    {
		[SerializeField]
		private AudioSystem audioSystem;

		[SerializeField]
		private AudioConfiguration _audioConfiguration;

		private void OnEnable() {
			this.audioSystem.PlayMusic(this._audioConfiguration.MusicSetMenu);
		}
	}
}
