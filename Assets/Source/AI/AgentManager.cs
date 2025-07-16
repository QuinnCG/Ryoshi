using QFSW.QC;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn.AI
{
	public class AgentManager : MonoBehaviour
	{
		public static AgentManager Instance { get; private set; }

		private readonly HashSet<AgentAI> _agents = new();

		private void Awake()
		{
			Instance = this;
		}

		public void RegisterAgent(AgentAI agent)
		{
			_agents.Add(agent);
		}

		public void UnregisterAgent(AgentAI agent)
		{
			_agents.Remove(agent);
		}

		[Command("agent.state", "Set whether agent's will have text above their head displaying their current AI state or not.")]
		protected void SetAgentStateDebug_Cmd(bool display)
		{
			foreach (var agent in  _agents)
			{
				if (agent != null)
				{
					agent.SetAgentStateDisplay(display);
				}
			}	
		}
	}
}
