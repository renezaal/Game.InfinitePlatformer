using System;
using UnityEngine;

namespace Spellenbakkerij {
	[CreateAssetMenu(fileName = "AudioConfigurationScriptableObject", menuName = "ScriptableObjects/AudioConfiguration")]
	public class AudioConfiguration : ScriptableObject
    {
		[SerializeField]
		internal MusicSet[] MusicSets;

		public AudioClip[] JumpClips;

		internal MusicSet FindMusicSetByName(string musicSetName) {
			return Array.Find(MusicSets, m => m.name == musicSetName);
		}
	}
}
