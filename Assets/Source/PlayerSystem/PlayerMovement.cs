using FMODUnity;
using Quinn.MovementSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(PlayableAnimator))]
	public class PlayerMovement : CharacterMovement
	{
		[SerializeField, Unit(Units.MetersPerSecond)]
		private float DefaultMoveSpeed = 4f;
		[SerializeField, Unit(Units.MetersPerSecond)]
		private float JumpSpeed = 10f;
		[SerializeField, Unit(Units.Meter)]
		private float JumpHeight = 3.2f;

		[SerializeField, FoldoutGroup("Animations")]
		private AnimationClip IdleAnim, MoveAnim;

		[SerializeField]
		private EventReference FootstepSound;

		public bool IsJumping { get; private set; }
		public bool IsDashing { get; private set; }
		public bool IsCrouched { get; private set; }

		private PlayableAnimator _animator;
		private float _jumpInitY;

		protected override void Awake()
		{
			base.Awake();

			_animator = GetComponent<PlayableAnimator>();
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
			UpdateFacingDir(xDir);

			_animator.PlayLooped((xDir != 0f) ? MoveAnim : IdleAnim);
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

		private void UpdateFacingDir(float xDir)
		{
			// TODO: In dueling fights, override this to always face boss opponent.

			if (xDir != 0f)
			{
				var scale = transform.localScale;
				scale.x = Mathf.Abs(scale.x) * Mathf.Sign(xDir);
				transform.localScale = scale;
			}
		}

		protected void OnFootstep()
		{
			Audio.Play(FootstepSound);
		}
	}

}
