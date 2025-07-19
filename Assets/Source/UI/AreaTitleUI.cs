using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Quinn.UI
{
	public class AreaTitleUI : MonoBehaviour
	{
		[SerializeField]
		private string Title = "Area Title";
		[SerializeField]
		private string Subtitle = "The Area's Subtitle";

		[Space]

		[SerializeField]
		private float FadeInDuration = 0.2f;
		[SerializeField]
		private Ease FadeInEase = Ease.Linear;

		[Space]

		[SerializeField]
		private float PersistDuration = 2f;

		[Space]

		[SerializeField]
		private float FadeOutDuration = 0.5f;
		[SerializeField]
		private Ease FadeOutEase = Ease.Linear;

		[SerializeField, FoldoutGroup("References")]
		private CanvasGroup Group;
		[SerializeField, FoldoutGroup("References"), Required]
		private TextMeshProUGUI TitleText, SubtitleText;

		private void Awake()
		{
			Group.alpha = 0f;

			if (SaveManager.IsSaved(Title))
			{
				return;
			}

			SaveManager.Save(Title);

			TitleText.text = Title;
			SubtitleText.text = Subtitle;

			var seq = DOTween.Sequence();
			seq.Append(Group.DOFade(1f, FadeInDuration).SetEase(FadeInEase));
			seq.AppendInterval(PersistDuration);
			seq.Append(Group.DOFade(0f, FadeOutDuration).SetEase(FadeOutEase));
		}
	}
}
