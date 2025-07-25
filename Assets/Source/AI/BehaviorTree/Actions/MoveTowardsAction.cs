using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move Towards", story: "Move in Direction [Dir]", category: "Action", id: "0ea0d1a4d3bcbd05cd71d63eabe04925")]
    public partial class MoveTowardsAction : Action
    {
        [SerializeReference] 
        public BlackboardVariable<int> Dir;

		[SerializeReference]
		public BlackboardVariable<bool> FaceDirection = new(true);

		private EnemyMovement _movement;

        protected override Status OnStart()
        {
            _movement = GameObject.GetComponent<EnemyMovement>();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            float dir = Mathf.Sign(Dir.Value);

			_movement.MoveTowards(dir);

			if (FaceDirection.Value)
			{
				_movement.SetFacingDir(dir);
			}

			return Status.Running;
        }
    }
}
