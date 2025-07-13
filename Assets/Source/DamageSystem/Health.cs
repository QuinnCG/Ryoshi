using UnityEngine;

namespace Quinn.DamageSystem
{
	[RequireComponent(typeof(Team))]
	public class Health : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private float Default = 50f;

		public float Current { get; private set; }
		public float Max { get; private set; }
		public float Missing => Max - Current;
		public float Normalized => Current / Max;

		public bool IsDead => Current <= 0f;

		public event System.Action<DamageInfo> OnDamage;
		public event System.Action OnDeath;

		private Team _team;

		private void Awake()
		{
			Current = Max = Default;
			_team = GetComponent<Team>();
		}

		public bool TakeDamage(DamageInfo info)
		{
			if (IsDead)
				return false;

			if (info.TeamType == _team.Type)
				return false;

			Current = Mathf.Clamp(Current - info.Damage, 0f, Max);
			OnDamage?.Invoke(info);

			if (Current <= 0f)
			{
				Death();
			}

			return true;
		}

		public void Heal(float amount)
		{
			Current = Mathf.Clamp(Current + amount, 0f, Max);
		}

		public void FullHeal()
		{
			Heal(Missing + 1f);
		}

		private void Death()
		{
			OnDeath?.Invoke();
		}
	}
}
