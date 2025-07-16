using Quinn.DamageSystem;
using System;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Is Dead", story: "Is Dead", category: "Conditions", id: "4d4460fff6922fc51db5ec40deb2d276")]
    public partial class IsDeadCondition : Condition
    {
        public override bool IsTrue()
        {
            return GameObject.GetComponent<Health>().IsDead;
        }
    }
}
