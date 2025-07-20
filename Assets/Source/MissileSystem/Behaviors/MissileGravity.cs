using UnityEngine;

namespace Quinn.MissileSystem.Behaviors
{
	public class MissileGravity : MissileBehavior
	{
		[field: SerializeField]
		public float Gravity { get; set; } = 0f;
		[SerializeField]
		private float MaxGravity = 30f;

		private float _gravity;

		public override void OnUpdate()
		{
			_gravity += Gravity * Time.deltaTime;
			_gravity = Mathf.Min(_gravity, MaxGravity);

			Missile.AddVelocity(Vector2.down * _gravity);
		}
	}
}
