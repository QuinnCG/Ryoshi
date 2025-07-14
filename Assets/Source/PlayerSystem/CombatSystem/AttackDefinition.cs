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

		[FoldoutGroup("Data")]
		[ShowIf(nameof(Mode), AttackMode.Continuous), Tooltip("Use a slow recovery animation variant, if the dash is at least a certain distance (normalized).")]
		public bool UseSlowRecovery;
		[FoldoutGroup("Data")]
		[ShowIf(nameof(Mode), AttackMode.Continuous), EnableIf(nameof(UseSlowRecovery)), Range(0f, 1f)]
		public float SlowRecoveryAfterDashDstNorm = 0.7f;
		[FoldoutGroup("Data")]
		[ShowIf(nameof(Mode), AttackMode.Continuous), EnableIf(nameof(UseSlowRecovery))]
		public AnimationClip SlowRecoveryAnim;

		[Space]

		[FoldoutGroup("Data")]
		public AttackType Type;
		[FoldoutGroup("Data")]
		[ShowIf(nameof(Type), AttackType.Chain)]
		public bool CanBeRepeated = false;

		[Space]

		[FoldoutGroup("Data")]
		public AttackStanceType Stance;
		[FoldoutGroup("Data")]
		public int Cost = 1;

		[FoldoutGroup("Data")]
		[Space, ShowIf(nameof(Mode), AttackMode.Instant)]
		public Vector2 PushVelocity = new(12f, 0f);
		[FoldoutGroup("Data")]
		[Unit(Units.MetersPerSecond), ShowIf(nameof(Mode), AttackMode.Instant)]
		public float PushDecayRate = 32f;

		[FoldoutGroup("Data")]
		[Space, ShowIf(nameof(Mode), AttackMode.Continuous)]
		public Vector2 DashVelocity = new(12f, 0f);
		[FoldoutGroup("Data")]
		[ShowIf(nameof(Mode), AttackMode.Continuous)]
		public float MinDashDistance = 1f;
		[FoldoutGroup("Data")]
		[ShowIf(nameof(Mode), AttackMode.Continuous)]
		public float MaxDashDistance = 2f;

		public override string ToString()
		{
#if UNITY_EDITOR
			try
			{
				var title = string.Empty;

				if (Animation == null && ChargeAnim == null)
				{
					return "Attack";
				}

				if (Mode is AttackMode.Continuous && ChargeAnim != null)
				{
					title = ChargeAnim.name;
					title = title.Remove(title.IndexOf("_Charge"), "_Charge".Length);
				}
				else if (Animation != null)
				{
					title = Animation.name;
				}

				title = title.Remove(title.IndexOf("Player_"), "Player_".Length);

				return UnityEditor.ObjectNames.NicifyVariableName(title);
			}
			catch (System.Exception)
			{
				return "Attack";
			}
#else
			return string.Empty;
#endif
		}
	}
}
