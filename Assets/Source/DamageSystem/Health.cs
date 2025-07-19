using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Quinn.MovementSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

namespace Quinn.DamageSystem
{
	[RequireComponent(typeof(Team))]
	public class Health : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private float Default = 50f;
		[SerializeField]
		private Slider HPBar;

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
		public bool IsAlive => !IsDead;

		public System.Func<DamageInfo, bool> AllowDamage { private get; set; }

		public event System.Action<DamageInfo> OnDamage;
		public event System.Action OnDeath;

		// Renderer may be null.
		private SpriteRenderer _renderer;
		// Locomotion may be null.
		private Locomotion _locomotion;
		private Team _team;

		private Vector2 _lastDamageDir;

		private void Awake()
		{
			TryGetComponent(out _renderer);
			TryGetComponent(out _locomotion);
			_team = GetComponent<Team>();

			Current = Max = Default;
		}

		private void LateUpdate()
		{
			if (HPBar != null)
			{
				HPBar.value = Normalized;
			}
		}

		public bool CanDamage(DamageInfo info)
		{
			if (IsDead)
				return false;

			if (info.TeamType == _team.Type)
				return false;

			return true;
		}

		public bool TakeDamage(DamageInfo info, out bool isLethal)
		{
			if (!CanDamage(info))
			{
				isLethal = false;
				return false;
			}

			bool? allowed = AllowDamage?.Invoke(info);
			if (allowed.HasValue && !allowed.Value)
			{
				isLethal = false;
				return false;
			}

			// This is the first instance of damage.
			if (Current == Max && HPBar != null)
			{
				HPBar.GetComponent<CanvasGroup>().DOFade(1f, 0.1f);
			}

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

			if (info.Knockback.sqrMagnitude > 0f && _locomotion != null)
			{
				// HACK: DecayRate shouldn't be a magic number.
				_locomotion.AddDecayingVelocity(info.Knockback, 32f);
			}

			if (Current <= 0f)
			{
				if (HPBar != null)
				{
					var group = HPBar.GetComponent<CanvasGroup>();
					group.DOKill();
					group.DOFade(0f, 0.1f);
				}

				Death();
			}

			isLethal = IsDead;
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
