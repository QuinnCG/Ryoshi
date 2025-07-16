using Quinn.DamageSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quinn.AI
{
	[RequireComponent(typeof(PlayableAnimator))]
	[RequireComponent(typeof(EnemyMovement))]
	[RequireComponent(typeof(Health))]
	public class AgentAI : MonoBehaviour
	{
		public delegate IEnumerator State();

		protected PlayableAnimator Animator { get; private set; }
		protected EnemyMovement Movement { get; private set; }
		protected Health Health { get; private set; }

		protected Transform Player => Quinn.Player.Instance.transform;
		protected float DistanceToPlayer => transform.position.DistanceTo(Player.position);
		protected Vector2 DirectionToPlayer => transform.position.DirectionTo(Player.position);

		private readonly HashSet<string> _events = new();
		private IEnumerator _activeState;

		protected virtual void Awake()
		{
			Animator = GetComponent<PlayableAnimator>();
			Movement = GetComponent<EnemyMovement>();
			Health = GetComponent<Health>();
		}

		private void LateUpdate()
		{
			ResetAllEvents();
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
		public bool PeakEvent(string name)
		{
			return _events.Contains(name);
		}

		protected void TransitionTo(State state)
		{
			Debug.Assert(state != null);

			ClearState();

			_activeState = state();
			StartCoroutine(_activeState);
		}

		protected void ClearState()
		{
			if (_activeState != null)
			{
				StopCoroutine(_activeState);
				_activeState = null;
			}
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
	}
}
