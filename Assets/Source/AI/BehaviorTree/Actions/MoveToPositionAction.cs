using Quinn.MovementSystem;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Move to Position", story: "Move to [Position]", category: "Action", id: "b935d4655bcd2664904056a00285689e")]
	public partial class MoveToPositionAction : Action
	{
		[SerializeReference]
		public BlackboardVariable<Vector2> Position;

		[SerializeReference]
		public BlackboardVariable<float> StoppingDistance = new(0.2f);

		private EnemyMovement _movement;

		protected override Status OnStart()
		{
			_movement = GameObject.GetComponent<EnemyMovement>();
			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			bool reached = _movement.MoveTo(Position.Value, StoppingDistance);
			return reached ? Status.Success : Status.Running;
		}
	}
}
