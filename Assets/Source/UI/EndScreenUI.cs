using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn.UI
{
	public class EndScreenUI : MonoBehaviour
	{
		private CursorStateHandle _cursorHandle;
		private bool _isButtonPressed;

		private async void Start()
		{
			await Awaitable.WaitForSecondsAsync(0.2f);

			_cursorHandle = InputManager.Instance.GetCursorStateHandle();
			await TransitionManager.Instance.FadeFromBlackAsync(1f);
		}

		public async void Retry()
		{
			if (_isButtonPressed)
				return;
			else
				_isButtonPressed = true;

				_cursorHandle.ForceShowCursor = false;

			await TransitionManager.Instance.FadeToBlackAsync(2f);
			await SceneManager.LoadSceneAsync(0);
		}

		public async void Quit()
		{
			if (_isButtonPressed)
				return;
			else
				_isButtonPressed = true;

			await TransitionManager.Instance.FadeToBlackAsync(1f);
			Application.Quit();

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
	}
}
