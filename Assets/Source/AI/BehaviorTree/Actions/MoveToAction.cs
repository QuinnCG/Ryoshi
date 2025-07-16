using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Move To", story: "Move to [Target]", category: "Action", id: "1f7ac2c5dfbb78fbef50a005fa8eacbd")]
	public partial class MoveToAction : Action
	{
		[SerializeReference] 
		public BlackboardVariable<Transform> Target;

		[SerializeReference]
		public BlackboardVariable<float> StoppingDistance = new(0.2f);
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
			bool reached = _movement.MoveTo(Target.Value.position, StoppingDistance);

			if (FaceDirection.Value)
			{
				_movement.SetFacingDir(Target.Value.position.x - GameObject.transform.position.x);
			}

			return reached ? Status.Success : Status.Running;
		}
	}
}
