using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Get Player Transform", story: "Get Player Transform as Var [T]", category: "Action", id: "273e98199d2ccea7da47a44d921994af")]
    public partial class GetPlayerTransformAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<Transform> T;

        protected override Status OnStart()
        {
            T.Value = Player.Instance.transform;
            return Status.Success;
        }
    }
}
