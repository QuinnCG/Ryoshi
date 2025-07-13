using UnityEngine;

namespace Quinn.DamageSystem
{
	public class Team : MonoBehaviour
	{
		[field: SerializeField]
		public TeamType Type { get; private set; }
	}
}
