using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
				Destroy(sources[j].playingAudioSource.gameObject);
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

	public static void Play(string soundName, Vector3 position, Transform parent)
	{

		AudioSource source = new GameObject(soundName).AddComponent<AudioSource>();
		source.gameObject.hideFlags = HideFlags.HideAndDontSave;
		source.transform.SetParent(parent);
		sources.Add(new PlayingAudioSourceData(soundName, source, false));
		source.transform.position = position;
		Sound sound = FindSoundFromName(soundName);


		source.clip = sound.clip;
		source.outputAudioMixerGroup = sound.mixer;
		source.loop = sound.loop;
		source.volume = sound.volume;
		source.pitch = sound.pitch;
		source.priority = sound.priority;
		source.spatialBlend = sound.spatialBlend ? 1 : 0;
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
		public Color previewColor;
		public int selectedMixer;
		public bool isVeryImportant;
		public bool soundVisibleInInspector;
		public bool soundVisibleInScene;
		public bool soundTesting;
		public bool paused;
		public bool isPlaying;
		public string soundName;
		public AudioClip clip;
		public AudioMixerGroup mixer;
		public bool loop;
		[Range(0, 256)] public int priority;
		[Range(0, 1)] public float volume;
		[Range(0, 3)] public float pitch;
		public bool spatialBlend;
		public Transform soundPreviewer;

		public DimensionalSoundSettings settings = new DimensionalSoundSettings();

		public Sound(AudioClip clip, AudioMixerGroup mixer, bool loop, int priority, float volume, float pitch, bool spatialBlend, DimensionalSoundSettings settings)
		{
			
			this.soundName = clip? clip.name : "New Sound";
			this.previewColor = new Color(.5f, 1, .5f);
			this.clip = clip;
			this.soundVisibleInScene = true;
			this.mixer = mixer;
			this.loop = loop;
			this.priority = priority;
			this.volume = volume;
			this.pitch = pitch;
			this.spatialBlend = spatialBlend;
			this.settings = settings;
			this.soundPreviewer = FindObjectOfType<AudioManager>().transform;
		}




	}
	[System.Serializable]
	public struct DimensionalSoundSettings
	{
		[Range(.001f, 500)] public float minDistance;
		[Range(.001f, 500)] public float maxDistance;

		public DimensionalSoundSettings(float minDistance = 1, float maxDistance = 500)
		{
			this.minDistance = minDistance;
			this.maxDistance = maxDistance;
		}
	}
	
	#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon ( transform.position, "Assets/Simple Audio Manager/Textures/sound icon.png" );
		DrawSoundSpheres();
	}
	
	void DrawSoundSpheres()
	{
		for (int i = 0; i < sounds.Count; i++)
		{
			if (Event.current.type == EventType.Repaint && sounds[i].soundVisibleInScene)
				if (sounds[i].spatialBlend && sounds[i].soundPreviewer)
				{
					if (sounds[i].isVeryImportant)
					{
						sounds[i].previewColor = Color.HSVToRGB((((float)EditorApplication.timeSinceStartup*100) % 100)/100, 1, 1);
					}
					Handles.color = new Color(.5f, .5f, 1);
					Handles.DrawWireDisc(sounds[i].soundPreviewer.position, new Vector3(0, 1, 0), sounds[i].settings.minDistance);
					Handles.color = new Color(.5f, .5f, 1, .05f);
					Handles.SphereHandleCap(0, sounds[i].soundPreviewer.position, Quaternion.identity, sounds[i].settings.minDistance * 2, EventType.Repaint);
					Handles.color = new Color(sounds[i].previewColor.r, sounds[i].previewColor.g, sounds[i].previewColor.b, 1);

					Handles.DrawWireDisc(sounds[i].soundPreviewer.position, new Vector3(0, 1, 0), sounds[i].settings.maxDistance);
					Handles.color = new Color(sounds[i].previewColor.r, sounds[i].previewColor.g, sounds[i].previewColor.b, .05f);
					Handles.SphereHandleCap(0, sounds[i].soundPreviewer.position, Quaternion.identity, sounds[i].settings.maxDistance * 2, EventType.Repaint);
					Handles.DrawBezier(transform.position, sounds[i].soundPreviewer.transform.position, transform.position + Vector3.down * Mathf.Abs(transform.position.y - sounds[i].soundPreviewer.transform.position.y), sounds[i].soundPreviewer.transform.position + Vector3.up * Mathf.Abs(transform.position.y - sounds[i].soundPreviewer.transform.position.y), Color.cyan, Texture2D.whiteTexture, 1);
					Handles.Label(sounds[i].soundPreviewer.position + Vector3.up * (sounds[i].settings.maxDistance + 10),sounds[i].soundName);
				}
		}
	}
	
	#endif
}
