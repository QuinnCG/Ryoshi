using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.CombatSystem
{
	[System.Serializable]
	public record AttackDefinition
	{
		public AttackMode Mode = AttackMode.Stationary;

		[Space, HideIf(nameof(Mode), AttackMode.Continuous)]
		public AnimationClip Animation;

		[Space, ShowIf(nameof(Mode), AttackMode.Continuous)]
		public AnimationClip ChargeAnim;
		[ShowIf(nameof(Mode), AttackMode.Continuous)]
		public AnimationClip AttackAnim;

		[Space]

		public AttackType Type;
		public AttackStanceType Stance;
		public int Cost = 1;

		[Space, HideIf(nameof(Mode), AttackMode.Continuous)]
		public Vector2 PushVelocity = new(12f, 0f);
		[Unit(Units.MetersPerSecond), HideIf(nameof(Mode), AttackMode.Continuous)]
		public float PushDecayRate = 32f;

		[Space, ShowIf(nameof(Mode), AttackMode.Continuous)]
		public Vector2 DashVelocity = new(12f, 0f);
		[ShowIf(nameof(Mode), AttackMode.Continuous)]
		public float MinDashDistance = 1f;
		[ShowIf(nameof(Mode), AttackMode.Continuous)]
		public float MaxDashDistance = 2f;
	}
}
