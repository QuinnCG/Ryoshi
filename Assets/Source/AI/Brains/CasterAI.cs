using FMODUnity;
using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace Quinn.AI.Brains
{
	public class CasterAI : AgentAI
	{
		[SerializeField]
		private GameObject MissilePrefab;
		[SerializeField]
		private Transform MissileOrigin;
		[SerializeField]
		private MissileSpawnBehavior MissileSpawnBehavior;
		[SerializeField, ChildGameObjectsOnly]
		private VisualEffect CastVFX;
		[SerializeField]
		private EventReference AlertSound;

		[Space]

		[SerializeField]
		private float MinIdealDstToPlayer = 6f, MaxIdealDstToPlayer = 10f;
		[SerializeField]
		private Vector2 ChargeDuration = new(0.5f, 2f);

		[SerializeField, Required, FoldoutGroup("Animations")]
		private AnimationClip IdleAnim, WalkAnim, WalkBackAnim, CastChargeAnim, CastAnim, DeathAnim;

		protected override void OnDeath()
		{
			ClearState();

			Animator.PlayOnce(DeathAnim, true);
			Movement.enabled = false;
			Hitbox.enabled = false;
		}

		private IEnumerator Start()
		{
			TransitionTo(PatrolState, "Patrol");

			yield return new WaitUntil(() => DistanceToPlayer < 6f);

			Audio.Play(AlertSound, transform.position);

			TransitionTo(IdleState, "Idle");
		}

		private IEnumerator PatrolState()
		{
			Animator.PlayLooped(IdleAnim);
			yield break;
		}

		private IEnumerator IdleState()
		{
			Animator.PlayLooped(IdleAnim);
			yield return new WaitForSeconds(Random.Range(0f, 1f));

			if (DistanceToPlayer < MinIdealDstToPlayer)
			{
				for (float t = 0f; t < Random.Range(0.5f, 2f); t += Time.deltaTime)
				{
					FacePlayer();
					Movement.MoveTowards(-DirectionToPlayer.x);

					Animator.PlayLooped(WalkBackAnim);

					yield return null;
				}
			}
			else if (DistanceToPlayer > MaxIdealDstToPlayer)
			{
				for (float t = 0f; t < Random.Range(0.5f, 1f); t += Time.deltaTime)
				{
					FacePlayer();
					Movement.MoveTowards(DirectionToPlayer.x);

					Animator.PlayLooped(WalkAnim);

					yield return null;
				}
			}

			TransitionTo(AttackState, "Idle -> Attack");
		}

		private IEnumerator AttackState()
		{
			FacePlayer();

			Animator.PlayLooped(CastChargeAnim);
			yield return new WaitForSeconds(Random.Range(ChargeDuration.x, ChargeDuration.y));

			FacePlayer();

			Animator.PlayOnce(CastAnim);
			MissileManager.SpawnMissile(gameObject, MissilePrefab, DamageSystem.TeamType.Enemy, MissileOrigin.position, MissileSpawnBehavior, DirectionToPlayer);

			CastVFX.Play();

			yield return new WaitForSeconds(CastAnim.length);
			TransitionTo(IdleState, "Attack -> Idle");
		}
	}
}
