using System;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
	[Serializable, Unity.Properties.GeneratePropertyBag]
	[Condition(name: "Is Player Within Range", story: "Is Player Within Range [Dst]", category: "Conditions", id: "c534eae9a4ce9153732cdca72e512e7a")]
	public partial class IsPlayerWithinRangeCondition : Condition
	{
		[SerializeReference]
		public BlackboardVariable<float> Dst;

		public override bool IsTrue()
		{
			float dst = Player.Instance.transform.position.DistanceTo(GameObject.transform.position);
			return dst <= Dst.Value;
		}
	}
}
