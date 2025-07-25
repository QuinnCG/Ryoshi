using Quinn.DamageSystem;
using Quinn.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Quinn.AI
{
	[RequireComponent(typeof(PlayableAnimator))]
	[RequireComponent(typeof(EnemyMovement))]
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(BoxCollider2D))]
	public class AgentAI : MonoBehaviour
	{
		[SerializeField, Required]
		private TextMeshProUGUI DebugStateText;
		[SerializeField, Required]
		private DialogueSpeaker Speaker;

		// HACK
		protected DialogueSpeaker SpeakerRef => Speaker;

		public delegate IEnumerator State();

		protected PlayableAnimator Animator { get; private set; }
		protected EnemyMovement Movement { get; private set; }
		protected Health Health { get; private set; }
		protected BoxCollider2D Hitbox { get; private set; }

		protected IEnumerator ActiveState { get; private set; }

		protected Transform Player => Quinn.Player.Instance.transform;
		protected float DistanceToPlayer => transform.position.DistanceTo(Player.position);
		protected Vector2 DirectionToPlayer => transform.position.DirectionTo(Player.position);

		private readonly HashSet<string> _events = new();

		private bool _hasHealthDroppedBelowHalfYet;

		protected virtual void Awake()
		{
			Animator = GetComponent<PlayableAnimator>();
			Movement = GetComponent<EnemyMovement>();
			Health = GetComponent<Health>();
			Hitbox = GetComponent<BoxCollider2D>();

			Health.OnDamage += Damage;
			Health.OnDeath += Death;

			AgentManager.Instance.RegisterAgent(this);
		}

		private void LateUpdate()
		{
			var room = LockedRoom.Instance;

			if (room != null && !room.IsConquered && room.HasBegun && room.Boss.gameObject != gameObject && DistanceToPlayer < 10f)
			{
				Health.Kill();
			}
		}

		public void SetAgentStateDisplay(bool visible)
		{
			if (visible)
			{
				if (ActiveState != null)
				{
					DebugStateText.text = "Unnamed State";
				}
				else
				{
					DebugStateText.text = "No State Set";
				}

				DebugStateText.enabled = true;
			}
			else
			{
				DebugStateText.enabled = false;
			}
		}

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
		public bool PeekEvent(string name)
		{
			return _events.Contains(name);
		}

		protected void TransitionTo(State state, string name = default)
		{
			Debug.Assert(state != null);

			if (Health.IsDead)
				return;

			ClearState();

			ActiveState = state();
			StartCoroutine(ActiveState);

			DebugStateText.text = name;
		}
		protected void TransitionTo(IEnumerator state, string name = default)
		{
			Debug.Assert(state != null);

			if (Health.IsDead)
				return;

			ClearState();

			ActiveState = state;
			StartCoroutine(ActiveState);

			DebugStateText.text = name;
		}

		protected void ClearState()
		{
			if (ActiveState != null)
			{
				StopCoroutine(ActiveState);
				ActiveState = null;

				DebugStateText.text = "No Active State!";
			}
		}

		protected void FaceDirection(float xDir)
		{
			Movement.SetFacingDir(xDir);
		}
		protected void FacePlayer()
		{
			Movement.SetFacingDir(DirectionToPlayer.x);
		}
		protected void FaceAwayFromPlayer()
		{
			Movement.SetFacingDir(-DirectionToPlayer.x);
		}

		/// <inheritdoc cref="DialogueSpeaker.Speak(string[])"/>
		protected float Speak(params string[] messages)
		{
			return Speaker.Speak(messages);
		}

		protected void StopSpeaking()
		{
			Speaker.StopSpeaking();
		}

		protected IEnumerator PlayAnimOnce(AnimationClip anim, bool holdLastFrame = false)
		{
			Animator.PlayOnce(anim, holdLastFrame);
			yield return new WaitForSeconds(anim.length);
		}

		protected IEnumerator WaitUntil(System.Func<bool> predicate, float timeout = -1f)
		{
			if (timeout > 0f)
			{
				yield return new WaitUntil(predicate, System.TimeSpan.FromSeconds((float)timeout), () => { });
			}
			else
			{
				yield return new WaitUntil(predicate);
			}
		}

		/// <summary>
		/// Applies a relative damage box.
		/// </summary>
		/// <param name="offset">The offset from the center of this enemy's hitbox. This is flipped based on <see cref="Movement.FacingDirection"/></param>
		/// <param name="size">The size of the damage box.</param>
		/// <param name="damage">The amount of damage to apply.</param>
		/// <param name="direction">The direction of the attack; used by the player for blocking and parrying purposes.</param>
		/// <param name="knockback">The knockback velocity; this is also flipped basd on <see cref="Movement.FacingDirection"/>.</param>
		/// <returns>True, if anything was sucessfully damaged.</returns>
		protected bool DamageBox(Vector2 offset, Vector2 size, int damage, Vector2 direction, Vector2 knockback)
		{
			offset.x *= Movement.FacingDirection;
			Vector2 center = (Vector2)Hitbox.bounds.center + offset;

			var colliders = Physics2D.OverlapBoxAll(center, size, 0f);

			knockback.x *= Movement.FacingDirection;
			bool hitAny = false;

			foreach (var collider in colliders)
			{
				if (collider.gameObject != gameObject && collider.TryGetComponent(out IDamageable dmg))
				{
					bool hit = dmg.TakeDamage(new()
					{
						Damage = damage,
						Direction = direction.normalized,
						Knockback = knockback,
						TeamType = TeamType.Enemy
					}, out bool _);

					if (hit)
						hitAny = true;
				}
			}

			return hitAny;
		}

		private void Damage(DamageInfo info)
		{
			if (Health.Normalized < 0.5f && !_hasHealthDroppedBelowHalfYet)
			{
				_hasHealthDroppedBelowHalfYet = true;
				OnSecondPhaseBegin();
			}

			OnDamage(info);
		}

		protected virtual void OnSecondPhaseBegin() { }

		protected virtual void OnDamage(DamageInfo info) { }

		private void Death()
		{
			AgentManager.Instance.UnregisterAgent(this);
			OnDeath();
		}

		protected virtual void OnDeath()
		{
			ClearState();
			gameObject.Destroy();
		}
	}
}
