using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.AI
{
	public class LockedRoom : MonoBehaviour
	{
		public static LockedRoom Instance { get; private set; }

		[SerializeField]
		private string SaveKey;
		[SerializeField]
		private Gate[] Gates;
		[SerializeField]
		private EventReference OpenSound, CloseSound;

		[field: SerializeField, Required]
		public AgentAI Boss { get; private set; }

		public bool HasBegun { get; private set; }
		public bool IsConquered { get; private set; }

		private void Awake()
		{
			if (SaveManager.IsSaved(SaveKey))
			{
				IsConquered = true;
			}
		}

		public void Begin()
		{
			if (!IsConquered && !HasBegun)
			{
				HasBegun = true;

				Close();
				Player.Instance.InLockedRoom = true;

				Instance = this;
			}
		}

		public async void Conquere()
		{
			if (!IsConquered)
			{
				await Awaitable.WaitForSecondsAsync(1f);

				IsConquered = true;
				Open();
				Player.Instance.InLockedRoom = false;

				Instance = null;

				SaveManager.Save(SaveKey);
			}
		}

		private void Open()
		{
			foreach (var gate in Gates)
			{
				gate.Open();
			}

			Audio.Play(OpenSound);
		}

		private void Close()
		{
			foreach (var gate in Gates)
			{
				gate.Close();
			}

			Audio.Play(CloseSound);
		}
	}
}
