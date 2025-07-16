using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Get Random Direction", story: "Get Random Direction as [Dir]", category: "Action", id: "5efc11066f3c7d8ac494ed8792406c55")]
    public partial class GetRandomDirectionAction : Action
    {
        [SerializeReference] 
        public BlackboardVariable<int> Dir;

        protected override Status OnStart()
        {
            Dir.Value = UnityEngine.Random.value > 0.5f ? 1 : -1;
            return Status.Success;
        }
    }
}
