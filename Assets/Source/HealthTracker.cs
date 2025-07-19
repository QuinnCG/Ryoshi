using Quinn.DamageSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Quinn.AI
{
	public class HealthTracker : MonoBehaviour
	{
		[SerializeField]
		private UnityEvent OnAllDead;
		[SerializeField]
		private List<Health> Healths;

		private void Awake()
		{
			foreach (var health in Healths)
			{
				health.OnDeath += () =>
				{
					Healths.Remove(health);

					if (Healths.Count == 0)
					{
						OnAllDead?.Invoke();
					}
				};
			}
		}
	}
}
