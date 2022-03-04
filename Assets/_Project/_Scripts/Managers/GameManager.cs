namespace Spellenbakkerij {
	using UnityEngine;
	using UnityEngine.SceneManagement;

	public class GameManager : MonoBehaviour {
		private const string SceneNameMain = "Main";
		private const string SceneNameRun = "Run";
		private const string SceneNameMinigame = "Minigame";
		private const string SceneNameMenu = "Menu";

		private RunConfiguration _nextRunToLoad;

		public void StartRun(RunConfiguration runConfiguration) {
			this._nextRunToLoad = runConfiguration;
			SceneManager.LoadScene("Run", LoadSceneMode.Additive);
		}

		public void EndRun() {
			Scene scene = SceneManager.GetSceneByName(SceneNameRun);
			if (scene.IsValid() && scene.isLoaded) {
				_ = SceneManager.UnloadSceneAsync(scene);
			}
		}

		public void OnEnable() {
			SceneManager.sceneLoaded += this.SceneManager_sceneLoaded;
		}

		public void OnDisable() {
			SceneManager.sceneLoaded -= this.SceneManager_sceneLoaded;
		}

		private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode) {
			if (scene.name == SceneNameRun) {
				RunManager runManager = GameObject.FindObjectOfType<RunManager>();
				runManager.Initialize(this._nextRunToLoad);
				this._nextRunToLoad = null;
			}
		}
	}
}