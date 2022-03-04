namespace Spellenbakkerij {
	using Spellenbakkerij.Assets._Project._Scripts.Models.Enums;

	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	using UnityEngine;

	public class RunManager : MonoBehaviour {
		[SerializeField]
		private Transform _player;
		[SerializeField]
		private Transform _level;

		private bool _initialized = false;
		private RunConfiguration _configuration;
		private Queue<LevelSegment> _levelSegmentsInOrder;
		private LevelSegment _lastAddedSegment;

		public void Initialize(RunConfiguration configuration) {
			this._configuration = configuration;

			LevelSegment start = Instantiate(configuration.StartSegment, -configuration.StartSegment.SegmentStart.position, Quaternion.identity, this._level);
			float predictedLength = start.SegmentEnd.position.x;
			this._levelSegmentsInOrder = new Queue<LevelSegment>();
			LevelSegment[] levelSegments = configuration.AllowedSegments.ToArray();
			while (predictedLength < configuration.MinimumLength) {
				LevelSegment randomSegment = levelSegments[Random.Range(0, levelSegments.Length)];
				this._levelSegmentsInOrder.Enqueue(randomSegment);
				predictedLength += randomSegment.SegmentEnd.position.x - randomSegment.SegmentStart.position.x;
			}

			this._player.transform.position = new Vector3(0, 0.5f, 0);
			this._player.gameObject.SetActive(true);

			this._initialized = true;
		}

		void LateUpdate() {
			// Should be initialized before any updates are called.
			Debug.Assert(this._initialized);

			if (this._player.position.x + 50 > this._lastAddedSegment.SegmentEnd.position.x && this._levelSegmentsInOrder.Count > 0) {
				LevelSegment nextSegment = this._levelSegmentsInOrder.Dequeue();
				this._lastAddedSegment = Instantiate(nextSegment, this._lastAddedSegment.SegmentEnd.position - nextSegment.SegmentStart.position, Quaternion.identity);
				// This should give a value between 0 and 1.
				float levelPart = this._lastAddedSegment.SegmentStart.position.x / this._configuration.MinimumLength;
				Debug.Assert(levelPart >= 0 && levelPart <= 1);
				// Set the density to the given amount.
				this._lastAddedSegment.SetObstacleDensity(this._configuration.ObstacleDensity.Evaluate(levelPart));
				this._lastAddedSegment.SetTheme(Theme.None);
			}
		}
	}
}
