namespace Spellenbakkerij {

	using UnityEngine;
	using System.Collections;

	/// <summary>
	/// Insanely basic audio system which supports 3D sound.
	/// Ensure you change the 'Sounds' audio source to use 3D spatial blend if you intend to use 3D sounds.
	/// </summary>
	public class AudioSystem : StaticInstance<AudioSystem> {
		[SerializeField] private AudioSource _musicSource;
		[SerializeField] private AudioSource _soundsSource;

		public void PlayMusic(AudioClip clip, bool loop) {
			_musicSource.clip = clip;
			_musicSource.loop = loop;
			_musicSource.Play();
		}

		public void StopMusic(bool fadeOut = false) {
			if (fadeOut) {
				FadeOutMusic();
				return;
			}
			_musicSource.Stop();
		}

		public bool IsMusicPlaying {
			get { return _musicSource.isPlaying; }
		}

		public void FadeOutMusic() {
			if (_musicSource.isPlaying) {
				FadeOut(_musicSource, 1);
			}
		}

		private static IEnumerator FadeOut(AudioSource audioSource, float fadeTime) {
			float startVolume = audioSource.volume;

			while (audioSource.volume > 0) {
				audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

				yield return null;
			}

			audioSource.Stop();
			audioSource.volume = startVolume;
		}

		public void PlaySound(AudioClip clip, Vector3 pos, float vol = 1) {
			_soundsSource.transform.position = pos;
			PlaySound(clip, vol);
		}

		public void PlaySound(AudioClip clip, float vol = 1) {
			_soundsSource.PlayOneShot(clip, vol);
		}
	}
}