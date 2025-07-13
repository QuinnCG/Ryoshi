using Sirenix.OdinInspector;
using UnityEngine;

namespace Quinn.CombatSystem
{
	[System.Serializable]
	public record AttackDefinition
	{
		[Title("@ToString()")]
		public AttackMode Mode = AttackMode.Stationary;

		[Space, HideIf(nameof(Mode), AttackMode.Continuous)]
		public AnimationClip Animation;

		[Space, ShowIf(nameof(Mode), AttackMode.Continuous)]
		public AnimationClip ChargeAnim;
		[ShowIf(nameof(Mode), AttackMode.Continuous)]
		public AnimationClip AttackAnim;
		[ShowIf(nameof(Mode), AttackMode.Continuous)]
		public AnimationClip RecoveryAnim;

		[Space]

		[ShowIf(nameof(Mode), AttackMode.Continuous), Tooltip("Use a slow recovery animation variant, if the dash is at least a certain distance (normalized).")]
		public bool UseSlowRecovery;
		[ShowIf(nameof(Mode), AttackMode.Continuous), EnableIf(nameof(UseSlowRecovery)), Range(0f, 1f)]
		public float SlowRecoveryAfterDashDstNorm = 0.7f;
		[ShowIf(nameof(Mode), AttackMode.Continuous), EnableIf(nameof(UseSlowRecovery))]
		public AnimationClip SlowRecoveryAnim;

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

		public override string ToString()
		{
#if UNITY_EDITOR
			string title;

			if (Animation == null && ChargeAnim == null)
			{
				return "Attack";
			}

			if (Mode is AttackMode.Continuous)
			{
				title = ChargeAnim.name;
				title = title.Remove(title.IndexOf("_Charge"), "_Charge".Length);
			}
			else
			{
				title = Animation.name;
			}

			title = title.Remove(title.IndexOf("Player_"), "Player_".Length);

			return UnityEditor.ObjectNames.NicifyVariableName(title);
#else
			return string.Empty;
#endif
		}
	}
}
