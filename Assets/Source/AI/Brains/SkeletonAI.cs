using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace Quinn.AI.Brains
{
	public class SkeletonAI : AgentAI
	{
		[SerializeField, FoldoutGroup("Animations")]
		private AnimationClip IdlingAnim, WalkingAnim, RunningAnim;
		[SerializeField, FoldoutGroup("Animations")]
		private AnimationClip AttackAnim1, AttackAnim2;

		private IEnumerator Start()
		{
			TransitionTo(PatrolState, "Patrol");

			yield return new WaitUntil(() => DistanceToPlayer < 6f);
			TransitionTo(IdleState, "Idle");
		}

		protected override void OnSecondPhaseBegin()
		{
			TransitionTo(FleeState);
		}

		private IEnumerator PatrolState()
		{
			Movement.SetSpeedWalk();

			while (true)
			{
				Animator.PlayLooped(IdlingAnim);

				yield return new WaitForSeconds(Random.Range(0.3f, 3f));

				float dir = Random.value < 0.5f ? 1f : -1f;
				FaceDirection(dir);
				Animator.PlayLooped(WalkingAnim);

				for (float t = 0f; t < 1f; t += Time.deltaTime)
				{
					Movement.MoveTowards(dir);
					yield return null;
				}
			}
		}

		private IEnumerator RetreatState()
		{
			Animator.PlayLooped(IdlingAnim);
			FacePlayer();
			yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));

			Animator.PlayLooped(WalkingAnim);

			FaceAwayFromPlayer();
			float dir = -DirectionToPlayer.x;

			for (float t = 0f; t < Random.Range(0.5f, 1.5f); t += Time.deltaTime)
			{
				Movement.MoveTowards(dir);
				yield return null;
			}

			TransitionTo(IdleState, "Retreat -> Idle");
		}

		private IEnumerator IdleState()
		{
			Animator.PlayLooped(IdlingAnim);

			for (float t = 0f; t < Random.Range(0.5f, 3f); t += Time.deltaTime)
			{
				FacePlayer();
				yield return null;
			}

			TransitionTo(ChaseState, "Idle -> Chase");
		}

		private IEnumerator ChaseState()
		{
			Movement.SetSpeedRun();
			Animator.PlayLooped(RunningAnim);

			while(true)
			{
				FacePlayer();

				// Reached.
				if (Movement.MoveTo(Player.position, 3f))
				{
					TransitionTo(AttackState(0), "Chase -> Attack");
				}

				yield return null;
			}
		}
		
		private IEnumerator AttackState(int recursionDepth = 0)
		{
			var anim = PlayAnimOnce(AttackAnim1);

			FacePlayer();

			yield return new WaitUntil(() => ReadEvent("dash"));
			FacePlayer();

			float dir = DirectionToPlayer.x;
			while (!ReadEvent("damage"))
			{
				Movement.SetVelocity(dir * 6f * Vector2.right);
				yield return null;
			}

			FacePlayer();
			DamageBox(new(1f, 0f), new(2f, 1.5f), 1, DirectionToPlayer.x * Vector2.right, new(12f, 0f));

			yield return anim;

			if (recursionDepth < 3 && Random.value < 0.5f)
			{
				TransitionTo(AttackState(recursionDepth + 1), "Attack -> Attack");
			}
			else if (Random.value < 0.3f)
			{
				TransitionTo(RetreatState, "Attack -> Retreat");
			}
			else
			{
				TransitionTo(IdleState, "Attack -> Idle");
			}
		}

		private IEnumerator FleeState()
		{
			Animator.StopOneShot();
			Movement.SetSpeedRun();

			while(true)
			{
				Animator.PlayLooped(RunningAnim);

				for (float t = 0f; t < Random.Range(1f, 4f); t += Time.deltaTime)
				{
					FaceAwayFromPlayer();
					Movement.MoveTowards(-DirectionToPlayer.x);

					yield return null;
				}

				if (DistanceToPlayer > 5f)
				{
					Animator.PlayLooped(IdlingAnim);
					FacePlayer();

					yield return new WaitForSeconds(Random.Range(0.5f, 1f));
					yield return WaitUntil(() => DistanceToPlayer < 5f, 2f);
				}
			}
		}
	}
}
