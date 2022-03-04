namespace Spellenbakkerij {
	using System.Collections;
	using System.Collections.Generic;

	using UnityEngine;

	[CreateAssetMenu(fileName = "RunConfigurationScriptableObject", menuName = "ScriptableObjects/Run")]
	public class RunConfiguration : ScriptableObject {
		[SerializeField]
		private LevelSegment startSegment;
		[SerializeField]
		private LevelSegment[] allowedSegments;
		[SerializeField]
		private float minimumLength = 100;
		[SerializeField]
		private AnimationCurve obstacleDensity;

		public LevelSegment StartSegment => this.startSegment;
		public IEnumerable<LevelSegment> AllowedSegments => this.allowedSegments;
		public float MinimumLength => this.minimumLength;
		public AnimationCurve ObstacleDensity => this.obstacleDensity;
	}
}
