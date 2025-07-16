using System;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Is Player Out of Range", story: "Is Player Out of Range [Dst]", category: "Conditions", id: "23576c71068c4c7b3cb1a92bcd74c1da")]
    public partial class IsPlayerOutOfRangeCondition : Condition
    {
        [SerializeReference] 
        public BlackboardVariable<float> Dst;

        public override bool IsTrue()
        {
            float dst = GameObject.transform.position.DistanceTo(Player.Instance.transform.position);
            return dst > Dst.Value;
        }
    }
}
