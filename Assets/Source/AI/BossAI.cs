using UnityEngine;

namespace Quinn.AI
{
	public class BossAI : AgentAI
	{
		[field: SerializeField]
		public string Title { get; private set; } = "Boss Name";
	}
}
