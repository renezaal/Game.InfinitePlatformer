using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spellenbakkerij {
	[CreateAssetMenu(fileName = "AudioConfigurationScriptableObject", menuName = "ScriptableObjects/AudioConfiguration")]
	public class AudioConfiguration : ScriptableObject
    {
		public AudioClip MusicClipMenu;
		public AudioClip MusicClipRun;
		public AudioClip[] JumpClips;
	}
}
