namespace Spellenbakkerij
{
	using Spellenbakkerij.Assets._Project._Scripts.Models.Enums;

	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	using UnityEngine;

	public class LevelSegment : MonoBehaviour {
		[SerializeField]
		private Transform segmentStart;
		[SerializeField]
		private Transform segmentEnd;
		[SerializeField]
		private Transform obstacleContainer;

		public Transform SegmentStart { get => segmentStart; }
		public Transform SegmentEnd { get => segmentEnd; }

		/// <summary>
		/// Sets the density of the obstacles to the given value.
		/// </summary>
		/// <param name="density">1 for maximum density, 0 for no obstacles.</param>
		public void SetObstacleDensity(float density) {
			Debug.Assert(density <= 1, $"Density is too high ({density}).");
			Debug.Assert(density >= 0, $"Density is too low ({density}).");
			var obstacles = new List<Transform>();
			foreach (Transform obstacle in obstacleContainer) {
				// Default is inactive.
				obstacle.gameObject.SetActive(false);
				obstacles.Add(obstacle);
			}

			// Get all inactive obstacles in random order.
			var inactiveObstacles = new Stack<Transform>(obstacles.OrderBy(x => Random.value));
			// Calculate the amount of obstacles we're going to activate.
			int amountToActivate = (int) (obstacles.Count * density);

			Debug.Assert(amountToActivate <= inactiveObstacles.Count, "Trying to activate more obstacles than present.");
			// Pop and activate the required amount of obstacles.
			for (int i = 0; i < amountToActivate; i++) {
				Transform obstacle = inactiveObstacles.Pop();
				obstacle.gameObject.SetActive(true);
			}
		}

		public void SetTheme(Theme theme) {

		}
	}
}
