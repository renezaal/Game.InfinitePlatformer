namespace Spellenbakkerij {
	using System.Collections;
	using System.Collections.Generic;

	using UnityEngine;

	[CreateAssetMenu(fileName = "RunConfigurationScriptableObject", menuName = "ScriptableObjects/Run")]
	public class RunConfiguration : ScriptableObject {
		[SerializeField]
		private LevelSegment _startSegment;

		[SerializeField]
		private LevelSegment[] _allowedSegments;

		[SerializeField]
		private float _minimumLength = 100;

		[SerializeField]
		private AnimationCurve _obstacleDensity;

		[SerializeField]
		private Vector3 _cameraOffsetFromPlayer = new Vector3(0,0,-10);

		public LevelSegment StartSegment => this._startSegment;
		public IEnumerable<LevelSegment> AllowedSegments => this._allowedSegments;
		public float MinimumLength => this._minimumLength;
		public AnimationCurve ObstacleDensity => this._obstacleDensity;
		public Vector3 CameraOffsetFromPlayer => this._cameraOffsetFromPlayer;
	}
}
