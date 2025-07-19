using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.AI
{
	public class Gate : MonoBehaviour
	{
		[SerializeField, Required, ChildGameObjectsOnly]
		private GameObject Opened, Closed;

		private void Awake()
		{
			Open();
		}

		public void Open()
		{
			Opened.SetActive(true);
			Closed.SetActive(false);
		}

		public void Close()
		{
			Closed.SetActive(true);
			Opened.SetActive(false);
		}
	}
}
