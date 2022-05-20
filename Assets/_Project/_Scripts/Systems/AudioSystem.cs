namespace Spellenbakkerij {
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;

	using UnityEngine;

	/// <summary>
	/// Insanely basic audio system which supports 3D sound.
	/// Ensure you change the 'Sounds' audio source to use 3D spatial blend if you intend to use 3D sounds.
	/// </summary>
	public class AudioSystem : StaticInstance<AudioSystem> {
		private AudioSource _soundsSource;
		private AudioSource _activeMusicSource;
		private AudioSource _inactiveMusicSource;

		private const float MusicPreparationDelay = 0.6f;
		private const float ScheduleAheadDuration = 1.2f;
		private MusicSet _musicSet;
		private MusicSet _nextMusicSet;

		private double _clipEndTime = 0;
		private double _scheduleEndTime = 0;
		private readonly ConcurrentDictionary<double, Coroutine> _scheduledCoroutines = new ConcurrentDictionary<double, Coroutine>();

		private Clip _lastScheduledClip = null;
		private int _lastScheduledClipIndex = 0;
		private MusicSetPart _lastScheduledMusicSetPart = MusicSetPart.Intro;
		private int _lastScheduledPlayIteration = 0;

		public bool IsPlaying { get; private set; } = false;

		override protected void Awake() {
			this._soundsSource = this.gameObject.AddComponent<AudioSource>();
			this._activeMusicSource = this.gameObject.AddComponent<AudioSource>();
			this._inactiveMusicSource = this.gameObject.AddComponent<AudioSource>();
			Debug.Assert(ScheduleAheadDuration > MusicPreparationDelay, "Plantijd moet groter zijn dan de voorbereidingstijd");

			this._scheduleEndTime = AudioSettings.dspTime + ScheduleAheadDuration + MusicPreparationDelay;

			base.Awake();
		}

		internal void PlayMusic(MusicSet musicSet) {
			// Schedule the next music set.
			this._nextMusicSet = musicSet;
			this.IsPlaying = true;
		}

		private void Update() {
			if (!this.IsPlaying) { return; }

			void SetNextMusicSet() {
				this._musicSet = this._nextMusicSet;
				this._nextMusicSet = null;
				this._lastScheduledMusicSetPart = MusicSetPart.Intro;
			}

			if (this._musicSet == null) {
				if (this._nextMusicSet == null) {
					return;
				} else {
					SetNextMusicSet();
				}
			}

			// Check of er een fade-out/in gedaan moet worden.
			if (this._nextMusicSet != null) {
				foreach (Coroutine coroutine in this._scheduledCoroutines.Values) {
					this.StopCoroutine(coroutine);
				}
				this._scheduledCoroutines.Clear();

				double fadeOutTime = this._musicSet.FadeOut;
				double fadeOverlapTime = this._musicSet.FadeOverlap;
				double fadeInTime = this._nextMusicSet.FadeIn;

				Debug.Assert(fadeOutTime <= MusicPreparationDelay, "Fade out time is larger than preparation delay.");
				Debug.Assert(fadeOverlapTime <= MusicPreparationDelay, "Fade overlap time is larger than preparation delay.");
				Debug.Assert(fadeInTime <= MusicPreparationDelay, "Fade in time is larger than preparation delay.");
				Debug.Assert(fadeOverlapTime <= fadeOutTime, "Fade overlap time is larger than fade out time.");
				Debug.Assert(fadeOverlapTime <= fadeInTime, "Fade overlap time is larger than fade in time.");


				_ = this.StartCoroutine(FadeOutMusic(this._activeMusicSource, (float) fadeOutTime));
				_ = this.StartCoroutine(FadeInMusic(this._inactiveMusicSource, (float) fadeInTime, fadeInDelay: fadeOutTime - fadeOverlapTime));
				
				SetNextMusicSet();
				this._scheduleEndTime = AudioSettings.dspTime + (fadeOutTime - fadeOverlapTime);
				this._lastScheduledClip = null;
			}

			// Controleer normale schedule.
			if (AudioSettings.dspTime + ScheduleAheadDuration + MusicPreparationDelay <= this._scheduleEndTime) { return; }

			Clip[] GetClips() {
				switch (this._lastScheduledMusicSetPart) {
					case MusicSetPart.Intro:
						return this._musicSet.Intro;
					case MusicSetPart.Loop:
						return this._musicSet.Loop;
					default:
						throw new Exception("Method must be updated to handle additional cases.");
				}
			}

			// Drie situaties:
			// 1. Geen last scheduled clip.
			// 2. Huidige iteratie is groter of gelijk aan clip iteraties.
			// 3. Huidige iteratie is kleiner dan clip iteraties.

			if (this._lastScheduledClip == null) {
				this._lastScheduledClip = GetClips()[0];
				this._lastScheduledPlayIteration = 0;
				this._lastScheduledClipIndex = 0;
			}

			if (this._lastScheduledPlayIteration >= this._lastScheduledClip.Iterations) {
				// We moeten naar de volgende clip.
				this._lastScheduledClipIndex++;
				if (GetClips().Length <= this._lastScheduledClipIndex) {
					this._lastScheduledClipIndex = 0;
					if (this._lastScheduledMusicSetPart == MusicSetPart.Intro) {
						this._lastScheduledMusicSetPart = MusicSetPart.Loop;
					}
				}

				this._lastScheduledClip = GetClips()[this._lastScheduledClipIndex];
				this._lastScheduledPlayIteration = 0;
			}


			// Hier gaan we uit van situatie 3.
			// Huidige iteratie is kleiner dan clip iteraties.
			// Dus de last scheduled clip moet ingepland worden.
			this._scheduledCoroutines[this._scheduleEndTime] = this.StartCoroutine(ScheduleEnqueueClipAt(this._lastScheduledClip, this._scheduleEndTime));
			this._lastScheduledPlayIteration++;
		}

		/// <summary>
		/// Enqueues a music clip.
		/// It PlaySchedules the first clip to start after the current playing clip
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="loop">When true, this clip will loop. If an additional clip is enqueued after this one the loop will be ignored.</param>
		/// <param name="playAsSoonAsPossible">The clip will start playing "now" with a short delay to allow preparing for playback. The currently playing clip will fade out within this delay time.</param>
		private IEnumerator ScheduleEnqueueClipAt(Clip clip, double timeToPlay) {
			Debug.Assert(clip.Duration >= MusicPreparationDelay, "Clip is shorter than preparation delay.");
			this._scheduleEndTime = Math.Max(timeToPlay + clip.Duration, this._scheduleEndTime);

			yield return new WaitForSecondsRealtime((float) ((timeToPlay - AudioSettings.dspTime) - MusicPreparationDelay));
			Debug.Log($"AudioSystem.ScheduleEnqueueClipAt(): {AudioSettings.dspTime} > Enqueue \"{clip.audioClip.name}\" at {timeToPlay}, _scheduleEndTime {this._scheduleEndTime}");
			Debug.Log($"AudioSystem.ScheduleEnqueueClipAt(): {AudioSettings.dspTime} > {this._activeMusicSource.GetInstanceID()} > {clip.audioClip.name}, _musicCurrentPlayingEndTime={_clipEndTime}");

			this.ToggleMusicSource();
			this._activeMusicSource.clip = clip.audioClip;
			this._activeMusicSource.PlayScheduled(timeToPlay);

			this._clipEndTime = timeToPlay + clip.Duration;
			_ = this._scheduledCoroutines.TryRemove(timeToPlay, out _);
		}

		/// <summary>
		/// Stops currently playing music
		/// </summary>
		/// <param name="fadeTime">Specify 0f to stop immediately. Default is 0f.</param>
		public void StopMusic(AudioSource audioSource, float fadeTime = 0f) {
			if (!audioSource.isPlaying) {
				return;
			}
			audioSource.loop = false;
			if (fadeTime > 0f) {
				_ = this.StartCoroutine(FadeOutMusic(audioSource, fadeTime));
				return;
			}
			audioSource.Stop();
		}

		private void ToggleMusicSource() {
			AudioSource temp = this._activeMusicSource;
			this._activeMusicSource = this._inactiveMusicSource;
			this._inactiveMusicSource = temp;
		}

		private static IEnumerator FadeOutMusic(AudioSource audioSource, float fadeTime) {
			float startVolume = audioSource.volume;
			while (audioSource.volume > 0) {
				audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
				yield return null;
				if (audioSource == null) {
					yield break;
				}
			}

			audioSource.Stop();
			audioSource.volume = startVolume;
		}

		private static IEnumerator FadeInMusic(AudioSource audioSource, float fadeTime, double fadeInDelay) {
			if (fadeInDelay > 0) {
				yield return new WaitForSecondsRealtime((float) fadeInDelay);
			}

			float startVolume = audioSource.volume;
			audioSource.volume = 0;
			while (audioSource.volume < startVolume) {
				audioSource.volume += startVolume * Time.deltaTime / fadeTime;
				yield return null;
				if (audioSource == null) {
					yield break;
				}
			}
			audioSource.volume = startVolume;
		}

		public void PlaySound(AudioClip clip, Vector3 pos, float volume = 1) {
			this._soundsSource.transform.position = pos;
			this.PlaySound(clip, volume);
		}

		public void PlaySound(AudioClip clip, float volume = 1) {
			this._soundsSource.PlayOneShot(clip, volume);
		}

		public void PlayRandomSound(AudioClip[] clips, float volume = 1) {
			this.PlaySound(clips[UnityEngine.Random.Range(0, clips.Length)], volume);
		}

		private enum MusicSetPart {
			Intro,
			Loop,
		}
	}
}