using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Set Move Mode", story: "Set Move Mode to [M]", category: "Action", id: "1c6e5a7d7d1b4efec4ed1bf412be41bb")]
    public partial class SetMoveModeAction : Action
    {
        [SerializeReference] 
        public BlackboardVariable<MoveMode> M;

        protected override Status OnStart()
        {
            var movement = GameObject.GetComponent<EnemyMovement>();

            if (M.Value is MoveMode.Walking)
                movement.SetSpeedWalk();
            else
                movement.SetSpeedRun();

                return Status.Success;
        }
    }
}
