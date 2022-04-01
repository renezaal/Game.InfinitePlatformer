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

		public void StopMusic(float fadeTime = 0f) {
			if (fadeTime > 0) {
				FadeOutMusic(fadeTime);
				return;
			}
			_musicSource.Stop();
		}

		public bool IsMusicPlaying {
			get { return _musicSource.isPlaying; }
		}

		private void FadeOutMusic(float fadeTime) {
			if (_musicSource.isPlaying) {
				StartCoroutine(FadeOut(_musicSource, fadeTime));
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

		public void PlaySound(AudioClip clip, Vector3 pos, float volume = 1) {
			_soundsSource.transform.position = pos;
			PlaySound(clip, volume);
		}

		public void PlaySound(AudioClip clip, float volume = 1) {
			_soundsSource.PlayOneShot(clip, volume);
		}

		public void PlayRandomSound(AudioClip[] clips, float volume = 1) {
			this.PlaySound(clips[Random.Range(0, clips.Length)], volume);
		}
	}
}