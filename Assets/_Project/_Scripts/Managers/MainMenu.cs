using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spellenbakkerij 
{
    public class MainMenu : MonoBehaviour
    {
		[SerializeField]
		private AudioSystem audioSystem;

		[SerializeField]
		private AudioConfiguration _audioConfiguration;

		private void OnEnable() {
			audioSystem.PlayMusic(this._audioConfiguration.MusicClipMenu, true);
		}

		private void OnDisable() {
			// Fade out 0 gezet want anders wordt de run music ook ge-fadeout
			// Zit in zelfde audiosource
			// TODO: oplossing voor bedenken
			//audioSystem.StopMusic(fadeTime: 0f);
		}
	}
}
