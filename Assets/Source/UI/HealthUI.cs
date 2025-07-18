using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class HealthUI : MonoBehaviour
	{
		[SerializeField, Required]
		private Image[] Bars;

		[Space]

		[SerializeField, Required]
		private Image FluidSprite;
		[SerializeField]
		private int MinFramerate = 6, MaxFramerate = 16;
		[SerializeField]
		private float MinHeight = -60f, MaxHeight = 60f;
		[SerializeField]
		private Sprite[] AnimSprites;

		[SerializeField, Range(0f, 1f)]
		private float DebugValue = 1f;

		private int _index;
		private float _next;

		private void OnValidate()
		{
			Set(DebugValue);
		}

		private void LateUpdate()
		{
			float norm = Player.Instance.Health.Normalized;
			Set(norm);

			if (Time.time >= _next)
			{
				_next = Time.time + (1f / Mathf.Lerp(MaxFramerate, MinFramerate, norm));

				_index++;

				if (_index >= AnimSprites.Length)
					_index = 0;

				FluidSprite.sprite = AnimSprites[_index];
			}
		}

		private void Set(float norm)
		{
			if (Bars.Length == 0 || FluidSprite == null)
				return;

			foreach (var bar in Bars)
			{
				bar.fillAmount = norm;
				FluidSprite.rectTransform.anchoredPosition = new(0f, Mathf.Lerp(MinHeight, MaxHeight, norm));
			}
		}
	}
}
