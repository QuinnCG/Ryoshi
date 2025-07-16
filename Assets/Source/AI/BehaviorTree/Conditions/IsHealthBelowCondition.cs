using Quinn.DamageSystem;
using System;
using Unity.Behavior;
using UnityEngine;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Is Health Below", story: "Is Health Below Normal Value [HP]", category: "Conditions", id: "c928c1e71c6c27ff12b4d734122367f4")]
    public partial class IsHealthBelowCondition : Condition
    {
        [SerializeReference]
        public BlackboardVariable<float> HP;

        public override bool IsTrue()
        {
            return GameObject.GetComponent<Health>().Normalized <= HP.Value;
        }
    }
}
