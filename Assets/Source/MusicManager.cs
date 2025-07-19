using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Quinn
{
	public class MusicManager : MonoBehaviour
	{
		public static MusicManager Instance { get; private set; }

		private EventInstance _areaMusic;
		private EventInstance _bossMusic;
		private EventReference _areaMusicRef;

		private void Awake()
		{
			Instance = this;
		}

		public void PlayAreaMusic(EventReference music)
		{
			if (music.IsNull)
				return;

			_areaMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_areaMusic.release();

			_areaMusic = RuntimeManager.CreateInstance(music);
			_areaMusic.start();

			_areaMusicRef = music;
		}

		public void PlayBossMusic(EventReference loop)
		{
			_areaMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_areaMusic.release();

			_bossMusic = RuntimeManager.CreateInstance(loop);
			_bossMusic.start();
		}

		public async void StopBossMusic(EventReference outro)
		{
			_bossMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_bossMusic.release();

			_bossMusic = RuntimeManager.CreateInstance(outro);
			_bossMusic.start();

			await Awaitable.WaitForSecondsAsync(5f);
			PlayAreaMusic(_areaMusicRef);
		}
	}
}
