using Quinn.MissileSystem;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace Quinn.AI.Brains
{
	public class CasterAI : AgentAI
	{
		[SerializeField, Required, FoldoutGroup("Animations")]
		private AnimationClip IdleAnim, WalkAnim, WalkBackAnim, CastChargeAnim, CastAnim, DeathAnim;

		[SerializeField]
		private GameObject MissilePrefab;
		[SerializeField]
		private Transform MissileOrigin;
		[SerializeField]
		private MissileSpawnBehavior MissileSpawnBehavior;

		protected override void OnDeath()
		{
			ClearState();

			Animator.PlayOnce(DeathAnim, true);
			Movement.enabled = false;
			Hitbox.excludeLayers = LayerMask.GetMask(Layers.CharacterName);
		}

		private IEnumerator Start()
		{
			TransitionTo(PatrolState, "Patrol");

			yield return new WaitUntil(() => DistanceToPlayer < 6f);
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

			if (DistanceToPlayer < 6f)
			{
				for (float t = 0f; t < Random.Range(0.5f, 2f); t += Time.deltaTime)
				{
					FacePlayer();
					Movement.MoveTowards(-DirectionToPlayer.x);

					Animator.PlayLooped(WalkBackAnim);

					yield return null;
				}
			}
			else if (DistanceToPlayer > 10f)
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
			Animator.PlayLooped(CastChargeAnim);
			yield return new WaitForSeconds(Random.Range(2f, 4f));

			Animator.PlayOnce(CastAnim);
			MissileManager.SpawnMissile(gameObject, MissilePrefab, DamageSystem.TeamType.Enemy, MissileOrigin.position, MissileSpawnBehavior, DirectionToPlayer);

			yield return new WaitForSeconds(CastAnim.length);
			TransitionTo(IdleState, "Attack -> Idle");
		}
	}
}
