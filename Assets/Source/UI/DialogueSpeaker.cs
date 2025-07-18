using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace Quinn.UI
{
	public class DialogueSpeaker : MonoBehaviour
	{
		[SerializeField, Required]
		private CanvasGroup Group;
		[SerializeField, Required]
		private TextMeshProUGUI Text;

		[Space, SerializeField]
		private EventReference TypeSound;
		[SerializeField, Unit(Units.Second)]
		private float TypeInterval = 0.05f;
		[SerializeField, Unit(Units.Second)]
		private float MessageInterval = 4f;

		private void Awake()
		{
			Group.alpha = 0f;
		}

		public void Speak(params string[] messages)
		{
			StopAllCoroutines();
			StartCoroutine(SpeakSequence(messages));
		}

		public void StopSpeaking()
		{
			StopAllCoroutines();
			Group.DOFade(0f, 0.1f);
		}

		private IEnumerator SpeakSequence(string[] messages)
		{
			Text.text = string.Empty;

			yield return Group.DOFade(1f, 0.1f);
			var builder = new StringBuilder();

			foreach (var message in messages)
			{
				for (int i = 0; i < message.Length; i++)
				{
					builder.Append(message[i]);
					Text.text = builder.ToString();

					Audio.Play(TypeSound, transform.position);
					yield return new WaitForSeconds(TypeInterval);
				}

				yield return new WaitForSeconds(MessageInterval);
			}

			StopSpeaking();
		}
	}
}
