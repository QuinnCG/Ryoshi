using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Quinn.UI
{
	public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
	{
		[SerializeField]
		private EventReference HoverSound;

		public void OnPointerEnter(PointerEventData eventData)
		{
			Audio.Play(HoverSound);
		}
	}
}
