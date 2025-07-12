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
	}
}
