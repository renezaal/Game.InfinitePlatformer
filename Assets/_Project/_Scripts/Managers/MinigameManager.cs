using Spellenbakkerij;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bakkerij
{
    public class MinigameManager : MonoBehaviour
    {
		private PlayState _playState;
		private MinigameConfiguration _configuration;
		private Camera _camera;
		private bool _initialized = false;

		public void Initialize(MinigameConfiguration configuration, PlayState playState) {
			this._playState = playState;
			this._configuration = configuration;
			this._camera = Camera.main;

			this.gameObject.SetActive(true);
			this._initialized = true;
		}
	}
}
