using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn.RoomManagement
{
	public class RoomManager : MonoBehaviour
	{
		public static RoomManager Instance { get; private set; }

		[SerializeField]
		private float FadeOutTime = 0.2f, FadeInTime = 0.2f;

		public bool IsLoading { get; private set; }

		private void Awake()
		{
			Instance = this;
		}

		public async Awaitable LoadRoom(string path)
		{
			IsLoading = true;

			await TransitionManager.Instance.FadeToBlackAsync(FadeOutTime);
			await SceneManager.LoadSceneAsync(path);
			await TransitionManager.Instance.FadeFromBlackAsync(FadeInTime);

			IsLoading = false;
		}
	}
}
