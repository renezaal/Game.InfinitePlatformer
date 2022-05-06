using System;
using UnityEngine;

namespace Spellenbakkerij {
	[CreateAssetMenu(fileName = "AudioConfigurationScriptableObject", menuName = "ScriptableObjects/AudioConfiguration")]
	public class AudioConfiguration : ScriptableObject
    {
		public MusicSet MusicSetMenu;
		public MusicSet MusicSetRun;
		public AudioClip[] JumpClips;

	}
}
