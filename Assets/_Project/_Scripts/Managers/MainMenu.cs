using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spellenbakkerij 
{
    public class MainMenu : MonoBehaviour
    {
		[SerializeField]
		AudioSystem audioSystem;
		[SerializeField]
		AudioClip musicClip;

		private void OnEnable() {
			audioSystem.PlayMusic(musicClip, true);
		}

		private void OnDisable() {
			audioSystem.StopMusic(fadeOut: true);
		}
	}
}
