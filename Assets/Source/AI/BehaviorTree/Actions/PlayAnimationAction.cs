using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Play Animation", story: "Play Animation [Anim]", category: "Action", id: "bfec0c57b3081f3585a412be0c133049")]
	public partial class PlayAnimationAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<AnimationClip> Anim;

		[SerializeReference]
		public BlackboardVariable<bool> WaitForAnimation = new(false);

		private float _animEndTime;

		protected override Status OnStart()
		{
			var animator = GameObject.GetComponent<PlayableAnimator>();

			if (Anim.Value.isLooping)
			{
				animator.PlayLooped(Anim.Value);
			}
			else
			{
				animator.PlayOnce(Anim.Value);
			}

			_animEndTime = Time.time + Anim.Value.length;
			return WaitForAnimation.Value ? Status.Running : Status.Success;
		}

		protected override Status OnUpdate()
		{
			return Time.time < _animEndTime ? Status.Running : Status.Success;
		}
	}
}
