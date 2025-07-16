using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Dash in Direction", story: "Dash in Direction [Dir] for Distance [Dst] at Speed [Spd]", category: "Action", id: "bd2b5eb9d859dbf79644a63998b2e0d7")]
	public partial class DashInDirectionAction : Action
	{
		[SerializeReference] 
		public BlackboardVariable<int> Dir = new(1);
		[SerializeReference]
		public BlackboardVariable<float> Dst = new(3f);
		[SerializeReference]
		public BlackboardVariable<float> Spd = new(12f);

		private EnemyMovement _movement;
		private float _dashEndTime;

		protected override Status OnStart()
		{
			_movement = GameObject.GetComponent<EnemyMovement>();
			_dashEndTime = Time.time + Dst.Value / Spd.Value;

			return Status.Running;
		}

		protected override Status OnUpdate()
		{
			_movement.MoveTowards(Dir.Value, Spd.Value);
			return Time.time < _dashEndTime ? Status.Running : Status.Success;
		}
	}
}
