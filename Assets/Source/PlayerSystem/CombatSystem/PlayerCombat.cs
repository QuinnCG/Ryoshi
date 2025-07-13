using FMOD;
using UnityEngine;

namespace Quinn.CombatSystem
{
	[RequireComponent(typeof(Player))]
	[RequireComponent(typeof(PlayableAnimator))]
	[RequireComponent(typeof(PlayerMovement))]
	public class PlayerCombat : MonoBehaviour
	{
		[SerializeField]
		private int DefaultAttackPoints = 5;

		[SerializeField]
		private AttackDefinition[] Moveset;

		/// <summary>
		/// Is the player's attack phase charging, attacking, or recovery? If any of those, then they are "attacking".
		/// </summary>
		public bool IsAttacking => _phase is not AttackPhase.None;

		private Player _player;
		private PlayableAnimator _animator;
		private PlayerMovement _movement;

		/// <summary>
		/// Attacks consume points. The number of current points, also dictates whether the attack will be a starter, chain, or finisher type.
		/// </summary>
		private int _attackPoints;

		// The end time of the entire attack. This is set delayed, for the continuous mode.
		private float _entireAttackEndTime;
		private bool _wantsToAttack;

		private AttackPhase _phase = AttackPhase.None;
		private AttackMode _mode;

		// Used by continuous mode.
		private Vector2 _dashVel;
		private Vector2 _dashStartPos;
		private float _dashMinDst, _dashMaxDst;

		// Used by continuous mode.
		private float _attackPhaseEndTime;
		private AnimationClip _attackAnim, _recoveryAnim;
		private AttackDefinition _attackDef;

		private void Awake()
		{
			_player = GetComponent<Player>();
			_animator = GetComponent<PlayableAnimator>();
			_movement = GetComponent<PlayerMovement>();

			ReplenishPoints();
		}

		private void Update()
		{
			UpdateExecutingAttack();
			UpdateAttackEndTime();
		}

		private void UpdateExecutingAttack()
		{
			if (_mode is AttackMode.Continuous)
			{
				if (_phase is AttackPhase.Charging)
				{
					_movement.BlockGravity(this);
					_movement.SetVelocity(_dashVel);

					float dst = transform.position.DistanceTo(_dashStartPos);

					bool maxDstReached = dst > _dashMaxDst;
					bool shouldEndDashEarly = !_wantsToAttack && dst > _dashMinDst;

					if (maxDstReached || shouldEndDashEarly)
					{
						_phase = AttackPhase.Attacking;
						_wantsToAttack = false;

						_animator.StopOneShot();
						_animator.PlayOnce(_attackAnim);

						_recoveryAnim = GetRecoveryAnim(_attackDef);
						_attackPhaseEndTime = Time.time + _attackAnim.length;
						_entireAttackEndTime = _attackPhaseEndTime + _recoveryAnim.length;
					}
				}
				else if (_phase == AttackPhase.Attacking && Time.time >= _attackPhaseEndTime)
				{
					_phase = AttackPhase.Recovering;
					_animator.PlayOnce(_recoveryAnim);
				}
			}
			else
			{
				_movement.UnblockGravity(this);
			}
		}

		private void UpdateAttackEndTime()
		{
			bool attackEndTime = Time.time > _entireAttackEndTime;
			bool isContinuousDashChargeActive = _mode is AttackMode.Continuous && _phase is AttackPhase.Charging;

			// If we are not in continuous mode, respect the attackEndTime value. Otherwise, wait for us to not be in charging phase, before respecting the value.
			// At the end of charging phase (while in continuous mode), the attackEndTime is updated to match the variable length of the charge; it's not valid for continuous mode, until the end of the charging phase.
			if (attackEndTime && !isContinuousDashChargeActive)
			{
				_mode = AttackMode.Stationary;
				_phase = AttackPhase.None;

				_wantsToAttack = false;
				ReplenishPoints();
			}
		}

		public void Attack()
		{
			if (_phase is not (AttackPhase.None or AttackPhase.Recovering))
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

		private AttackStanceType GetPlayerStance()
		{
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

				if (attack.Cost > DefaultAttackPoints)
					continue;

				if (_attackPoints == DefaultAttackPoints && attack.Type is not AttackType.Starter)
					continue;

				if (_attackPoints == attack.Cost && attack.Type is not AttackType.Finisher)
					continue;

				result = attack;
				return true;
			}

			result = default;
			return false;
		}

		private void ExecuteAttack(AttackDefinition attack)
		{
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
					_movement.Push(attack.PushVelocity, attack.PushDecayRate, true);
				}
			}
			else
			{
				_animator.PlayOnce(attack.ChargeAnim, holdEndFrame: true);

				_dashStartPos = transform.position;
				_dashMinDst = attack.MinDashDistance;
				_dashMaxDst = attack.MaxDashDistance;
				_attackAnim = attack.AttackAnim;
				_attackDef = attack;

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
			if (_mode is not AttackMode.Continuous)
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
