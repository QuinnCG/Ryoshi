using FMODUnity;
using QFSW.QC;
using Quinn.CombatSystem;
using Quinn.MovementSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(Player))]
	[RequireComponent(typeof(SpriteRenderer))]
	[RequireComponent(typeof(PlayableAnimator))]
	[RequireComponent(typeof(PlayerCombat))]
	[RequireComponent(typeof(BoxCollider2D))]
	public class PlayerMovement : CharacterMovement
	{
		[Space]

		[SerializeField, Unit(Units.MetersPerSecond)]
		private float DefaultMoveSpeed = 4f;
		[SerializeField, Unit(Units.MetersPerSecond)]
		private float CrouchMoveSpeed = 2f;

		[Space]

		[SerializeField]
		private Vector2 CrouchHitboxOffset;
		[SerializeField]
		private Vector2 CrouchHitboxSize;

		[Space]

		[SerializeField, Unit(Units.MetersPerSecond)]
		private float JumpSpeed = 10f;
		[SerializeField, Unit(Units.Meter)]
		private float JumpHeight = 3.2f;

		[Space]

		[SerializeField]
		private float DashSpeed = 12f;
		[SerializeField]
		private float DashDistance = 3f;
		[SerializeField]
		private float DashCooldown = 0.5f;

		[SerializeField, FoldoutGroup("Animations"), Required]
		private AnimationClip IdleAnim, MoveAnim, MoveBackAnim, CrouchedIdleAnim, CrouchedMoveAnim, JumpingAnim, FallingAnim, DashingAnim;

		[SerializeField, FoldoutGroup("SFX")]
		private EventReference FootstepSound, JumpSound, LandSound, DashSound;

		public float InputDir { get; private set; }

		public bool IsJumping { get; private set; }
		public bool IsDashing { get; private set; }
		public bool IsCrouched { get; private set; }

		private Player _player;
		private SpriteRenderer _renderer;
		private PlayableAnimator _animator;
		private PlayerCombat _combat;
		private BoxCollider2D _hitbox;

		private float _jumpInitY;
		private float _lastMoveInput;

		private float _nextAllowedDashTime;
		private float _dashEndTime;

		protected override void Awake()
		{
			base.Awake();

			_player = GetComponent<Player>();
			_renderer = GetComponent<SpriteRenderer>();
			_animator = GetComponent<PlayableAnimator>();
			_combat = GetComponent<PlayerCombat>();
			_hitbox = GetComponent<BoxCollider2D>();

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

			if (IsDashing)
			{
				ResetGravity();
				SetVelocity(DashSpeed * InputDir * Vector2.right);

				SetVisualFacingDirection(InputDir);

				if (Time.time > _dashEndTime)
				{
					IsDashing = false;
				}

				_animator.PlayLooped(DashingAnim, true);
			}
		}

		public void Move(float xDir)
		{
			if (xDir != 0f)
			{
				InputDir = Mathf.Sign(xDir);
				SetVisualFacingDirection(FacingDirection);
			}

			// Can only change facing direction, while blocking.
			if (_combat.IsBlocking)
				return;

			if (IsTouchingGround && !IsDashing && !IsJumping)
			{
				if (xDir == 0f)
				{
					_animator.PlayLooped(IsCrouched ? CrouchedIdleAnim : IdleAnim);
				}
				else
				{
					var moveAnim = Mathf.Sign(xDir) == Mathf.Sign(_player.FacingDirection) ? MoveAnim : MoveBackAnim;
					_animator.PlayLooped(IsCrouched ? CrouchedMoveAnim : moveAnim);
				}
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
				Uncrouch();

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

		protected override void OnLeaveGround()
		{
			if (!IsJumping)
			{
				ResetGravity();
			}
		}

		public void StopJump()
		{
			if (IsJumping)
			{
				IsJumping = false;
				UnblockGravity(this);
				
				if (!IsTouchingGround)
				{
					StartFalling();
				}
			}
		}

		public void Crouch()
		{
			if (!IsCrouched && IsTouchingGround)
			{
				IsCrouched = true;
				MoveSpeed = CrouchMoveSpeed;

				//_hitbox.offset = CrouchHitboxOffset;
				//_hitbox.size = CrouchHitboxSize;
			}
		}

		public void Uncrouch()
		{
			if (IsCrouched && !IsTouchingCeiling)
			{
				IsCrouched = false;
				MoveSpeed = DefaultMoveSpeed;

				//_hitbox.offset = _initHitboxOffset;
				//_hitbox.size = _initHitboxSize;
			}
		}

		public void Dash()
		{
			if (!IsDashing && Time.time >= _nextAllowedDashTime)
			{
				IsDashing = true;
				_dashEndTime = Time.time + (DashDistance / DashSpeed);
				_nextAllowedDashTime = _dashEndTime + DashCooldown;

				Audio.Play(DashSound);
			}
		}

		public void StopDash()
		{
			if (IsDashing)
			{
				IsDashing = false;
				_nextAllowedDashTime = Time.time + DashCooldown;
			}
		}

		public new void SetVelocity(Vector2 vel)
		{
			base.SetVelocity(vel);
		}

		public new void AddVelocity(Vector2 vel)
		{
			base.AddVelocity(vel);
		}

		/// <summary>
		/// Flips the sprite.
		/// </summary>
		private void SetVisualFacingDirection(float xDir)
		{
			if (xDir != 0f)
			{
				_renderer.flipX = xDir < 0f;
			}
		}

		protected override bool OnContact(Collider2D collider, Vector2 normal, int layer)
		{
			if (layer == LayerMask.NameToLayer(Layers.CharacterName))
			{
				Log.Info(normal);

				// Direction from collider to us.
				Vector2 dir = collider.bounds.center.DirectionTo(_hitbox.bounds.center);
				dir += Vector2.up; // Crude way to apply an upward bias.
				dir.Normalize();

				// Push us away from collider.
				AddDecayingVelocity(dir * 16f, 12f);

				return false;
			}

			return true;
		}

		protected void OnFootstep()
		{
			Audio.Play(FootstepSound);
		}

		[Command("nudge", "Teleport the player a distance on the x.")]
		protected void Nudge_Cmd(float x)
		{
			var pos = transform.position;
			pos.x += x;

			transform.position = pos;
		}
	}
}
