using System.Collections.Generic;
using UnityEngine;

namespace Quinn.MovementSystem
{
	[RequireComponent(typeof(Rigidbody2D))]
	public abstract class Locomotion : MonoBehaviour
	{
		[SerializeField]
		private float GenericDecayRateFactor = 1f;

		// Used for things like knockback.
		class DecayingVelocity
		{
			public Vector2 Velocity;
			public float DecayRate;
		}

		public bool HasAnyVelocity => Velocity.sqrMagnitude > 0f;
		public float DecayRateFactor { get; set; } = 1f;

		public Vector2 Velocity
		{
			get
			{
				if (_overrideVelocity.HasValue)
				{
					return _overrideVelocity.Value;
				}
				else
				{
					return _cumulativeVelocity;
				}
			}
		}

		protected Rigidbody2D Rigidbody { get; private set; }

		private readonly HashSet<DecayingVelocity> _decayingVelocities = new();
		private Vector2 _cumulativeVelocity;
		private Vector2? _overrideVelocity;

		protected virtual void Awake()
		{
			Rigidbody = GetComponent<Rigidbody2D>();
		}

		protected virtual void LateUpdate()
		{
			_cumulativeVelocity += ProcessDecayingVelocities();

			Vector2 finalVel;

			if (_overrideVelocity.HasValue)
			{
				finalVel = _overrideVelocity.Value;
			}
			else
			{
				finalVel = _cumulativeVelocity;
			}

			Rigidbody.linearVelocity = finalVel;

			_overrideVelocity = null;
			ResetVelocity();
		}

		protected virtual void OnDisable()
		{
			ResetVelocity();
			Rigidbody.linearVelocity = Vector2.zero;
		}

		/// <summary>
		/// Additive <c>direction * speed</c> that is reset by the end of the frame.
		/// </summary>
		protected void AddVelocity(Vector2 velocity)
		{
			_cumulativeVelocity += velocity;
		}

		/// <summary>
		/// Force the velocity to be a certain value for this frame. This will ignore any calls from <see cref="AddVelocity(Vector2)"/> for this frame.
		/// </summary>
		protected void SetVelocity(Vector2 velocity)
		{
			_overrideVelocity = velocity;
		}

		/// <summary>
		/// This can be used for things like knockback.
		/// </summary>
		/// <param name="velocity">The initial speed and direction.</param>
		/// <param name="decayRate">How much speed should be subtracted every second.</param>
		public void AddDecayingVelocity(Vector2 velocity, float decayRate)
		{
			_decayingVelocities.Add(new DecayingVelocity()
			{
				Velocity = velocity,
				DecayRate = decayRate
			});
		}

		/// <summary>
		/// Resets cumulative velocity to zero.
		/// </summary>
		protected void ResetVelocity(bool removeDecayingVelocities = false)
		{
			_cumulativeVelocity = Vector2.zero;

			if (removeDecayingVelocities)
			{
				_decayingVelocities.Clear();
			}
		}

		// Returns the sum of all decaying velocities, then decays them.
		private Vector2 ProcessDecayingVelocities()
		{
			var sum = Vector2.zero;
			var toRemove = new List<DecayingVelocity>();

			foreach (var handle in _decayingVelocities)
			{
				sum += handle.Velocity;

				float mag = handle.Velocity.magnitude;
				Vector2 dir = handle.Velocity.normalized;

				mag -= Time.deltaTime * handle.DecayRate * DecayRateFactor * GenericDecayRateFactor;
				mag.MakeAtLeast(0f);

				handle.Velocity = dir * mag;

				if (handle.Velocity.sqrMagnitude == 0f)
				{
					toRemove.Add(handle);
				}
			}

			_decayingVelocities.RemoveRange(toRemove);
			return sum;
		}
	}
}
