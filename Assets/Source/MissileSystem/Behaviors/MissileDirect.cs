using UnityEngine;

namespace Quinn.MissileSystem.Behaviors
{
	public class MissileDirect : MissileBehavior
	{
		[field: SerializeField]
		public float Speed { get; set; } = 8f;

		public override void OnUpdate()
		{
			Missile.AddVelocity(Speed * Missile.BaseDirection);
		}
	}
}
