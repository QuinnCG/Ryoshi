using DG.Tweening;
using Quinn.AI;
using Quinn.DamageSystem;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class BossBarUI : MonoBehaviour
	{
		public static BossBarUI Instance { get; private set; }

		[SerializeField, Required]
		private CanvasGroup Group;
		[SerializeField, Required]
		private TextMeshProUGUI Title;
		[SerializeField, Required]
		private Slider Health;

		private Health _boss;

		private void Awake()
		{
			Instance = this;
		}

		private void LateUpdate()
		{
			if (_boss != null)
			{
				Health.value = _boss.Normalized;
			}
		}

		public void StartFight(BossAI boss)
		{
			Title.text = boss.Title;
			_boss = boss.GetComponent<Health>();

			Group.DOFade(1f, 1f);
		}

		public void StopFight()
		{
			_boss = null;
			Health.value = 0f;
			Group.DOFade(0f, 1f);
		}
	}
}
