namespace Spellenbakkerij {
	using System;
	using System.Collections;

	using UnityEngine;

	[Serializable]
	public class MusicSet {
		public Clip[] Intro;
		public Clip[] Loop;
		public float FadeIn;
		public float FadeOut;
		public float FadeOverlap;

		public int Index { get; set; }
	}

	[Serializable]
	public class Clip {
		public AudioClip audioClip;
		/// <summary>
		/// loop and repeat are mutually exclusive. If loop then no repeat. If repeat then no loop.
		/// loop true/false is leading in this.
		/// </summary>
		public int Iterations = 1;

		public double Duration => (double) audioClip.samples / audioClip.frequency;
	}
}
