using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Face Player", story: "Face Player", category: "Action", id: "4164bb94b86c0afc5e314bce0498002f")]
    public partial class FacePlayerAction : Action
    {
        private Transform _player;
        private EnemyMovement _movement;

        protected override Status OnStart()
        {
            _player = Player.Instance.transform;
            _movement = GameObject.GetComponent<EnemyMovement>();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            _movement.SetFacingDir(_player.transform.position.x - GameObject.transform.position.x);
            return Status.Running;
        }
    }
}
