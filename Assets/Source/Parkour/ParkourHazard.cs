using Quinn.DamageSystem;
using UnityEngine;

namespace Quinn.Parkour
{
	[RequireComponent(typeof(Collider2D))]
	public class ParkourHazard : MonoBehaviour
	{
		[SerializeField]
		private float Damage = 5f;
		[SerializeField]
		private Vector2 Knockback;
		[SerializeField]
		private float Cooldown = 2f;

		private float _nextAllowedDamageTime;

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.collider.IsPlayer() && Time.time > _nextAllowedDamageTime)
			{
				_nextAllowedDamageTime = Time.time + Cooldown;

				Vector2 dirToPlayer = transform.position.DirectionTo(Player.Instance.transform.position);

				Vector2 knockback = Knockback;
				knockback.x *= Mathf.Sign(dirToPlayer.x);

				bool success = Player.Instance.Health.TakeDamage(new DamageSystem.DamageInfo()
				{
					Damage = Damage,
					Direction = dirToPlayer,
					Knockback = knockback,
					TeamType = DamageSystem.TeamType.Environment
				}, out bool isLethal);

				if (success && !isLethal)
				{
					Player.Instance.GoToParkourCheckpoint();
				}
			}
			else if (collision.collider.IsEnemy())
			{
				collision.collider.GetComponent<Health>().Kill();
			}
		}
	}
}
