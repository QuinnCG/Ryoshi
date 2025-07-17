using Quinn.MovementSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	public class AlignWithFaceDirection : MonoBehaviour
	{
		[SerializeField, Required]
		private CharacterMovement Movement;

		private void LateUpdate()
		{
			var scale = transform.localScale;
			scale.x = Mathf.Abs(scale.x) * Movement.FacingDirection;
			transform.localScale = scale;
		}
	}
}
