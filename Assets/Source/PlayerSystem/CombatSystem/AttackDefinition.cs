using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.CombatSystem
{
	[System.Serializable]
	public record AttackDefinition
	{
		public AnimationClip Animation;

		[Space]

		public AttackType Type;
		public AttackStanceType Stance;
		public int Cost = 1;

		[Space]

		public Vector2 PushVelocity;
		[Unit(Units.MetersPerMinute)]
		public float PushDecayRate = 32f;
	}
}
