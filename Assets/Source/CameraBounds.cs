using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(BoxCollider2D))]
	public class CameraBounds : MonoBehaviour
	{
		private void OnValidate()
		{
			GetComponent<BoxCollider2D>().isTrigger = true;
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.CompareTag("Player"))
			{
				CameraController.Instance.ResetConfiner();
			}
		}

		public void Activate()
		{
			CameraController.Instance.SetConfiner(GetComponent<BoxCollider2D>());
		}
	}
}
