using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Quinn.MovementSystem;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move Towards", story: "Move in Direction [Dir]", category: "Action", id: "0ea0d1a4d3bcbd05cd71d63eabe04925")]
    public partial class MoveTowardsAction : Action
    {
        [SerializeReference] 
        public BlackboardVariable<int> Dir;

        private EnemyMovement _movement;

        protected override Status OnStart()
        {
            _movement = GameObject.GetComponent<EnemyMovement>();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            _movement.MoveTowards(Mathf.Sign(Dir.Value));
            return Status.Running;
        }
    }
}
