using UnityEngine;

namespace Quinn.DamageSystem
{
	public record DamageInfo
	{
		public float Damage = 1f;
		public Vector2 Direction;
		public TeamType TeamType;
	}
}
