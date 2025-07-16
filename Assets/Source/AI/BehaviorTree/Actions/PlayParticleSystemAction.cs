using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Play Particle System", story: "Play [ParticleSystem]", category: "Action", id: "3303b9c7ca07ff73b1c84bab9f5df6eb")]
    public partial class PlayParticleSystemAction : Action
    {
        [SerializeReference] 
        public BlackboardVariable<ParticleSystem> ParticleSystem;

        protected override Status OnStart()
        {
            ParticleSystem.Value.Play();
            return Status.Success;
        }
    }
}
