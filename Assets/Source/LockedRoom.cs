using FMODUnity;
using Quinn.DamageSystem;
using Quinn.UI;
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
		[SerializeField]
		private EventReference BossMusic, OutroMusic;

		[field: SerializeField, Required]
		public BossAI Boss { get; private set; }

		public bool HasBegun { get; private set; }
		public bool IsConquered { get; private set; }

		private Health _bossHealth;

		private void Awake()
		{
			if (SaveManager.IsSaved(SaveKey))
			{
				IsConquered = true;
			}

			_bossHealth = Boss.GetComponent<Health>();
		}

		private void Update()
		{
			if (HasBegun && !IsConquered)
			{
				Audio.SetGlobalParameter("is-second-phase", (_bossHealth.Normalized < 0.5f) ? 1f : 0f);
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

				MusicManager.Instance.PlayBossMusic(BossMusic);

				if (Boss != null)
				{
					BossBarUI.Instance.StartFight(Boss);
				}
			}
		}

		public async void Conquere()
		{
			if (!IsConquered)
			{
				MusicManager.Instance.StopBossMusic(OutroMusic);

				if (Boss != null)
				{
					BossBarUI.Instance.StopFight();
				}

				SaveManager.Save(SaveKey);

				await Awaitable.WaitForSecondsAsync(1f);

				IsConquered = true;
				Open();
				Player.Instance.InLockedRoom = false;

				Instance = null;
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
