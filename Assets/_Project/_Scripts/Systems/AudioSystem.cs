namespace Spellenbakkerij {
	using System;
	using System.Collections;

	using UnityEngine;

	/// <summary>
	/// Insanely basic audio system which supports 3D sound.
	/// Ensure you change the 'Sounds' audio source to use 3D spatial blend if you intend to use 3D sounds.
	/// </summary>
	public class AudioSystem : StaticInstance<AudioSystem> {
		[SerializeField] private AudioSource _soundsSource;

		private AudioSource[] _musicSource;
		private int _musicSourceIndex;

		private double _musicCurrentPlayingEndTime;
		private (AudioSource MusicSource, double StartFade, float FadeTime, bool Active) _musicFadeOut;
		private (AudioSource MusicSource, double StartFade, float FadeTime, bool Active) _musicFadeIn;

		private const float MusicPrepartationDelay = 0.5f;
		private MusicSet _musicSet;

		public bool IsPlaying { get; private set; }

		override protected void Awake() {
			this._musicSource = new AudioSource[2];
			this._musicSource[0] = this.gameObject.AddComponent<AudioSource>();
			this._musicSource[1] = this.gameObject.AddComponent<AudioSource>();
			this._musicSourceIndex = 0;
			this._musicCurrentPlayingEndTime = 0d; // initial value
			this._musicFadeOut = (null, 0, 0, false);
			this._musicFadeIn = (null, 0, 0, false);
			this.IsPlaying = false;
			base.Awake();
		}

		internal void PlayMusic(MusicSet musicSet) {
			// if previous MusicSet is playing, fade out?
			if (this.IsPlaying) {
				// longest of both fadein and fadeout time
				float time = Math.Max(this._musicSet.fadeout, musicSet.fadein);
				// new end time
				double endtime = AudioSettings.dspTime + Math.Max(time, MusicPrepartationDelay);

				// set fadeout
				this._musicFadeOut = (this.ActiveMusicSource, endtime - this._musicSet.fadeout, this._musicSet.fadeout, false);
				// set fadein
				this._musicFadeIn = (this.OtherMusicSource, endtime - musicSet.fadein, musicSet.fadein, false);

				this._musicCurrentPlayingEndTime = endtime;
			} else {
				// set fadein
				this._musicFadeIn = (this.OtherMusicSource, AudioSettings.dspTime + MusicPrepartationDelay, musicSet.fadein, false);

				this._musicCurrentPlayingEndTime = AudioSettings.dspTime + Math.Max(musicSet.fadein, MusicPrepartationDelay);
			}

			this._musicSet = musicSet;
			this._musicSet.Index = 0;
			this.IsPlaying = true;
			// set endtime
		}

		private void Update() {
			// time to schedule next music clip
			if (this._musicSet.Index < this._musicSet.clips.Length && AudioSettings.dspTime > this._musicCurrentPlayingEndTime - MusicPrepartationDelay) {
				Clip clip = this._musicSet.clips[this._musicSet.Index++];
				EnqueueMusic(clip);
			}

			// time to start fadeout
			if (!this._musicFadeOut.Active && this._musicFadeOut.StartFade > 0 && AudioSettings.dspTime > this._musicFadeOut.StartFade) {
				this._musicFadeOut.Active = true;
				_ = this.StartCoroutine(FadeOutMusic(this._musicFadeOut.MusicSource, this._musicFadeOut.FadeTime));
			}

			// time to start fadein
			if (!this._musicFadeIn.Active && this._musicFadeIn.StartFade > 0 && AudioSettings.dspTime > this._musicFadeIn.StartFade) {
				this._musicFadeIn.Active = true;
				_ = this.StartCoroutine(FadeInMusic(this._musicFadeIn.MusicSource, this._musicFadeIn.FadeTime));
			}
		}

		/// <summary>
		/// Enqueues a music clip.
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="loop">When true, this clip will loop. If an additional clip is enqueued after this one the loop will be ignored.</param>
		/// <param name="playAsSoonAsPossible">The clip will start playing "now" with a short delay to allow preparing for playback. The currently playing clip will fade out within this delay time.</param>
		private void EnqueueMusic(Clip clip) {
			IsPlaying = true;

			this.ToggleMusicSource();
			this.ActiveMusicSource.clip = clip.audioClip;
			this.ActiveMusicSource.loop = clip.loop;
			this.ActiveMusicSource.PlayScheduled(this._musicCurrentPlayingEndTime);
			Debug.Log($"{AudioSettings.dspTime}: PlayMusic: {clip.audioClip.name} loop={clip.loop} _musicSourceIndex={this._musicSourceIndex} _musicCurrentPlayingEndTime={_musicCurrentPlayingEndTime}");

			if (!clip.loop) {
				this._musicCurrentPlayingEndTime += clip.Duration;
			}
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

		public AudioSource ActiveMusicSource => this._musicSource[_musicSourceIndex];
		public AudioSource OtherMusicSource => this._musicSource[1 - _musicSourceIndex];

		private void ToggleMusicSource() {
			this._musicSourceIndex = 1 - this._musicSourceIndex;
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

		private static IEnumerator FadeInMusic(AudioSource audioSource, float fadeTime) {
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

	}
}