using DG.Tweening;
using FMODUnity;
using Quinn.DamageSystem;
using Sirenix.OdinInspector;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Quinn.CombatSystem
{
	[RequireComponent(typeof(Player))]
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(PlayableAnimator))]
	[RequireComponent(typeof(PlayerMovement))]
	[RequireComponent(typeof(BoxCollider2D))]
	public class PlayerCombat : MonoBehaviour
	{
		[SerializeField]
		private int DefaultAttackPoints = 5;
		[SerializeField, Tooltip("Cooldown penality to encourage chaining attacks.")]
		private float AttackChainEndCooldown = 0.8f;
		[SerializeField]
		private float HitTimeSlowDecayRate = 10f;

		[Space]

		[SerializeField, Required]
		private AnimationClip BlockAnim, StaggerAnim;
		[SerializeField, Required, Tooltip("Not a prefab."), ChildGameObjectsOnly]
		private ParticleSystem BlockDamageVFX;
		[SerializeField]
		private EventReference BlockDamageSound;

		[SerializeField]
		private AttackDefinition[] Moveset;

		/// <summary>
		/// Is the player's attack phase charging, attacking, or recovery? If any of those, then they are "attacking".
		/// </summary>
		public bool IsAttacking => _phase is not AttackPhase.None;
		public bool IsRecovering => _phase is AttackPhase.Recovering;
		public bool IsBlocking { get; private set; }
		public bool IsStaggered => Time.time < _staggerEndTime;

		private Player _player;
		private Health _health;
		private PlayableAnimator _animator;
		private PlayerMovement _movement;
		private BoxCollider2D _hitbox;

		private float _staggerEndTime;

		/// <summary>
		/// Attacks consume points. The number of current points, also dictates whether the attack will be a starter, chain, or finisher type.
		/// </summary>
		private int _attackPoints;

		// The end time of the entire attack. This is set delayed, for the continuous mode.
		private float _entireAttackEndTime;
		private bool _wantsToAttack;

		private AttackPhase _phase = AttackPhase.None;
		private AttackMode _mode;

		private float _nextAttackChainCooldownEndTime;
		private AttackDefinition _lastAttack;

		// Used by continuous mode.
		private Vector2 _dashVel;
		private Vector2 _dashStartPos;
		private float _dashMinDst, _dashMaxDst;
		private float _dashAttackForceEndTime;

		private bool _hasEnteredAttackPhase;
		private AttackDefinition _attackDef;

		// Used by continuous mode.
		private float _attackPhaseEndTime;
		private AnimationClip _attackAnim, _recoveryAnim;

		private void Awake()
		{
			_player = GetComponent<Player>();
			_health = GetComponent<Health>();
			_animator = GetComponent<PlayableAnimator>();
			_movement = GetComponent<PlayerMovement>();
			_hitbox = GetComponent<BoxCollider2D>();

			_health.AllowDamage = AllowDamage;
			ReplenishPoints();
		}

		private void Update()
		{
			UpdateExecutingAttack();
			UpdateAttackChainEndTime();

			if (IsBlocking)
			{
				_animator.PlayLooped(BlockAnim, overrideOneShot: true);
			}
		}

		public void Attack()
		{
			if (Time.time < _nextAttackChainCooldownEndTime)
				return;

			if (_phase is not (AttackPhase.None or AttackPhase.Recovering))
				return;

			if (IsStaggered)
				return;

			_wantsToAttack = true;

			var stance = GetPlayerStance();

			if (TrySearchForAttack(stance, out var attack))
			{
				ExecuteAttack(attack);
			}
		}

		public void ReleaseAttack()
		{
			_wantsToAttack = false;
		}

		public void Block()
		{
			if (!IsBlocking && !_movement.IsJumping && _movement.IsTouchingGround && !_movement.IsDashing && !IsStaggered)
			{
				IsBlocking = true;
			}
		}

		public void Unblock()
		{
			if (IsBlocking)
			{
				IsBlocking = false;
			}
		}

		// Damage blocking.
		private bool AllowDamage(DamageInfo info)
		{
			// Ignore damage if we are blocking in the opposing direction.
			if (IsBlocking)
			{
				bool blockingDmg = false;

				if (info.Direction.x > 0f && _player.FacingDirection < 0f)
					blockingDmg = true;

				if (info.Direction.x < 0f && _player.FacingDirection > 0f)
					blockingDmg = true;

				if (blockingDmg)
				{
					Vector2 dir = info.Direction;
					// Upward bias.
					dir.y += 1f;
					dir.Normalize();
					float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

					// -45f is needed because default angle is not 0f.
					BlockDamageVFX.transform.rotation = Quaternion.AngleAxis(angle - 45f, Vector3.forward);
					BlockDamageVFX.Play();

					Audio.Play(BlockDamageSound);

					_staggerEndTime = Time.time + StaggerAnim.length;

					Unblock();
					_animator.PlayOnce(StaggerAnim);

					// Do not allow damage.
					return false;
				}
			}

			return true;
		}

		private void UpdateExecutingAttack()
		{
			void GoToAttack()
			{
				_phase = AttackPhase.Attacking;
				_wantsToAttack = false;

				_animator.StopOneShot();
				_animator.PlayOnce(_attackAnim);

				_recoveryAnim = GetRecoveryAnim(_attackDef);
				_attackPhaseEndTime = Time.time + _attackAnim.length;
				_entireAttackEndTime = _attackPhaseEndTime + _recoveryAnim.length;
			}

			void GoToRecovery()
			{
				_phase = AttackPhase.Recovering;
				_animator.PlayOnce(_recoveryAnim);
			}

			if (_mode is AttackMode.Continuous)
			{
				if (Time.time >= _dashAttackForceEndTime && _phase is AttackPhase.Charging)
				{
					GoToAttack();
				}

				if (_phase is AttackPhase.Charging)
				{
					_movement.BlockGravity(this);
					_movement.SetVelocity(_dashVel);

					float dst = transform.position.DistanceTo(_dashStartPos);

					bool maxDstReached = dst > _dashMaxDst;
					bool shouldEndDashEarly = !_wantsToAttack && dst > _dashMinDst;

					if (maxDstReached || shouldEndDashEarly)
					{
						GoToAttack();
					}
				}
				else if (_phase == AttackPhase.Attacking && Time.time >= _attackPhaseEndTime)
				{
					GoToRecovery();
				}
			}
			else
			{
				_movement.UnblockGravity(this);
			}

			if (_phase is AttackPhase.Attacking && !_hasEnteredAttackPhase)
			{
				_hasEnteredAttackPhase = true;

				Vector2 offset = _attackDef.DamageBoxOffset;
				offset.x *= _movement.FacingDirection;

				Vector2 center = (Vector2)_hitbox.bounds.center + offset;
				Vector2 size = _attackDef.DamageBoxSize;

				var colliders = Physics2D.OverlapBoxAll(center, size, 0f);
				bool hitAny = false;

				foreach (var collider in colliders)
				{
					if (collider.TryGetComponent(out IDamageable dmg))
					{
						var knockback = _attackDef.KnockbackVelocity;
						knockback.x *= _movement.FacingDirection;

						bool success = dmg.TakeDamage(new DamageInfo()
						{
							Damage = _attackDef.Damage,
							Direction = Vector2.right * _movement.FacingDirection,
							TeamType = TeamType.Player,
							Knockback = knockback
						});

						if (success)
							hitAny = true;
					}
				}

				if (hitAny)
				{
					TimeManager.ApplyFactor(this, 0f);

					this.DOKill();

					DOTween.To(() => TimeManager.GetFactor(this), t => TimeManager.ApplyFactor(this, t), 1f, 0.03f)
						.SetTarget(this)
						.SetUpdate(true)
						.SetEase(Ease.InCubic)
						.OnComplete(() => TimeManager.RemoveFactor(this));
				}
			}
		}

		private void UpdateAttackChainEndTime()
		{
			bool attackIsComplete = Time.time > _entireAttackEndTime;
			bool isContinuousDashChargeActive = _mode is AttackMode.Continuous && _phase is AttackPhase.Charging;

			// If we are not in continuous mode, respect the attackEndTime value. Otherwise, wait for us to not be in charging phase, before respecting the value.
			// At the end of charging phase (while in continuous mode), the attackEndTime is updated to match the variable length of the charge; it's not valid for continuous mode, until the end of the charging phase.
			if (attackIsComplete && !isContinuousDashChargeActive && _phase is not AttackPhase.None)
			{
				EndChain();
			}

			if (_attackPoints == 0)
			{
				EndChain();
			}
		}

		private void EndChain()
		{
			_mode = AttackMode.Stationary;
			_phase = AttackPhase.None;

			_wantsToAttack = false;
			ReplenishPoints();

			_nextAttackChainCooldownEndTime = Time.time + AttackChainEndCooldown;
		}

		private AttackStanceType GetPlayerStance()
		{
			// Dashing takes priority over airborne.
			if (_movement.IsDashing)
			{
				return AttackStanceType.Dashing;
			}
			else if (!_movement.IsTouchingGround)
			{
				return AttackStanceType.Airborne;
			}
			else if (_movement.IsCrouched)
			{
				return AttackStanceType.Crouched;
			}
			else
			{
				return AttackStanceType.Standing;
			}
		}

		private bool TrySearchForAttack(AttackStanceType stance, out AttackDefinition result)
		{
			foreach (var attack in Moveset)
			{
				if (attack.Stance != stance)
					continue;

				// Do not repeat chain attacks that have disabled such functionality.
				if (attack.Type is AttackType.Chain && !attack.CanBeRepeated && _lastAttack == attack)
					continue;

				if (attack.Cost > _attackPoints)
					continue;

				if (_attackPoints == DefaultAttackPoints && attack.Type is not AttackType.Starter)
					continue;

				if (_attackPoints != DefaultAttackPoints && attack.Type is AttackType.Starter)
					continue;

				if (_attackPoints == attack.Cost && attack.Type is not AttackType.Finisher)
					continue;

				if (_attackPoints != attack.Cost && attack.Type is AttackType.Finisher)
					continue;

				result = attack;
				return true;
			}

			result = default;
			return false;
		}

		private void ExecuteAttack(AttackDefinition attack)
		{
			_movement.StopJump();
			_movement.Uncrouch();
			_movement.StopDash();

			_attackDef = attack;

			_hasEnteredAttackPhase = false;

			Unblock();

			_lastAttack = attack;
			_animator.StopOneShot();

			_phase = AttackPhase.Charging;
			_mode = attack.Mode;

			_attackPoints = Mathf.Max(_attackPoints - attack.Cost, 0);
			_movement.StopJump();

			if (attack.Mode is AttackMode.Stationary or AttackMode.Instant)
			{
				_animator.PlayOnce(attack.Animation);
				_entireAttackEndTime = Time.time + attack.Animation.length;

				if (attack.Mode is AttackMode.Instant)
				{
					var pushVel = attack.PushVelocity;
					pushVel.x *= _player.FacingDirection;
					_movement.Push(pushVel, attack.PushDecayRate, true);
				}
			}
			else
			{
				_animator.PlayOnce(attack.ChargeAnim, holdEndFrame: true);

				_dashStartPos = transform.position;
				_dashMinDst = attack.MinDashDistance;
				_dashMaxDst = attack.MaxDashDistance;
				_attackAnim = attack.AttackAnim;
				_dashAttackForceEndTime = Time.time + (attack.MaxDashDistance / attack.DashVelocity.x);

				_dashVel = attack.DashVelocity;
				_dashVel.x *= _player.FacingDirection;
			}
		}

		private void ReplenishPoints()
		{
			_attackPoints = DefaultAttackPoints;
		}

		/// <returns>The regular recovery animation, or the slow variant, if applicable.</returns>
		private AnimationClip GetRecoveryAnim(AttackDefinition attack)
		{
			if (_mode is not AttackMode.Continuous || attack.SlowRecoveryAnim == null)
			{
				return attack.RecoveryAnim;
			}

			float dst = transform.position.DistanceTo(_dashStartPos);
			float dstNorm = dst / attack.MaxDashDistance;

			return (dstNorm >= attack.SlowRecoveryAfterDashDstNorm) ? attack.SlowRecoveryAnim : attack.RecoveryAnim;
		}

		/* ANIMATION EVENTS */

		protected void SFX(string eventName)
		{
			Audio.Play(eventName, transform.position);
		}

		/// <summary>
		/// Called by attacks that aren't of the continuous mode.<br/>
		/// Continuous mode attacks break up the animation into a part for each phase, so there is no need to call an event to indicate a phase change.
		/// </summary>
		protected void AttackingPhase()
		{
			if (_mode is not AttackMode.Continuous)
			{
				_phase = AttackPhase.Attacking;
			}
			else
			{
				Log.Warning($"Active animation '{_attackAnim}' has events to set the attacking phase. This shouldn't happen with continuous mode attacks.");
			}
		}

		/// <summary>
		/// Called by attacks that aren't of the continuous mode.<br/>
		/// Continuous mode attacks break up the animation into a part for each phase, so there is no need to call an event to indicate a phase change.
		/// </summary>
		protected void RecoveringPhase()
		{
			if (_mode is not AttackMode.Continuous)
			{
				_phase = AttackPhase.Recovering;
			}
			else
			{
				Log.Warning($"Active animation '{_attackAnim}' has events to set the recovery phase. This shouldn't happen with continuous mode attacks.");
			}
		}
	}
}
