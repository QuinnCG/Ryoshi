using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Play Movement Animation", story: "Play Movement Animations [Idling] and [Moving]", category: "Action", id: "a2abb270ea8b17f718ce75ecb0ca9f74")]
    public partial class PlayMovementAnimationAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<AnimationClip> Idling;
        [SerializeReference]
        public BlackboardVariable<AnimationClip> Moving;

        private PlayableAnimator _animator;
        private EnemyMovement _movement;

        protected override Status OnStart()
        {
            _animator = GameObject.GetComponent<PlayableAnimator>();
            _movement = GameObject.GetComponent<EnemyMovement>();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            _animator.PlayLooped(_movement.Velocity.x != 0f ? Moving.Value : Idling.Value);
            return Status.Running;
        }
    }
}
