using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Quinn.DamageSystem;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Apply Damage", story: "Apply Damage [D] and Knockback [K] at Offset [O] Size [S]", category: "Action", id: "1db1d464cb447dd8c98a8e79450ab2cf")]
    public partial class ApplyDamageAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<int> D = new(1);
        [SerializeReference]
        public BlackboardVariable<Vector2> K = new(new(8f, 0f));
        [SerializeReference]
        public BlackboardVariable<Vector2> O = new(new(1f, 0f));
        [SerializeReference]
        public BlackboardVariable<Vector2> S = new(new(2f, 2f));

        protected override Status OnStart()
        {
            var movement = GameObject.GetComponent<EnemyMovement>();

            Vector2 offset = O;
            offset.x *= movement.FacingDirection;

            Vector2 center = (Vector2)GameObject.transform.position + offset;

            var colliders = Physics2D.OverlapBoxAll(center, S.Value, 0f);

            foreach (var collider in colliders)
            {
                if (collider.gameObject != GameObject && collider.TryGetComponent(out IDamageable dmg))
                {
                    dmg.TakeDamage(new()
                    {
                        Damage = D.Value,
                        Direction = GameObject.transform.position.DirectionTo(collider.bounds.Bottom()),
                        Knockback = K.Value,
                        TeamType = TeamType.Enemy
                    });
                }
            }

            return Status.Success;
        }
    }
}
