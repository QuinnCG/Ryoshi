using Quinn.MovementSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.AI
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class EnemyMovement : CharacterMovement
	{
		[SerializeField, Unit(Units.MetersPerSecondSquared)]
		private float WalkSpeed = 1f, RunSpeed = 3f;
		[SerializeField]
		private bool WalkByDefault = true;

		public bool IsDashing { get; private set; }

		private SpriteRenderer _renderer;
		private float _dashDir, _dashSpeed, _dashEndTime;

		//public event System.Action OnJumpComplete, OnDashComplete;

		protected override void Awake()
		{
			base.Awake();

			_renderer = GetComponent<SpriteRenderer>();

			if (WalkByDefault)
				SetSpeedWalk();
			else
				SetSpeedRun();
		}

		protected override void Update()
		{
			base.Update();

			if (IsDashing)
			{
				SetVelocity(_dashDir * _dashSpeed * Vector2.right);

				if (Time.time >= _dashEndTime)
				{
					IsDashing = false;
				}
			}
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			_renderer.flipX = FacingDirection < 0f;
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
			IsDashing = true;

			_dashDir = Mathf.Sign(xDirection);
			_dashSpeed = speed;
			_dashEndTime = Time.time + (distance / speed);
		}

		public new void SetVelocity(Vector2 velocity)
		{
			base.SetVelocity(velocity);
		}
	}
}
