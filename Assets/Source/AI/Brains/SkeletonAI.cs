using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace Quinn.AI.Brains
{
	public class SkeletonAI : AgentAI
	{
		[SerializeField]
		private int AttackDamage = 1;
		[SerializeField]
		private Vector2 AttackKnockback = new(12f, 0f);
		[SerializeField]
		private float AttackDashSpeed = 6f;
		[SerializeField]
		private float RetreatChance = 0.3f;

		[Space]

		[SerializeField]
		private float PlayMessageChance = 0.5f;
		[SerializeField]
		private string[] RandomFirstMessage;

		[Space]

		[SerializeField]
		private Vector2 IdleDuration = new(0.5f, 3f);

		[SerializeField, FoldoutGroup("Animations")]
		private AnimationClip IdlingAnim, WalkingAnim, RunningAnim;
		[SerializeField, FoldoutGroup("Animations")]
		private AnimationClip AttackAnim1, AttackAnim2;

		private IEnumerator Start()
		{
			TransitionTo(PatrolState, "Patrol");
			yield return new WaitUntil(() => DistanceToPlayer < 6f);

			if (Random.value < PlayMessageChance)
			{
				ClearState();

				Animator.PlayLooped(IdlingAnim);
				FacePlayer();

				float duration = Speak(RandomFirstMessage.GetRandom());
				yield return new WaitForSeconds(duration);
			}

			TransitionTo(IdleState, "Idle");
		}

		protected override void OnSecondPhaseBegin()
		{
			if (Random.value < 0.5f)
			{
				TransitionTo(FleeState, "Flee");
			}
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

			for (float t = 0f; t < Random.Range(IdleDuration.x, IdleDuration.y); t += Time.deltaTime)
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
					TransitionTo(AttackState, "Chase -> Attack");
				}

				yield return null;
			}
		}
		
		private IEnumerator AttackState()
		{
			FacePlayer();
			Animator.PlayOnce(AttackAnim1);

			yield return WaitUntil(() => ReadEvent("dash"), 2f);
			FacePlayer();

			float dir = DirectionToPlayer.x;
			float timeoutTime = Time.time + 2f;

			while (!ReadEvent("damage") && Time.time < timeoutTime)
			{
				Movement.SetVelocity(dir * AttackDashSpeed * Vector2.right);
				yield return null;
			}

			FacePlayer();
			DamageBox(new(1f, 0f), new(2f, 1.5f), AttackDamage, DirectionToPlayer.x * Vector2.right, AttackKnockback);

			if (Random.value < RetreatChance)
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
