using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Play Sound", story: "Play Sound [Name]", category: "Action", id: "4941487026c43a5b1a3ff4f15ac08f73")]
    public partial class PlaySoundAction : Action
    {
        [SerializeReference] 
        public BlackboardVariable<string> Name;

        protected override Status OnStart()
        {
            Audio.Play(Name.Value, GameObject.transform.position);
            return Status.Success;
        }
    }
}
