using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.DamageSystem
{
	[RequireComponent(typeof(Team))]
	public class Health : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private float Default = 50f;

		[SerializeField, FoldoutGroup("SFX")]
		private EventReference HurtSound, DeathSound;

		[SerializeField, FoldoutGroup("VFX")]
		private VisualEffect HurtVFX, DeathVFX;

		[Space, SerializeField, FoldoutGroup("VFX")]
		private SpriteRenderer FlashOnHurt;
		[EnableIf(nameof(FlashOnHurt)), SerializeField, FoldoutGroup("VFX")]
		private float FlashInDuration = 0.1f, FlashHoldDuration = 0.1f, FlashOutDuration = 0.1f;

		public float Current { get; private set; }
		public float Max { get; private set; }
		public float Missing => Max - Current;
		public float Normalized => Current / Max;

		public bool IsDead => Current <= 0f;

		public event System.Action<DamageInfo> OnDamage;
		public event System.Action OnDeath;

		// Renderer may be null.
		private SpriteRenderer _renderer;
		private Team _team;

		private Vector2 _lastDamageDir;

		private void Awake()
		{
			TryGetComponent(out _renderer);
			_team = GetComponent<Team>();

			Current = Max = Default;
		}

		public bool TakeDamage(DamageInfo info)
		{
			if (IsDead)
				return false;

			if (info.TeamType == _team.Type)
				return false;

			_lastDamageDir = info.Direction.normalized;

			Current = Mathf.Clamp(Current - info.Damage, 0f, Max);
			OnDamage?.Invoke(info);

			Audio.Play(HurtSound, transform.position);

			if (HurtVFX != null)
			{
				if (HurtVFX.HasVector2("Direction"))
				{
					HurtVFX.SetVector2("Direction", _lastDamageDir);
				}

				HurtVFX.Play();
			}

			FlashSequence();

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

			Audio.Play(DeathSound, transform.position);
			
			if (DeathVFX != null)
			{
				if (DeathVFX.HasVector2("Direction"))
				{
					DeathVFX.SetVector2("Direction", _lastDamageDir);
				}

				DeathVFX.Play();
			}
		}

		private void FlashSequence()
		{
			_renderer.material.DOKill();

			var a = _renderer.material.DOAnimateFloat("_Flash", 1f, FlashInDuration);
			var b = _renderer.material.DOAnimateFloat("_Flash", 0f, FlashOutDuration);

			var seq = DOTween.Sequence();
			seq.Append(a);
			seq.AppendInterval(FlashHoldDuration);
			seq.Append(b);
		}
	}
}
