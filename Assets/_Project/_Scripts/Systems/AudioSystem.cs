namespace Spellenbakkerij {

	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Insanely basic audio system which supports 3D sound.
	/// Ensure you change the 'Sounds' audio source to use 3D spatial blend if you intend to use 3D sounds.
	/// </summary>
	public class AudioSystem : StaticInstance<AudioSystem> {
		[SerializeField] private AudioSource _soundsSource;

		private AudioSource[] _musicSource;
		private int _musicCurrentSourceIndex;
		private double _musicNextStartTime;
		private const float MusicPrepartationDelay = 0.5f;

		protected void Awake() {
			this._musicSource = new AudioSource[2];
			this._musicSource[0] = this.gameObject.AddComponent<AudioSource>();
			this._musicSource[1] = this.gameObject.AddComponent<AudioSource>();
			this._musicCurrentSourceIndex = 0;
			this._musicNextStartTime = 0d; // initial value
		}

		/// <summary>
		/// Enqueues a music clip.
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="loop">When true, this clip will loop. If an additional clip is enqueued after this one the loop will be ignored.</param>
		/// <param name="playNow">The clip will start playing "now" with a short delay to allow preparing for playback. The currently playing clip will fade out within this delay time.</param>
		public void PlayMusic(AudioClip clip, bool loop = false, bool playNow = false) {
			// When first time, set to start in 0.5s from now (current dspTime)
			// This to set an actual time to start the clip (using the Audio System’s DSP Time)
			// and to avoid instantly preparing the sound for playback
			if (this._musicNextStartTime < AudioSettings.dspTime) {
				playNow = true;
			}

			if (playNow) {
				// fadeout currently playing music clip
				this.StopMusic(fadeTime: MusicPrepartationDelay);
				// and start "immediately"
				this._musicNextStartTime = AudioSettings.dspTime + MusicPrepartationDelay;
			}

			this.SwitchAudioSource();
			this.CurrentMusicSource.clip = clip;
			this.CurrentMusicSource.loop = loop;
			this.CurrentMusicSource.PlayScheduled(this._musicNextStartTime);
			Debug.Log($"PlayMusic: {clip.name} loop={loop} playnow={playNow} _musicCurrentSourceIndex={this._musicCurrentSourceIndex} _musicNextStartTime={_musicNextStartTime}");

			double duration = (double)clip.samples / clip.frequency;
			this._musicNextStartTime += duration;
		}

		public void PlaySound(AudioClip clip, Vector3 pos, float volume = 1) {
			this._soundsSource.transform.position = pos;
			this.PlaySound(clip, volume);
		}

		public void PlaySound(AudioClip clip, float volume = 1) {
			this._soundsSource.PlayOneShot(clip, volume);
		}

		public void PlayRandomSound(AudioClip[] clips, float volume = 1) {
			this.PlaySound(clips[Random.Range(0, clips.Length)], volume);
		}

		/// <summary>
		/// Stops currently playing music
		/// </summary>
		/// <param name="fadeTime">Specify 0f to stop immediately</param>
		public void StopMusic(float fadeTime = 0f) {
			if (!this.CurrentMusicSource.isPlaying) {
				return;
			}
			if (fadeTime > 0f) {
				_ = this.StartCoroutine(FadeOutMusic(this.CurrentMusicSource, fadeTime));
				return;
			}
			// stop both audio sources
			this._musicSource[0].Stop();
			this._musicSource[1].Stop();
		}

		private AudioSource CurrentMusicSource => this._musicSource[_musicCurrentSourceIndex];

		private void SwitchAudioSource() {
			this._musicCurrentSourceIndex = 1 - this._musicCurrentSourceIndex;
			Debug.Log($"AudioSource switched to index {this._musicCurrentSourceIndex}");
		}

		private static IEnumerator FadeOutMusic(AudioSource audioSource, float fadeTime) {
			float startVolume = audioSource.volume;

			while (audioSource.volume > 0) {
				audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

				yield return null;
			}

			audioSource.Stop();
			audioSource.volume = startVolume;
		}
	}
}