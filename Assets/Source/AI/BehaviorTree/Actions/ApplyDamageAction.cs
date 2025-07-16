using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Quinn.DamageSystem;
using Quinn.MovementSystem;

namespace Quinn.AI.BehaviorTree
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Apply Damage", story: "Apply [Damage] and [Knockback] at [Offset] [Size]", category: "Action", id: "1db1d464cb447dd8c98a8e79450ab2cf")]
    public partial class ApplyDamageAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<int> Damage = new(1);
        [SerializeReference]
        public BlackboardVariable<Vector2> Knockback = new(new(8f, 0f));
        [SerializeReference]
        public BlackboardVariable<Vector2> Offset = new(new(1f, 0f));
        [SerializeReference]
        public BlackboardVariable<Vector2> Size = new(new(2f, 2f));

        protected override Status OnStart()
        {
            var movement = GameObject.GetComponent<EnemyMovement>();

            Vector2 offset = Offset;
            offset.x *= movement.FacingDirection;

            Vector2 center = (Vector2)GameObject.transform.position + offset;

            var colliders = Physics2D.OverlapBoxAll(center, Size.Value, 0f);

            foreach (var collider in colliders)
            {
                if (collider.gameObject != GameObject && collider.TryGetComponent(out IDamageable dmg))
                {
                    dmg.TakeDamage(new()
                    {
                        Damage = Damage.Value,
                        Direction = GameObject.transform.position.DirectionTo(collider.bounds.Bottom()),
                        Knockback = Knockback.Value,
                        TeamType = TeamType.Enemy
                    });
                }
            }

            return Status.Success;
        }
    }
}
