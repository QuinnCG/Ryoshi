using Quinn.MovementSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	public class PlayerMovement : CharacterMovement
	{
		[SerializeField, Unit(Units.MetersPerSecond)]
		private float DefaultMoveSpeed = 4f;
		[SerializeField, Unit(Units.MetersPerSecond)]
		private float JumpSpeed = 10f;
		[SerializeField, Unit(Units.Meter)]
		private float JumpHeight = 3.2f;

		public bool IsJumping { get; private set; }
		public bool IsDashing { get; private set; }
		public bool IsCrouched { get; private set; }

		private float _jumpInitY;

		protected override void Awake()
		{
			base.Awake();
			MoveSpeed = DefaultMoveSpeed;
		}

		protected override void Update()
		{
			base.Update();

			if (IsJumping)
			{
				float heightDelta = Mathf.Abs(transform.position.y - _jumpInitY);
				
				if (heightDelta > JumpHeight || IsTouchingCeiling)
				{
					StopJump();
				}
				else
				{
					AddVelocity(JumpSpeed * Vector2.up);
				}
			}
		}

		public void Move(float xDir)
		{
			AddVelocity(MoveSpeed * xDir * Vector2.right);
		}

		public void Jump()
		{
			if (!IsJumping && IsTouchingGround)
			{
				IsJumping = true;
				_jumpInitY = transform.position.y;
			}
		}

		public void StopJump()
		{
			if (IsJumping)
			{
				IsJumping = false;
			}
		}
	}
}
