using UnityEngine;

namespace Quinn.CombatSystem
{
	[RequireComponent(typeof(PlayerMovement))]
	public class PlayerCombat : MonoBehaviour
	{
		[SerializeField]
		private int DefaultAttackPoints = 5;

		[SerializeField]
		private AttackDefinition[] Moveset;

		private PlayerMovement _movement;
		/// <summary>
		/// Attacks consume points. The number of current points, also dictates whether the attack will be a starter, chain, or finisher type.
		/// </summary>
		private int _attackPoints;

		private void Awake()
		{
			_movement = GetComponent<PlayerMovement>();

			ReplenishPoints();
		}

		private void Update()
		{
			
		}

		public void TriggerAttack()
		{
			// 1) Collect state (e.g. crouched).
			// 2) Search for best attack from moveset.
			// 3) Execute attack.

			var stance = GetPlayerStance();
			var attack = SearchForAttack(stance);
			ExecuteAttack(attack);
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

		private AttackDefinition SearchForAttack(AttackStanceType stance)
		{
			foreach (var attack in Moveset)
			{
				if (attack.Stance != stance)
					continue;

				if (attack.Cost > DefaultAttackPoints)
					continue;

				// TODO: Check if the attack should be a starter, chain, or finisher type.

				return attack;
			}

			Log.Warning("Failed to find a valid attack for the player to execute!");
			return default;
		}

		private void ExecuteAttack(AttackDefinition attack)
		{
			_attackPoints = Mathf.Max(_attackPoints - attack.Cost, 0);
		}

		private void ReplenishPoints()
		{
			_attackPoints = DefaultAttackPoints;
		}
	}
}
