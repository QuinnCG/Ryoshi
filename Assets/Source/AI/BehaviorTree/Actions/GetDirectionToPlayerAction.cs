using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Get Direction to Player", story: "Get Direction to Player as Var [Dir]", category: "Action", id: "a0c707688918cb21887e17ece755ed48")]
    public partial class GetDirectionToPlayerAction : Action
    {
        [SerializeReference] 
        public BlackboardVariable<int> Dir;

        protected override Status OnStart()
        {
            float dir = GameObject.transform.position.DirectionTo(Player.Instance.transform.position).x;
            Dir.Value = Mathf.RoundToInt(Mathf.Sign(dir));

            return Status.Success;
        }
    }
}
