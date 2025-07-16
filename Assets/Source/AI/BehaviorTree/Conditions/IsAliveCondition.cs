using Quinn.DamageSystem;
using System;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Is Alive", story: "Is Alive", category: "Conditions", id: "efda60927da9450b6b3c631106be87b7")]
    public partial class IsAliveCondition : Condition
    {
        public override bool IsTrue()
        {
            return GameObject.GetComponent<Health>().IsAlive;
        }
    }
}
