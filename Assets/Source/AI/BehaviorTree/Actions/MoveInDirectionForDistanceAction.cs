using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using Action = Unity.Behavior.Action;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move in Direction for Distance", story: "Move in Direction [Dir] for Distance [Dst]", category: "Action", id: "70a9c75510c2bc3249a4b8f038125ec9")]
    public partial class MoveInDirectionForDistanceAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<int> Dir;
        [SerializeReference]
        public BlackboardVariable<float> Dst;

		[SerializeReference]
		public BlackboardVariable<bool> FaceDirection = new(true);

		private EnemyMovement _movement;
        private float _endTime;

        protected override Status OnStart()
        {
            _movement = GameObject.GetComponent<EnemyMovement>();
            _endTime = Time.time + Dst.Value / _movement.MoveSpeed;

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            _movement.MoveTowards(Dir.Value);

			if (FaceDirection.Value)
			{
				_movement.SetFacingDir(Dir.Value);
			}

			return Time.time < _endTime ? Status.Running : Status.Success;
        }
    }
}
