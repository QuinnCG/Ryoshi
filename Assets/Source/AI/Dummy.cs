using Quinn.DamageSystem;
using System.Collections;
using UnityEngine;

namespace Quinn.AI
{
	public class Dummy : MonoBehaviour
	{
		private IEnumerator Start()
		{
			while (true)
			{
				yield return new WaitForSeconds(2f);

				var colliders = Physics2D.OverlapBoxAll(transform.position, Vector2.one * 5f, 0f);

				foreach (var collider in colliders)
				{
					if (collider.TryGetComponent(out IDamageable dmg))
					{
						dmg.TakeDamage(new()
						{
							Damage = 1,
							Direction = transform.position.DirectionTo(collider.bounds.center),
							TeamType = TeamType.Enemy
						});
					}
				}
			}
		}
	}
}
