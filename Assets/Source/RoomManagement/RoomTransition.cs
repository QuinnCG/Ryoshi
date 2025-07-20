using UnityEngine;

namespace Quinn.RoomManagement
{
	[RequireComponent(typeof(Collider2D))]
	public class RoomTransition : MonoBehaviour
	{
		[SerializeField, ScenePicker]
		private string NextRoom;

		private bool _isTransitioning;

		private void OnValidate()
		{
			GetComponent<Collider2D>().isTrigger = true;
		}

		private async void OnTriggerEnter2D(Collider2D collision)
		{
			if (!_isTransitioning && collision.IsPlayer())
			{
				_isTransitioning = true;

				MusicManager.Instance.StopAllMusic();
				await RoomManager.Instance.LoadRoom(NextRoom);
			}
		}
	}
}
