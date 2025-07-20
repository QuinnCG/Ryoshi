using FMOD.Studio;
using FMODUnity;
using System;
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

			if (!loop.IsNull)
			{
				_bossMusic = RuntimeManager.CreateInstance(loop);
				_bossMusic.start();
			}
		}

		public async void StopBossMusic(EventReference outro)
		{
			if (_bossMusic.isValid())
			{
				_bossMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				_bossMusic.release();
			}

			if (!outro.IsNull)
			{
				_bossMusic = RuntimeManager.CreateInstance(outro);
				_bossMusic.start();
			}

			await Awaitable.WaitForSecondsAsync(5f);
			PlayAreaMusic(_areaMusicRef);
		}

		public void StopAllMusic()
		{
			_areaMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_areaMusic.release();

			_bossMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_bossMusic.release();
		}
	}
}
