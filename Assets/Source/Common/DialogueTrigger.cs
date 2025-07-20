using Quinn.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Collider2D))]
	public class DialogueTrigger : MonoBehaviour
	{
		[SerializeField, Required]
		private DialogueSpeaker Speaker;

		[Space, SerializeField]
		private string[] Dialogue;

		private bool _hasTriggered;

		public void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer() && !_hasTriggered)
			{
				_hasTriggered = true;
				Speaker.Speak(Dialogue);
			}
		}
	}
}
