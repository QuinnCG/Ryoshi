using UnityEngine;

namespace Quinn.Parkour
{
	[RequireComponent(typeof(Collider2D))]
	public class ParkourForce : MonoBehaviour
	{
		[SerializeField]
		private Vector2 Velocity = Vector2.up * 20f;
		[SerializeField]
		private float DecayRate = 16f;
		[SerializeField]
		private bool RequirePlayerGrounded = true;

		private void OnValidate()
		{
			GetComponent<Collider2D>().isTrigger = true;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.IsPlayer())
			{
				var movement = collision.GetComponent<PlayerMovement>();
				
				if ((movement.IsTouchingGround || !RequirePlayerGrounded) && !movement.IsTouchingCeiling)
				{
					movement.AddDecayingVelocity(Velocity, DecayRate);
				}
			}
		}
	}
}
