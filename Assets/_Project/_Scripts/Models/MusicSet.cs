namespace Spellenbakkerij {
	using System;

	using UnityEngine;

	[Serializable]
	internal class MusicSet {
		public string name;
		public Clip[] clips;
		public float fadein;
		public float fadeout;

		public int Index { get; set; }
	}

	[Serializable]
	internal class Clip {
		public AudioClip audioClip;
		public bool loop;
		/// <summary>
		/// loop and repeat are mutually exclusive. If loop then no repeat. If repeat then no loop.
		/// loop true/false is leading in this.
		/// </summary>
		public int repeat;

		public double Duration => (double) audioClip.samples / audioClip.frequency;
	}
}
