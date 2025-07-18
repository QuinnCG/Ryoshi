using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class FacePlayer : MonoBehaviour
	{
		private SpriteRenderer _renderer;

		private void Awake()
		{
			_renderer = GetComponent<SpriteRenderer>();
		}

		private void LateUpdate()
		{
			float dir = transform.position.DirectionTo(Player.Instance.transform.position).x;
			_renderer.flipX = dir < 0f;
		}
	}
}
