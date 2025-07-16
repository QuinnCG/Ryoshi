using Quinn.MovementSystem;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Move Towards Target", story: "Move Towards [Target]", category: "Action", id: "892e554b3d5c02c602ccbb7ba34f9ad7")]
	public partial class MoveTowardsTargetAction : Action
	{
		[SerializeReference] 
		public BlackboardVariable<Transform> Target;

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
			float dir = Target.Value.position.x - GameObject.transform.position.x;
			_movement.MoveTowards(dir);

			if (FaceDirection.Value)
			{
				_movement.SetFacingDir(dir);
			}

			return Status.Running;
		}
	}
}
