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
		[SerializeField]
		private AudioConfiguration _audioConfiguration;

		private bool _initialized = false;
		private RunConfiguration _configuration;
		private PlayState _playState;
		private Queue<LevelSegment> _levelSegmentsInOrder;
		private LevelSegment _lastAddedSegment;
		private Camera _camera;
		private TarodevController.PlayerController _playerController;
		private AudioSystem _audioSystem;

		public void Initialize(RunConfiguration configuration, PlayState playState) {
			this._playState = playState;
			this._configuration = configuration;

			this._audioSystem = FindObjectOfType<AudioSystem>();
			this._audioSystem.PlayMusic(this._audioConfiguration.MusicSetRun);

			LevelSegment start = Instantiate(configuration.StartSegment, -configuration.StartSegment.SegmentStart.position, Quaternion.identity, this._level);
			float predictedLength = start.SegmentEnd.position.x;
			this._levelSegmentsInOrder = new Queue<LevelSegment>();
			LevelSegment[] levelSegments = configuration.AllowedSegments.ToArray();
			while (predictedLength < configuration.MinimumLength) {
				LevelSegment randomSegment = levelSegments[Random.Range(0, levelSegments.Length)];
				this._levelSegmentsInOrder.Enqueue(randomSegment);
				predictedLength += randomSegment.SegmentEnd.position.x - randomSegment.SegmentStart.position.x;
			}

			this._lastAddedSegment = start;

			this._camera = Camera.main;

			this._player.transform.position = new Vector3(0, 0.5f, 0);
			this._playerController = this._player.GetComponent<TarodevController.PlayerController>();
			this._playerController.OnJumping += this.PlayerController_OnJumping;
			this._player.gameObject.SetActive(true);
			this.gameObject.SetActive(true);

			this._initialized = true;
		}

		private void PlayerController_OnJumping() {
			this._playState.ModifyStamina(-1f);
			// play jump soundclip
			this._audioSystem.PlayRandomSound(this._audioConfiguration.JumpClips, 1f);
		}

		private void Update() {
			this._camera.transform.position = this._player.position + this._configuration.CameraOffsetFromPlayer;
		}

		private void LateUpdate() {
			// Should be initialized before any updates are called.
			Debug.Assert(this._initialized);

			// Does a new segment need to be placed?
			if (this._player.position.x + 50 > this._lastAddedSegment.SegmentEnd.position.x && this._levelSegmentsInOrder.Count > 0) {
				LevelSegment nextSegment = this._levelSegmentsInOrder.Dequeue();
				this._lastAddedSegment = Instantiate(nextSegment, this._lastAddedSegment.SegmentEnd.position - nextSegment.SegmentStart.position, Quaternion.identity, this._level);
				// This should give a value between 0 and 1.
				float levelPart = this._lastAddedSegment.SegmentStart.position.x / this._configuration.MinimumLength;
				Debug.Assert(levelPart >= 0 && levelPart <= 1);
				// Set the density to the given amount.
				this._lastAddedSegment.SetObstacleDensity(this._configuration.ObstacleDensity.Evaluate(levelPart));
				this._lastAddedSegment.SetTheme(Theme.None);
			}

			// Has the player reached the end of the run?
			if (this._levelSegmentsInOrder.Count == 0 && this._player.position.x > this._lastAddedSegment.SegmentEnd.position.x) {
				FindObjectOfType<GameManager>().EndRun();
			}

			// TODO: Player out-of-bounds check.
			// We should be able to get the current segment.
			// If the player is way lower than the lowest platform of the segment, the player is out of bounds.
		}

		private void OnDisable() {
			//this._audioSystem.StopMusic(0.5f);
		}
	}
}
