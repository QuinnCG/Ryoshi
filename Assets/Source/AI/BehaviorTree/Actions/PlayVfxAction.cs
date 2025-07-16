using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.VFX;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Play VFX", story: "Play [VFX]", category: "Action", id: "f2e11455138c0ff904512f518e3589c0")]
    public partial class PlayVfxAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<VisualEffect> VFX;

        protected override Status OnStart()
        {
            VFX.Value.Play();
            return Status.Success;
        }
    }
}
