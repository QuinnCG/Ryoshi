using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.UI
{
	public class ProximityFadeUI : MonoBehaviour
	{
		[SerializeField]
		private float Proximity = 5f;
		[SerializeField]
		private float FadeDuration = 0.1f;

		[Space]

		[SerializeField, Required]
		private CanvasGroup Group;

		private void Awake()
		{
			Group.alpha = 0f;
		}

		private void LateUpdate()
		{
			Group.DOKill();
			float dst = transform.position.DistanceTo(Player.Instance.transform.position);

			if (dst <= Proximity && Group.alpha < 1f)
			{
				Group.DOFade(1f, FadeDuration);
			}
			else if (dst > Proximity && Group.alpha > 0f)
			{
				Group.DOFade(0f, FadeDuration);
			}
		}
	}
}
