using UnityEngine;

namespace Quinn.DamageSystem
{
	public record DamageInfo
	{
		public int Damage = 1;
		public Vector2 Direction;
		public TeamType TeamType;
	}
}
