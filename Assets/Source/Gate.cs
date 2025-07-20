using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.AI
{
	public class Gate : MonoBehaviour
	{
		[SerializeField, Required, ChildGameObjectsOnly]
		private GameObject Opened, Closed;

		[Space, SerializeField]
		private bool StartOpen = true;

		[SerializeField]
		private EventReference OpenSound, CloseSound;

		private void Awake()
		{
			if (StartOpen)
			{
				Opened.SetActive(true);
				Closed.SetActive(false);
			}
			else
			{
				Closed.SetActive(true);
				Opened.SetActive(false);
			}
		}

		public void Open()
		{
			Opened.SetActive(true);
			Closed.SetActive(false);

			Audio.Play(OpenSound);
		}

		public void Close()
		{
			Closed.SetActive(true);
			Opened.SetActive(false);

			Audio.Play(CloseSound);
		}
	}
}
