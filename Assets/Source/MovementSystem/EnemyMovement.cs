using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.MovementSystem
{
	public class EnemyMovement : CharacterMovement
	{
		[SerializeField, Unit(Units.MetersPerSecondSquared)]
		private float WalkSpeed = 1f, RunSpeed = 3f;
		[SerializeField]
		private bool WalkByDefault = true;

		public event System.Action OnJumpComplete, OnDashComplete;

		protected override void Awake()
		{
			base.Awake();

			if (WalkByDefault)
				SetSpeedWalk();
			else
				SetSpeedRun();
		}

		public void SetSpeedWalk()
		{
			MoveSpeed = WalkSpeed;
		}

		public void SetSpeedRun()
		{
			MoveSpeed = RunSpeed;
		}

		public bool MoveTo(Vector2 destination, float stoppingDst = 0.2f)
		{
			float diff = destination.x - transform.position.x;

			float xDir = Mathf.Sign(diff);
			MoveTowards(xDir);

			return Mathf.Abs(diff) <= stoppingDst;
		}

		public void MoveTowards(float xDirection)
		{
			MoveTowards(xDirection, MoveSpeed);
		}
		public void MoveTowards(float xDirection, float speed)
		{
			AddVelocity(Mathf.Sign(xDirection) * speed * Vector2.right);
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
