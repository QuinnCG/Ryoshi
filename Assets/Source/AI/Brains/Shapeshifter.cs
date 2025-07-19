using Quinn.DamageSystem;
using Quinn.MissileSystem;
using Quinn.UI;
using Sirenix.OdinInspector;
using System.Collections;
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

		[SerializeField, Required, FoldoutGroup("Grandpa")]
		private AnimationClip GrandpaIdle, GrandpaTransform;

		[SerializeField, Required, FoldoutGroup("Oni")]
		private AnimationClip OniIdle, OniThrow, ToMage, ToKnight;
		[SerializeField, FoldoutGroup("Oni")]
		private Vector2 OniIdleTime = new(0.5f, 2f);
		[SerializeField, Required, FoldoutGroup("Oni")]
		private Transform OniThrowSpawnPoint;
		[SerializeField, FoldoutGroup("Oni")]
		private MissileSpawnBehavior OniCastBehavior;
		[SerializeField, Required, FoldoutGroup("Oni")]
		private GameObject OniMissile;

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

		[SerializeField, Required, FoldoutGroup("Knight")]
		private AnimationClip KnightIdle, FromKnight;

		private Form _form = Form.Oni;
		private bool _inSecondPhase;

		// Transform if the HP is below this threshold.
		private float _transformHPThreshold;

		private bool _isTransforming;

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

			yield return PlayAnimOnce(GrandpaTransform);
			TransitionTo(OniIdleState);
		}

		protected override void OnSecondPhaseBegin()
		{
			_inSecondPhase = true;
		}

		protected override void OnDamage(DamageInfo info)
		{
			if (Health.Current < _transformHPThreshold)
			{
				TransitionTo(TransformState(GetRandomTransform()));
			}
		}

		private Form GetRandomTransform()
		{
			if (_form is Form.Oni)
			{
				
				if (_inSecondPhase)
				{
					return (Random.value < 0.5f) ? Form.Mage : Form.Knight;
				}

				return Form.Mage;
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

			yield return new WaitUntil(() => !Animator.IsPlayingOneShot);

			_isTransforming = true;

			if (form == _form)
			{
				Log.Warning($"Trying to transform to same form; {_form} -> {form}");
				yield break;
			}

			_form = form;

			_transformHPThreshold = Health.Current - TransformHPInterval;

			if (form is Form.Oni)
			{
				yield return PlayAnimOnce(_form == Form.Mage ? FromMage : FromKnight);
			}
			else
			{
				yield return PlayAnimOnce(form == Form.Mage ? ToMage : ToKnight);
			}

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

			TransitionTo(OniThrowState);
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

			if (Random.value < TeleportOverCastChance)
			{
				TransitionTo(MageTeleportState);
			}
			else
			{
				TransitionTo(MageCastState);
			}
		}

		private IEnumerator MageCastState()
		{
			FacePlayer();
			yield return PlayAnimOnce(MageCharge);

			FacePlayer();
			CastVFX.Play();
			MissileManager.SpawnMissile(gameObject, MageMissile, TeamType.Enemy, CastVFX.transform.position, MageCastBehavior, DirectionToPlayer);

			yield return PlayAnimOnce(MageCast);
			TransitionTo(MageIdleState);
		}

		private IEnumerator MageTeleportState()
		{
			yield return PlayAnimOnce(MageCharge);

			TeleportVFX.Play();

			float left = MageTeleportBounds.bounds.Left().x;
			float right = MageTeleportBounds.bounds.Right().x;

			float x = Random.Range(left, right);
			transform.position = new(x, transform.position.y);

			TeleportVFX.Play();

			TransitionTo(MageIdleState);
		}

		private IEnumerator KnightIdleState()
		{
			yield break;
		}
	}
}
