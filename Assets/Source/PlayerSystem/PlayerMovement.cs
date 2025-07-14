using FMODUnity;
using Quinn.CombatSystem;
using Quinn.MovementSystem;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(SpriteRenderer))]
	[RequireComponent(typeof(PlayableAnimator))]
	[RequireComponent(typeof(PlayerCombat))]
	public class PlayerMovement : CharacterMovement
	{
		[Space]

		[SerializeField, Unit(Units.MetersPerSecond)]
		private float DefaultMoveSpeed = 4f;
		[SerializeField, Unit(Units.MetersPerSecond)]
		private float JumpSpeed = 10f;
		[SerializeField, Unit(Units.Meter)]
		private float JumpHeight = 3.2f;

		[SerializeField, FoldoutGroup("Animations")]
		private AnimationClip IdleAnim, MoveAnim, JumpingAnim, FallingAnim;

		[SerializeField, FoldoutGroup("SFX")]
		private EventReference FootstepSound, JumpSound, LandSound;

		public bool IsJumping { get; private set; }
		public bool IsDashing { get; private set; }
		public bool IsCrouched { get; private set; }

		private SpriteRenderer _renderer;
		private PlayableAnimator _animator;
		private PlayerCombat _combat;

		private float _jumpInitY;
		private float _lastMoveInput;

		protected override void Awake()
		{
			base.Awake();

			_renderer = GetComponent<SpriteRenderer>();
			_animator = GetComponent<PlayableAnimator>();
			_combat = GetComponent<PlayerCombat>();

			MoveSpeed = DefaultMoveSpeed;
		}

		protected override void Update()
		{
			base.Update();

			if (IsJumping)
			{
				ResetGravity();
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

			if (!IsTouchingGround)
			{
				_animator.PlayLooped(IsJumping ? JumpingAnim : FallingAnim);
			}
		}

		public void Move(float xDir)
		{
			UpdateFacingDir(xDir);

			if (IsTouchingGround)
			{
				_animator.PlayLooped((xDir != 0f) ? MoveAnim : IdleAnim);
			}

			if (!_combat.IsAttacking)
			{
				AddVelocity(MoveSpeed * xDir * Vector2.right);

				// Play starting footstep.
				if (xDir != 0f && _lastMoveInput == 0f)
				{
					OnFootstep();
				}
			}

			_lastMoveInput = xDir;
		}

		public void Jump()
		{
			if (!IsJumping && IsTouchingGround)
			{
				IsJumping = true;
				_jumpInitY = transform.position.y;

				BlockGravity(this);

				Audio.Play(JumpSound);
			}
		}

		protected override void OnTouchGround()
		{
			Audio.Play(LandSound);
		}

		public void StopJump()
		{
			if (IsJumping)
			{
				IsJumping = false;
				UnblockGravity(this);
				StartFalling();
			}
		}

		public new void SetVelocity(Vector2 vel)
		{
			base.SetVelocity(vel);
		}

		private void UpdateFacingDir(float xDir)
		{
			// TODO: In dueling fights, override this to always face boss opponent.

			if (xDir != 0f)
			{
				//var scale = transform.localScale;
				//scale.x = Mathf.Abs(scale.x) * Mathf.Sign(xDir);
				//transform.localScale = scale;

				_renderer.flipX = xDir < 0f;
			}
		}

		protected void OnFootstep()
		{
			Audio.Play(FootstepSound);
		}
	}

}
