using FMODUnity;
using Quinn.DamageSystem;
using UnityEngine;

namespace Quinn
{
	public class Foliage : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private EventReference SFX;
		[SerializeField]
		private ParticleSystem VFX;

		[SerializeField]
		private Sprite[] Sprites;

		private bool _isDead;

		private void Awake()
		{
			GetComponent<SpriteRenderer>().sprite = Sprites.GetRandom();
		}

		bool IDamageable.IsLowPriority()
		{
			return true;
		}

		public bool CanDamage(DamageInfo info)
		{
			return !_isDead;
		}

		public bool TakeDamage(DamageInfo info)
		{
			_isDead = true;

			Audio.Play(SFX, transform.position);
			VFX.transform.SetParent(null, true);
			VFX.Play();

			gameObject.Destroy();
			return true;
		}
	}
}
