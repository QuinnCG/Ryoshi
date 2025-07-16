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
			TransitionTo(PatrolState);

			yield return new WaitUntil(() => DistanceToPlayer < 6f);
			TransitionTo(IdleState);
		}

		private IEnumerator PatrolState()
		{
			Animator.PlayLooped(IdlingAnim);

			while (true)
			{
				yield return null;
			}
		}

		private IEnumerator IdleState()
		{
			Animator.PlayLooped(IdlingAnim);

			yield return new WaitForSeconds(Random.Range(0.1f, 1f));
			TransitionTo(ChaseState);
		}

		private IEnumerator ChaseState()
		{
			Movement.SetSpeedRun();
			Animator.PlayLooped(RunningAnim);

			while(true)
			{
				// Reached.
				if (Movement.MoveTo(Player.position, 1f))
				{
					TransitionTo(AttackState);
				}

				yield return null;
			}
		}
		
		private IEnumerator AttackState()
		{
			Movement.DashTowards(DirectionToPlayer.x, 12f, 1f);
			yield return PlayAnimOnce(AttackAnim1);

			yield return WaitUntil(() => ReadEvent("damage"), 2f);
			Log.Info("Damage applied!");

			TransitionTo(IdleState);
		}
	}
}
