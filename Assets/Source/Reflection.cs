using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class Reflection : MonoBehaviour
	{
		[SerializeField, Required]
		private SpriteRenderer Copy;

		// Self.
		private SpriteRenderer _renderer;

		private void OnValidate()
		{
			Awake();

			if (Copy != null)
			{
				Set();
			}
		}

		private void Awake()
		{
			_renderer = GetComponent<SpriteRenderer>();
		}

		private void LateUpdate()
		{
			Set();
		}

		private void Set()
		{
			_renderer.sprite = Copy.sprite;
			_renderer.flipX = Copy.flipX;
			_renderer.flipY = Copy.flipY;
		}
	}
}
