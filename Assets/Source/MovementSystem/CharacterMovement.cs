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
		// The FMOD global parameter; what surface the player is currently standing on.
		public const string PlayerSurfaceMatKey = "player-surface-material";

		// HACK: Shouldn't set this surface mat key for anyone but the player's movement script.

		[SerializeField, Unit(Units.MetersPerSecondSquared)]
		private float FallSpeed = 12f;
		[SerializeField, Unit(Units.MetersPerSecond)]
		private float InitialFallSpeed = 12f;
		[SerializeField, Unit(Units.MetersPerSecondSquared)]
		private float MaxFallSpeed = 64f;

		[Space, SerializeField]
		private bool StartGrounded = true;
		[SerializeField]
		private float AnimatedWalkSpeed = 2f;
		[SerializeField]
		private bool StartFacingRight = true;

		[Space, SerializeField]
		private bool TrackPlayerSoundMaterial;

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

		public SoundMaterialType LastFloorMaterial { get; protected set; }

		public bool IsTouchingGround { get; private set; }
		public bool IsTouchingCeiling { get; private set; }
		public bool IsTouchingWall { get; private set; }
		/// <summary>
		/// Is moving towards a wall, that we are also in contact with.
		/// </summary>
		public bool IsMovingIntoWall { get; private set; }

		public float FacingDirection { get; private set; } = 1f;

		private readonly HashSet<SpeedFactor> _moveSpeedFactors = new();
		private readonly HashSet<object> _blockGravity = new();

		private float _fallSpeed;

		private bool _isAnimatingWalk;
		private float _animWalkDir;
		private float _animWalkSpeed;

		protected override void Awake()
		{
			base.Awake();
			IsTouchingGround = StartGrounded;

			SetFacingDir(StartFacingRight ? 1f : -1f);
		}

		private void OnValidate()
		{
			GetComponent<SpriteRenderer>().flipX = !StartFacingRight;
		}

		protected virtual void Update()
		{
			if (_blockGravity.Count > 0)
			{
				_fallSpeed = 0f;
			}
			else
			{
				if (IsTouchingGround)
				{
					_fallSpeed = 0f;
				}
				else
				{
					_fallSpeed += FallSpeed * Time.deltaTime;
					_fallSpeed.MakeLessThan(MaxFallSpeed);

					AddVelocity(_fallSpeed * Vector2.down);
				}
			}

			ProcessContacts();

			if (_isAnimatingWalk)
			{
				AddVelocity(_animWalkDir * _animWalkSpeed * Vector2.right);
			}

			// HACK: Magic number.
			DecayRateFactor = IsTouchingGround ? 5f : 1f;
		}

		protected override void OnDisable()
		{
			ResetGravity();
			base.OnDisable();
		}

		public void ResetGravity()
		{
			_fallSpeed = 0f;
		}

		public void BlockGravity(object key)
		{
			_blockGravity.Add(key);
		}
		public void UnblockGravity(object key)
		{
			_blockGravity.Remove(key);
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

		public void SetFacingDir(float xDir)
		{
			if (xDir != 0f)
			{
				FacingDirection = Mathf.Sign(xDir);
			}
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

			bool processedFloorAnyContact = false;

			foreach (var contact in contacts)
			{
				int layer = contact.collider.gameObject.layer;

				if (layer != LayerMask.NameToLayer(Layers.ObstacleName))
					continue;

				Vector2 normal = contact.normal;

				if (!OnContact(contact.collider, normal, layer))
				{
					break;
				}

				// Vertical
				if (normal.IsVertical())
				{
					if (normal.y > 0f)
					{
						IsTouchingGround = true;

						if (!processedFloorAnyContact && TrackPlayerSoundMaterial && contact.collider.TryGetComponent(out SoundMaterial mat))
						{
							LastFloorMaterial = mat.Material;
							Audio.SetGlobalParameter(PlayerSurfaceMatKey, LastFloorMaterial.ToString());
						}

						processedFloorAnyContact = true;
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
					StartFalling();
					OnLeaveGround();
				}
			}
		}

		/// <summary>
		/// Called for each collision contact.
		/// </summary>
		/// <param name="normal">The normal of the contact.</param>
		/// <param name="layer">The physics layer of the contact.</param>
		/// <returns>True, if we should continue looking for contacts this frame.</returns>
		protected virtual bool OnContact(Collider2D collider, Vector2 normal, int layer)
		{
			return true;
		}

		protected void StartFalling()
		{
			_fallSpeed = InitialFallSpeed;
		}

		/* ANIMATION EVENTS */

		// 'dir' is relative to facing dir.
		protected void Walk(int dir)
		{
			_isAnimatingWalk = true;
			_animWalkDir = Mathf.Sign(dir) * FacingDirection;
			_animWalkSpeed = AnimatedWalkSpeed;
		}

		protected void StopWalk()
		{
			_isAnimatingWalk = false;
		}

		protected void SFX(string eventName)
		{
			Audio.Play(eventName, transform.position);
		}
	}
}
