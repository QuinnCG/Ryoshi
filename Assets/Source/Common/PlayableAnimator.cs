using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Quinn
{
	/// <summary>
	/// An animator that plays <see cref="AnimationClip"/>s directly.
	/// </summary>
	public class PlayableAnimator : MonoBehaviour
	{
		private PlayableGraph _graph;
		private AnimationPlayableOutput _output;

		private AnimationClip _loopingAnim;
		private float _nextOneShotEndTime;
		private bool _isOneShotPlaying;

		private void Awake()
		{
			if (TryGetComponent(out Animator a))
			{
				DestroyImmediate(a);
			}

			var animator = gameObject.AddComponent<Animator>();

			_graph = PlayableGraph.Create("Playable Animator Graph");
			_output = AnimationPlayableOutput.Create(_graph, "Animation Output", animator);
		}

		private void Update()
		{
			if (_isOneShotPlaying && Time.time >= _nextOneShotEndTime)
			{
				_isOneShotPlaying = false;
				
				if (_loopingAnim != null)
				{
					PlayAnimClip(_loopingAnim);
				}
				else
				{
					Stop();
				}
			}
		}

		private void OnDestroy()
		{
			_graph.Destroy();
		}

		public void PlayLooped(AnimationClip anim)
		{
			if (_loopingAnim != anim)
			{
				_loopingAnim = anim;
				PlayAnimClip(anim);
			}
		}

		public void PlayOnce(AnimationClip anim)
		{
			_nextOneShotEndTime = Time.time + anim.length;
			PlayAnimClip(anim);

			_isOneShotPlaying = true;
		}

		public void Stop()
		{
			_graph.Stop();
			_loopingAnim = null;
			_isOneShotPlaying = false;
		}

		public void StopOneShot()
		{
			_nextOneShotEndTime = -1f;
		}

		private void PlayAnimClip(AnimationClip anim)
		{
			// Remove current, if it exists.
			var current = _output.GetSourcePlayable();

			if (current.IsValid())
			{
				current.Destroy();
			}

			// Create new.
			var playableClip = AnimationClipPlayable.Create(_graph, anim);
			_output.SetSourcePlayable(playableClip);

			// Play graph, in case it isn't already playing.
			_graph.Play();
		}
	}
}
