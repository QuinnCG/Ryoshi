using System.Collections.Generic;
using UnityEngine;

namespace Quinn.AI
{
	public class AgentAI : MonoBehaviour
	{
		private readonly HashSet<string> _events = new();

		/// <summary>
		/// Trigger a named event for the behavior tree to read.<br/>
		/// Events are reset upon being read.
		/// </summary>
		public void TriggerEvent(string name)
		{
			_events.Add(name);
		}

		/// <summary>
		/// Reset an event early. This is normally done automatically when the even is read.
		/// </summary>
		public void ResetEvent(string name)
		{
			_events.Remove(name);
		}

		public void ResetAllEvents()
		{
			_events.Clear();
		}

		/// <summary>
		/// Read and remove the specified event, if it exists.
		/// </summary>
		/// <returns>True, if the event was found and removed. False, if the event wasn't triggered yet.</returns>
		public bool ReadEvent(string name)
		{
			if (_events.Contains(name))
			{
				_events.Remove(name);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Check for the existance of an event, without removing it.
		/// </summary>
		public bool PeakEvent(string name)
		{
			return _events.Contains(name);
		}
	}
}
