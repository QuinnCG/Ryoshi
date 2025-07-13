using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn.MovementSystem
{
	/// <summary>
	/// Supports things like slowed movement from a status effect.
	/// </summary>
	public class CharacterMovement : Locomotion
	{
		[SerializeField, Unit(Units.MetersPerSecondSquared)]
		private float FallSpeed = 12f;
		[SerializeField, Unit(Units.MetersPerSecondSquared)]
		private float MaxFallSpeed = 64f;

		public float MoveSpeed { get; protected set; }
		/// <summary>
		/// The move speed, accounting for any factors from status effects or the like.
		/// </summary>
		public float FinalMoveSpeed => SpeedFactor * MoveSpeed;
		public float SpeedFactor
		{
			get
			{
				float speed = 1f;

				foreach (var factor in _moveSpeedFactors)
				{
					speed *= factor.Factor;
				}

				return speed;
			}
		}

		public bool IsTouchingGround { get; private set; }
		public bool IsTouchingCeiling { get; private set; }
		public bool IsTouchingWall { get; private set; }
		/// <summary>
		/// Is moving towards a wall, that we are also in contact with.
		/// </summary>
		public bool IsMovingIntoWall { get; private set; }

		private readonly HashSet<SpeedFactor> _moveSpeedFactors = new();

		private float _fallSpeed;

		protected virtual void Update()
		{
			ProcessContacts();

			_fallSpeed += FallSpeed * Time.deltaTime;
			_fallSpeed.MakeLessThan(MaxFallSpeed);

			if (IsTouchingGround)
			{
				_fallSpeed = 0f;
			}
			else
			{
				AddVelocity(_fallSpeed * Vector2.down);
			}
		}

		public SpeedFactor CreateMoveSpeedFactor()
		{
			var factor = new SpeedFactor();
			_moveSpeedFactors.Add(factor);

			return factor;
		}

		public void DestroyMoveSpeedFactor(SpeedFactor factor)
		{
			_moveSpeedFactors.Remove(factor);
		}

		/// <summary>
		/// Applies a velocity that decays over time by <paramref name="decayRate"/>.
		/// </summary>
		/// <param name="velocity">The initial speed and direction.</param>
		/// <param name="decayRate">How much speed should be subtracted every second.</param>
		/// <param name="overrideVelocity">If true, the velocity will be reset before applying this value.</param>
		public void Push(Vector2 velocity, float decayRate, bool overrideVelocity = false)
		{
			if (overrideVelocity)
			{
				CeaseMomentum();
			}

			AddDecayingVelocity(velocity, decayRate);
		}

		public void CeaseMomentum()
		{
			ResetVelocity(true);
		}

		protected virtual void OnLeaveGround() { }
		protected virtual void OnTouchGround() { }

		private void ProcessContacts()
		{
			var contacts = new List<ContactPoint2D>();
			Rigidbody.GetContacts(contacts);

			bool wasTouchingGroundLastFrame = IsTouchingGround;

			IsTouchingGround = false;
			IsTouchingCeiling = false;
			IsTouchingWall = false;

			foreach (var contact in contacts)
			{
				Vector2 normal = contact.normal;

				// Vertical
				if (normal.IsVertical())
				{
					if (normal.y > 0f)
					{
						IsTouchingGround = true;
					}
					else if (normal.y < 0f)
					{
						IsTouchingCeiling = true;
					}
				}
				// Horizontal
				else
				{
					IsTouchingWall = true;
				}
			}

			// Grounded state has changed from last frame.
			if (wasTouchingGroundLastFrame != IsTouchingGround)
			{
				// Touching now, but wasn't last frame.
				if (IsTouchingGround)
				{
					OnTouchGround();
				}
				// Not touching now, but was last frame.
				else
				{
					OnLeaveGround();
				}
			}
		}
	}
}
