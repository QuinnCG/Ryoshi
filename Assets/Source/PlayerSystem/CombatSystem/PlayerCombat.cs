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

		private float _attackEndTime;
		private AttackPhase _phase = AttackPhase.None;

		private AttackMode _mode;
		private Vector2 _dashVel;
		private Vector2 _dashStartPos;
		private float _dashMinDst, _dashMaxDst;
		private AnimationClip _attackAnim;

		private bool _wantsToAttack;

		private void Awake()
		{
			_player = GetComponent<Player>();
			_animator = GetComponent<PlayableAnimator>();
			_movement = GetComponent<PlayerMovement>();

			ReplenishPoints();
		}

		private void Update()
		{
			if (_mode is AttackMode.Continuous && _phase is AttackPhase.Charging)
			{
				_movement.BlockGravity(this);
				_movement.SetVelocity(_dashVel);

				float dst = transform.position.DistanceTo(_dashStartPos);

				bool maxDstReached = dst > _dashMaxDst;
				bool shouldEndDashEarly = !_wantsToAttack && dst > _dashMinDst;

				if (maxDstReached || shouldEndDashEarly)
				{
					_phase = AttackPhase.Attacking;
					_animator.StopOneShot();
					_animator.PlayOnce(_attackAnim);
					_wantsToAttack = false;

					_attackEndTime = Time.time + _attackAnim.length;
				}
			}
			else
			{
				_movement.UnblockGravity(this);
			}

			if (Time.time > _attackEndTime && (_mode is not AttackMode.Continuous || _phase is not AttackPhase.Charging))
			{
				_phase = AttackPhase.None;
				ReplenishPoints();
				_wantsToAttack = false;
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
				_attackEndTime = Time.time + attack.Animation.length;

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
				_dashVel = attack.DashVelocity;
				_attackAnim = attack.AttackAnim;
			}
		}

		private void ReplenishPoints()
		{
			_attackPoints = DefaultAttackPoints;
		}


		/* ANIMATION EVENTS */

		protected void SFX(string eventName)
		{
			Audio.Play(eventName, transform.position);
		}

		protected void AttackingPhase()
		{
			_phase = AttackPhase.Attacking;
		}

		protected void RecoveringPhase()
		{
			_phase = AttackPhase.Recovering;
		}
	}
}
