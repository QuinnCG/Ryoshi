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

		private void Awake()
		{
			_player = GetComponent<Player>();
			_animator = GetComponent<PlayableAnimator>();
			_movement = GetComponent<PlayerMovement>();

			ReplenishPoints();
		}

		private void Update()
		{
			if (Time.time > _attackEndTime)
			{
				_phase = AttackPhase.None;
				ReplenishPoints();
			}
		}

		public void Attack()
		{
			if (_phase is not (AttackPhase.None or AttackPhase.Recovering))
				return;

			var stance = GetPlayerStance();
			
			if (TrySearchForAttack(stance, out var attack))
			{
				ExecuteAttack(attack);
			}
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

			_attackPoints = Mathf.Max(_attackPoints - attack.Cost, 0);
			_animator.PlayOnce(attack.Animation);

			_attackEndTime = Time.time + attack.Animation.length;

			var pushVel = attack.PushVelocity;
			pushVel.x *= _player.FacingDirection;
			_movement.Push(attack.PushVelocity, attack.PushDecayRate, true);
		}

		private void ReplenishPoints()
		{
			_attackPoints = DefaultAttackPoints;
			Log.Info("Replenished attack points!");
		}


		/* ANIMATION EVENTS */

		protected void SFX(string eventName)
		{
			Audio.Play(eventName, transform.position);
		}

		protected void AttackingPhase()
		{
			Log.Info("Attack phase!");
			_phase = AttackPhase.Attacking;
		}

		protected void RecoveringPhase()
		{
			Log.Info("Recovery phase!");
			_phase = AttackPhase.Recovering;
		}
	}
}
