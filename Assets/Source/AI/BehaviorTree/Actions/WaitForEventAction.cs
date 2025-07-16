using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Wait for Event", story: "Wait for Event [Name]", category: "Action", id: "43515d4cf79fdaaf531774562b4fc7df")]
    public partial class WaitForEventAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<string> Name;

        protected override Status OnStart()
        {
            GameObject.GetComponent<AgentAI>().TriggerEvent(Name.Value);
            return Status.Success;
        }
    }
}
