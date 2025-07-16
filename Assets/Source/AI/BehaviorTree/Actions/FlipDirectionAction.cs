using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Flip Direction", story: "Flip Direction [Dir]", category: "Action", id: "995fe1bad05afcb51c96f89dd90c0643")]
    public partial class FlipDirectionAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<int> Dir;

        protected override Status OnStart()
        {
            Dir.Value *= -1;
            return Status.Success;
        }
    }
}
