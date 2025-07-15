using UnityEngine;

namespace Quinn.MovementSystem
{
	public class EnemyMovement : CharacterMovement
	{
		public event System.Action OnJumpComplete, OnDashComplete;

		public bool MoveTo(Vector2 destination)
		{
			throw new System.NotImplementedException();
		}

		public void MoveTowards(Vector2 destination)
		{
			throw new System.NotImplementedException();
		}
		public void MoveTowards(float xDirection)
		{
			throw new System.NotImplementedException();
		}

		public void JumpTo(Vector2 destination, float height, float speed)
		{
			throw new System.NotImplementedException();
		}

		public void DashTowards(float xDirection, float speed, float distance)
		{
			throw new System.NotImplementedException();
		}
	}
}
