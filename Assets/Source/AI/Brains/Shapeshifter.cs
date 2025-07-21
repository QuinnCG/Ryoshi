using DG.Tweening;
using FMODUnity;
using Quinn.DamageSystem;
using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.AI.Brains
{
	public class Shapeshifter : BossAI
	{
		enum Form
		{
			Oni,
			Mage,
			Knight
		}

		[SerializeField, Required]
		private LockedRoom Room;

		[Space]

		[SerializeField]
		private float DialogueStartDistance = 7f;
		[SerializeField]
		private string[] Dialogue;

		[Space]

		[SerializeField]
		private float TransformHPInterval = 80f;
		[SerializeField, Required]
		private AnimationClip Death;
		[SerializeField, Required]
		private VisualEffect DeathBlood;

		[SerializeField, Required, FoldoutGroup("Grandpa")]
		private AnimationClip GrandpaIdle, GrandpaTransform;
		[SerializeField, Required]
		private VisualEffect TransformBlood;

		[SerializeField, Required, FoldoutGroup("Oni")]
		private AnimationClip OniIdle, OniThrow, ToMage, ToKnight, OniJump, OniJumping, OniFalling, OniLand;
		[SerializeField, FoldoutGroup("Oni")]
		private Vector2 OniIdleTime = new(0.5f, 2f);
		[SerializeField, Required, FoldoutGroup("Oni")]
		private Transform OniThrowSpawnPoint;
		[SerializeField, FoldoutGroup("Oni")]
		private MissileSpawnBehavior OniCastBehavior;
		[SerializeField, Required, FoldoutGroup("Oni")]
		private GameObject OniMissile;
		[SerializeField, FoldoutGroup("Oni")]
		private float OniJumpOverThrowChance = 0.5f;
		[SerializeField, FoldoutGroup("Oni")]
		private float OniJumpHeight = 2f, OniJumpDuration = 2f;
		[SerializeField, FoldoutGroup("Oni"), Required]
		private EventReference OniJumpSound, OniLandSound;

		[SerializeField, Required, FoldoutGroup("Mage")]
		private AnimationClip MageIdle, MageCharge, MageCast, FromMage;
		[SerializeField, FoldoutGroup("Mage")]
		private Vector2 MageIdleTime = new(0.1f, 0.5f);
		[SerializeField, Required, FoldoutGroup("Mage")]
		private VisualEffect CastVFX;
		[SerializeField, Required, FoldoutGroup("Mage")]
		private GameObject MageMissile;
		[SerializeField, FoldoutGroup("Mage")]
		private MissileSpawnBehavior MageCastBehavior;
		[SerializeField, FoldoutGroup("Mage")]
		private float TeleportOverCastChance = 0.2f;
		[SerializeField, Required, ChildGameObjectsOnly, FoldoutGroup("Mage")]
		private VisualEffect TeleportVFX;
		[SerializeField, Required, FoldoutGroup("Mage")]
		private BoxCollider2D MageTeleportBounds;
		[SerializeField, Required, FoldoutGroup("Mage")]
		private EventReference MageTeleportSound, CastSound;

		[SerializeField, Required, FoldoutGroup("Knight")]
		private AnimationClip KnightIdle, FromKnight, KnightDash, KnightAttack1, KnightAttack2;
		[SerializeField, FoldoutGroup("Knight")]
		private Vector2 KnightIdleTime = new(2f, 4f);
		[SerializeField, FoldoutGroup("Knight")]
		private float DashOverAttackChance = 0.7f;
		[SerializeField, FoldoutGroup("Knight")]
		private float AttackAfterDashChance = 0.8f;
		[SerializeField, FoldoutGroup("Knight")]
		private Vector2Int AttackCount = new(1, 3);
		[SerializeField, FoldoutGroup("Knight")]
		private float KnightDashSpeed = 12f;
		[SerializeField, FoldoutGroup("Knight")]
		private float KnightMaxDashDistance = 7f, KnightAttackRange = 3f;
		[SerializeField, FoldoutGroup("Knight")]
		private Vector2 KnightAttackBoxOffset, KnightAttackBoxSize;
		[SerializeField, FoldoutGroup("Knight")]
		private Vector2 KnightAttackKnockback;
		[SerializeField, FoldoutGroup("Knight")]
		private float KnightAttackDamage;
		[SerializeField, FoldoutGroup("Knight")]
		private EventReference KnightDashSound;

		private Form _form = Form.Oni;
		private bool _inSecondPhase;

		// Transform if the HP is below this threshold.
		private float _transformHPThreshold;

		private bool _isPastFirstTransformation;
		private bool _isTransforming;
		private bool _isJumping;
		private bool _didJustJump;
		private bool _didJustTP;

		protected override void Awake()
		{
			base.Awake();
			Health.AllowDamage = info => Room.HasBegun;
		}

		private IEnumerator Start()
		{
			Animator.PlayLooped(GrandpaIdle);

			yield return new WaitUntil(() => transform.position.DistanceTo(Player.position) < DialogueStartDistance);
			SpeakerRef.Speak(Dialogue);

			yield return new WaitUntil(() => Room.HasBegun);
			SpeakerRef.StopSpeaking();

			_transformHPThreshold = Health.Current - TransformHPInterval;

			TransformBlood.Play();

			yield return PlayAnimOnce(GrandpaTransform);
			_isPastFirstTransformation = true;

			TransformBlood.Stop();

			TransitionTo(OniIdleState);
		}

		protected override void OnSecondPhaseBegin()
		{
			_inSecondPhase = true;
		}

		protected override void OnDamage(DamageInfo info)
		{
			if (Health.Current < _transformHPThreshold && _isPastFirstTransformation)
			{
				TransitionTo(TransformState(GetNextForm()));
			}
		}

		protected override async void OnDeath()
		{
			await Wait.Until(() => !_isJumping);

			StopAllCoroutines();
			StartCoroutine(DeathSequence());
		}

		private IEnumerator DeathSequence()
		{
			Movement.enabled = false;
			Hitbox.enabled = false;

			ClearState();

			DeathBlood.Play();

			var initForm = _form;

			if (_form is not Form.Oni)
			{
				yield return PlayAnimOnce((_form is Form.Mage) ? FromMage : FromKnight);
			}

			var newForm = (initForm is Form.Mage) ? Form.Knight : Form.Mage;

			yield return PlayAnimOnce((newForm == Form.Mage) ? ToMage : ToKnight);
			yield return PlayAnimOnce((newForm is Form.Mage) ? FromMage : FromKnight);

			DeathBlood.Stop();

			yield return PlayAnimOnce(Death, true);
		}

		private Form GetNextForm()
		{
			if (_form is Form.Oni)
			{
				return _inSecondPhase ? Form.Knight : Form.Mage;
			}
			else
			{
				return Form.Oni;
			}
		}

		private IEnumerator TransformState(Form form)
		{
			if (_isTransforming)
				yield break;

			yield return new WaitUntil(() => !Animator.IsPlayingOneShot && !_isJumping);

			_isTransforming = true;
			_didJustJump = false;
			_didJustTP = false;

			if (form == _form)
			{
				Log.Warning($"Trying to transform to same form; {_form} -> {form}");
				yield break;
			}

			_transformHPThreshold = Health.Current - TransformHPInterval;

			if (form is Form.Oni)
			{
				yield return PlayAnimOnce((_form == Form.Mage) ? FromMage : FromKnight);
			}
			else
			{
				yield return PlayAnimOnce((form == Form.Mage) ? ToMage : ToKnight);
			}

			_form = form;
			_isTransforming = false;

			if (_form is Form.Oni)
			{
				TransitionTo(OniIdleState);
			}
			else if (_form is Form.Mage)
			{
				TransitionTo(MageIdleState);
			}
			else if (_form is Form.Knight)
			{
				TransitionTo(KnightIdleState);
			}
			else
			{
				Log.Error($"Unknown form: {_form}");
			}
		}

		private IEnumerator OniIdleState()
		{
			Animator.PlayLooped(OniIdle, true);

			for (float t = 0f; t < Random.Range(OniIdleTime.x, OniIdleTime.y); t += Time.deltaTime)
			{
				FacePlayer();
				yield return null;
			}

			if (Random.value < OniJumpOverThrowChance && !_didJustJump)
			{
				_didJustJump = true;
				TransitionTo(OniJumpState);
			}
			else
			{
				_didJustJump = false;
				TransitionTo(OniThrowState);
			}
		}

		private IEnumerator OniJumpState()
		{
			FacePlayer();
			yield return PlayAnimOnce(OniJump);
			Audio.Play(OniJumpSound, transform.position);

			float left = MageTeleportBounds.bounds.Left().x;
			float right = MageTeleportBounds.bounds.Right().x;
			var xPos = GetRandomArenaPos(left, right);

			var destination = new Vector2(xPos, transform.position.y);

			_isJumping = true;
			Movement.enabled = false;

			float halfwayPoint = Time.time + (OniJumpDuration / 2f);

			transform.DOJump(destination, OniJumpHeight, 1, OniJumpDuration)
				.SetEase(Ease.Linear)
				.OnUpdate(() =>
				{
					FacePlayer();
					Animator.PlayLooped((Time.time > halfwayPoint) ? OniFalling : OniJumping, true);
				})
				.OnComplete(() => _isJumping = false);

			yield return new WaitUntil(() => !_isJumping);

			Audio.Play(OniLandSound, transform.position);
			yield return PlayAnimOnce(OniLand);
			FacePlayer();

			Movement.enabled = true;
			TransitionTo(OniIdleState);
		}

		private IEnumerator OniThrowState()
		{
			FacePlayer();
			yield return PlayAnimOnce(OniThrow);
			TransitionTo(OniIdleState);
		}

		protected void OniThrowEvent()
		{
			FacePlayer();

			Vector2 dir = (DirectionToPlayer + (Vector2.up * 1f)).normalized;
			MissileManager.SpawnMissile(gameObject, OniMissile, TeamType.Enemy, OniThrowSpawnPoint.position, OniCastBehavior, DirectionToPlayer);
		}

		private IEnumerator MageIdleState()
		{
			Animator.PlayLooped(MageIdle, true);

			for (float t = 0f; t < Random.Range(MageIdleTime.x, MageIdleTime.y); t += Time.deltaTime)
			{
				FacePlayer();
				yield return null;
			}

			if ((Random.value < TeleportOverCastChance || DistanceToPlayer < 2.5f) && !_didJustTP)
			{
				_didJustTP = true;
				TransitionTo(MageTeleportState);
			}
			else
			{
				_didJustTP = false;
				TransitionTo(MageCastState);
			}
		}

		private IEnumerator MageCastState()
		{
			FacePlayer();
			yield return PlayAnimOnce(MageCharge);

			Audio.Play(CastSound);

			FacePlayer();
			CastVFX.Play();
			MissileManager.SpawnMissile(gameObject, MageMissile, TeamType.Enemy, CastVFX.transform.position, MageCastBehavior, DirectionToPlayer);

			yield return PlayAnimOnce(MageCast);
			TransitionTo(MageIdleState);
		}

		private IEnumerator MageTeleportState()
		{
			FacePlayer();
			yield return PlayAnimOnce(MageCharge);
			FacePlayer();

			TeleportVFX.Play();

			float left = MageTeleportBounds.bounds.Left().x;
			float right = MageTeleportBounds.bounds.Right().x;

			Audio.Play(MageTeleportSound, transform.position);

			float x = GetRandomArenaPos(left, right);
			transform.position = new(x, transform.position.y);

			TeleportVFX.Play();

			Animator.PlayLooped(MageIdle, true);
			
			for (float t = 0f; t < Random.Range(0.1f, 1f); t += Time.deltaTime)
			{
				FacePlayer();
				yield return null;
			}

			TransitionTo(MageIdleState);
		}

		private float GetRandomArenaPos(float left, float right)
		{
			// HACK: Hack way to do biased randomness.
			for (int i = 0; i < 100; i++)
			{
				float x = Random.Range(left, right);

				if (Mathf.Abs(x - transform.position.x) > 2f && Mathf.Abs(x - Player.position.x) > 4f)
				{
					return x;
				}
			}

			return Random.Range(left, right);
		}

		private IEnumerator KnightIdleState()
		{
			Animator.PlayLooped(KnightIdle, true);

			float duration = Random.Range(KnightIdleTime.x, KnightIdleTime.y);
			if (_inSecondPhase) duration *= 0.5f;

			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				FacePlayer();
				yield return null;
			}

			if (DistanceToPlayer > KnightAttackRange)
			{
				TransitionTo(KnightDashState(true));
				yield break;
			}

			if (Random.value < DashOverAttackChance)
			{
				TransitionTo(KnightDashState());
			}
			else
			{
				TransitionTo(KnightAttackState);
			}
		}

		private IEnumerator KnightDashState(bool alwaysAttack = false)
		{
			Audio.Play(KnightDashSound, transform.position);
			Animator.PlayLooped(KnightDash, true);
			float dir = Mathf.Sign(DirectionToPlayer.x);

			for (float t = 0f; t < KnightDashSpeed / KnightMaxDashDistance; t += Time.deltaTime)
			{
				FaceDirection(dir);

				if (DistanceToPlayer < KnightAttackRange)
				{
					break;
				}

				Movement.SetVelocity(KnightDashSpeed * dir * Vector2.right);
				yield return null;
			}

			if (alwaysAttack || Random.value < AttackAfterDashChance)
			{
				TransitionTo(KnightAttackState());
			}
			else
			{
				TransitionTo(KnightIdleState);
			}
		}

		private IEnumerator KnightAttackState()
		{
			for (int i = 0; i < Random.Range(AttackCount.x, AttackCount.y + 1); i++)
			{
				FacePlayer();
				var anim = i.IsEven() ? KnightAttack1 : KnightAttack2;

				yield return PlayAnimOnce(anim);
				FacePlayer();

				Animator.PlayLooped(KnightIdle);
			}

			TransitionTo(KnightIdleState);
		}

		protected void SwingHitbox()
		{
			Vector2 center = transform.position;
			Vector2 offset = KnightAttackBoxOffset;
			offset.x *= Mathf.Sign(DirectionToPlayer.x);

			Vector2 size = KnightAttackBoxSize;

			Vector2 knockback = KnightAttackKnockback;
			knockback.x *= Mathf.Sign(DirectionToPlayer.x);

			var colliders = Physics2D.OverlapBoxAll(center, size, 0f);
			foreach (var collider in colliders)
			{
				if (collider.TryGetComponent(out IDamageable damageable))
				{
					var info = new DamageInfo()
					{
						Damage = KnightAttackDamage,
						Direction = DirectionToPlayer,
						TeamType = TeamType.Enemy,
						Knockback = KnightAttackKnockback
					};
					damageable.TakeDamage(info, out bool _);
				}
			}
		}
	}
}
