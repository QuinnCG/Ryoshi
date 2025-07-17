using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn.UI
{
	public class HealthUI : MonoBehaviour
	{
		[SerializeField, Required]
		private Slider HealthBar;

		private void LateUpdate()
		{
			HealthBar.value = Player.Instance.Health.Normalized;
		}
	}
}
