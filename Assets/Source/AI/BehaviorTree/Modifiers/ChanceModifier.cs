using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "Chance", story: "Execute [Percent] of the Time", category: "Flow", id: "8df118e04d47ea9f8b4b88f717adac43")]
	public partial class ChanceModifier : Modifier
	{
		[SerializeReference]
		public BlackboardVariable<float> Percent = new(100f);

		protected override Status OnStart()
		{
			if (UnityEngine.Random.value <= Percent.Value / 100f)
			{
				return Status.Failure;
			}

			if (Child != null)
			{
				return StartNode(Child);
			}

			return Status.Failure;
		}

		protected override Status OnUpdate()
		{
			if (Child != null)
			{
				var status = Child.CurrentStatus;

				if (status is not Status.Running)
				{
					if (Child != null)
					{
						EndNode(Child);
					}
				}
			}

			return Status.Success;
		}
	}
}
