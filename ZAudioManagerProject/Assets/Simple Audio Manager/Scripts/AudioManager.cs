using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;

[System.Serializable]
public class AudioManager : MonoBehaviour
{
	public static AudioManager i;
	public Transform soundPreviewer;
	void OnValidate()
	{
		soundPreviewer = transform;
	}
	public List<AudioMixerGroup> mixers = new List<AudioMixerGroup>();
	public List<Sound> sounds = new List<Sound>();
	public List<int> chosenMixers = new List<int>();
	public List<bool> soundOpen = new List<bool>();
	public List<bool> soundTesting = new List<bool>();
	public List<bool> paused = new List<bool>();
	public static List<PlayingAudioSourceData> sources = new List<PlayingAudioSourceData>();
	[System.Serializable]
	public class PlayingAudioSourceData
	{
		public string soundName;
		public AudioSource playingAudioSource;
		public bool isPaused;

		public PlayingAudioSourceData(string soundName, AudioSource playingAudioSource, bool isPaused)
		{
			this.soundName = soundName;
			this.playingAudioSource = playingAudioSource;
			this.isPaused = isPaused;
		}
	}
	void Awake()
	{
		i = i == null ? this : i;

	}

	void Update()
	{
		for (int j = 0; j < sources.Count; j++)
		{
			if (!sources[j].playingAudioSource.isPlaying && !sources[j].isPaused)
			{
				Destroy(sources[j].playingAudioSource);
				sources.RemoveAt(j);
			}
		}
	}

	static Sound FindSoundFromName(string name)
	{
		int index = 0;
		bool found = false;

		for (int j = 0; j < i.sounds.Count; j++)
		{
			if (i.sounds[j].soundName == name)
			{
				if (found)
				{
					Debug.LogError("There is more than one sound named " + name);
					return null;
				}

				index = j;
				found = true;
			}
		}

		if (!found)
		{
			Debug.LogWarning(name + " doesn't exist!");
			return null;
		}


		return i.sounds[index];
	}

	static PlayingAudioSourceData FindSoundSourceFromName(string name)
	{
		int index = 0;
		bool found = false;


		for (int j = 0; j < sources.Count; j++)
		{
			if (sources[j].soundName == name)
			{
				found = true;
				index = j;
			}
		}

		if (!found)
		{
			Debug.LogWarning(name + " doesn't exist!");
		}


		return sources[index];
	}

	public static void Play(string soundName)
	{
		AudioSource source = i.gameObject.AddComponent<AudioSource>();
		sources.Add(new PlayingAudioSourceData(soundName, source, false));
		Sound sound = FindSoundFromName(soundName);

		source.clip = sound.clip;
		source.outputAudioMixerGroup = sound.mixer;
		source.loop = sound.loop;
		source.volume = sound.volume;
		source.pitch = sound.pitch;
		source.priority = sound.priority;
		source.spatialBlend = 0;

		source.Play();

	}

	public static void Play(string soundName, Vector3 position)
	{

		AudioSource source = new GameObject(soundName).AddComponent<AudioSource>();
		sources.Add(new PlayingAudioSourceData(soundName, source, false));
		source.transform.position = position;
		Sound sound = FindSoundFromName(soundName);


		source.clip = sound.clip;
		source.outputAudioMixerGroup = sound.mixer;
		source.loop = sound.loop;
		source.volume = sound.volume;
		source.pitch = sound.pitch;
		source.priority = sound.priority;
		source.spatialBlend = 1;
		source.minDistance = sound.settings.minDistance;
		source.maxDistance = sound.settings.maxDistance;

		source.Play();
	}

	public static void TogglePause(string soundName)
	{
		PlayingAudioSourceData data = FindSoundSourceFromName(soundName);

		if (!data.playingAudioSource.isPlaying) data.playingAudioSource.UnPause();
		else if (data.playingAudioSource.isPlaying) data.playingAudioSource.Pause();

		data.isPaused = !data.isPaused;

	}

	//Clase contenedora de sonidos y otras movidas
	[System.Serializable]
	public class Sound
	{
		public bool isPlaying;
		public string soundName;
		public AudioClip clip;
		public AudioMixerGroup mixer;
		public bool loop;
		[Range(0, 256)] public int priority;
		[Range(0, 1)] public float volume;
		[Range(0, 3)] public float pitch;
		[Range(0, 1)] public bool spatialBlend;
		public Transform soundPreviewer;

		public DimensionalSoundSettings settings = new DimensionalSoundSettings();

		public Sound(AudioClip clip, AudioMixerGroup mixer, bool loop, int priority, float volume, float pitch, bool spatialBlend, DimensionalSoundSettings settings)
		{
			if (clip)
				this.soundName = clip.name;
			this.clip = clip;
			this.mixer = mixer;
			this.loop = loop;
			this.priority = priority;
			this.volume = volume;
			this.pitch = pitch;
			this.spatialBlend = spatialBlend;
			this.settings = settings;
		}




	}
	[System.Serializable]
	public struct DimensionalSoundSettings
	{
		public float minDistance;
		public float maxDistance;

		public DimensionalSoundSettings(float minDistance = 1, float maxDistance = 500)
		{
			this.minDistance = minDistance;
			this.maxDistance = maxDistance;
		}
	}



}
