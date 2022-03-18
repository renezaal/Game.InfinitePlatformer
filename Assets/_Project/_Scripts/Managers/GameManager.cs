namespace Spellenbakkerij {
	using TMPro;

	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEngine.UI;

	public class GameManager : MonoBehaviour {
		[SerializeField]
		private GameObject _gameObjectMainMenu;

		[SerializeField]
		private Button _buttonStartRun;

		[SerializeField]
		private Button _buttonQuitGame;

		[SerializeField]
		private Slider _staminaSlider;

		[SerializeField]
		private TextMeshProUGUI _timeDisplay;

		[SerializeField]
		private RunConfiguration _runConfigurationForTesting;

		private PlayState _playState;

		private void Start() {
			UnloadAllScenesExcept(_sceneNameMain);
			_playState = ScriptableObject.CreateInstance<PlayState>();
			_playState.Time = 0;
			_playState.OnStaminaChanged.AddListener(stamina => this._staminaSlider.SetValueWithoutNotify(1 - (stamina / _playState.MaxStamina)));

			_buttonStartRun.onClick.AddListener(() => { this.StartRun(this._runConfigurationForTesting); });
			_buttonQuitGame.onClick.AddListener(() => { Application.Quit(); });
		}

		private void FixedUpdate() {
			_playState.Time += Time.fixedDeltaTime * _playState.TimeRate;
			const int SecondsInMinute = 60;
			const int SecondsInHour = 60*SecondsInMinute;
			const int MinutesInHour = 60;
			const int SecondsInDay = 24*SecondsInHour;

			if (_playState.Time > SecondsInDay) {
				_playState.Time -= SecondsInDay;
			}

			int hour = ((int)_playState.Time / SecondsInHour);
			int minute = ((int)_playState.Time / SecondsInMinute) % MinutesInHour;
			int second = ((int)_playState.Time) % SecondsInMinute;

			_timeDisplay.text = $"{hour:00}:{minute:00}";
		}

		private static void UnloadAllScenesExcept(string sceneName) {
			int c = SceneManager.sceneCount;
			for (int i = 0; i < c; i++) {
				Scene scene = SceneManager.GetSceneAt (i);
				print(scene.name);
				if (scene.name != sceneName) {
					SceneManager.UnloadSceneAsync(scene);
				}
			}
		}


		private const string _sceneNameMain = "Main";
		private const string _sceneNameRun = "Run";
		private const string _sceneNameMinigame = "Minigame";

		private RunConfiguration _nextRunToLoad;

		public void StartRun(RunConfiguration runConfiguration) {
			this._gameObjectMainMenu.SetActive(false);
			this._nextRunToLoad = runConfiguration;
			Camera.main.transform.position = Vector3.zero;
			SceneManager.LoadScene(_sceneNameRun, LoadSceneMode.Additive);
		}

		public void EndRun() {
			Scene scene = SceneManager.GetSceneByName(_sceneNameRun);
			if (scene.IsValid() && scene.isLoaded) {
				_ = SceneManager.UnloadSceneAsync(scene);
			}

			this._gameObjectMainMenu.SetActive(true);
		}

		private void OnEnable() {
			SceneManager.sceneLoaded += this.SceneManager_sceneLoaded;
		}

		private void OnDisable() {
			SceneManager.sceneLoaded -= this.SceneManager_sceneLoaded;
		}

		private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode) {
			if (scene.name == _sceneNameRun && this._nextRunToLoad != null) {
				RunManager runManager = GameObject.FindObjectOfType<RunManager>(true);
				runManager.Initialize(this._nextRunToLoad, this._playState);
				this._nextRunToLoad = null;
			}
		}
	}
}