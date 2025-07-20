using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.Parkour
{
	[RequireComponent(typeof(Collider2D))]
	public class ParkourCheckpoint : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void StaticReset() => Active = null;

		public static ParkourCheckpoint Active { get; private set; }

		[SerializeField]
		private bool DisableUponPlayerExit = false;
		[field: SerializeField, Required, ChildGameObjectsOnly]
		public Transform TeleportPoint { get; private set; }

		private void OnValidate()
		{
			GetComponent<Collider2D>().isTrigger = true;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				Active = this;
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.IsPlayer() && DisableUponPlayerExit)
			{
				Active = null;
			}
		}
	}
}
