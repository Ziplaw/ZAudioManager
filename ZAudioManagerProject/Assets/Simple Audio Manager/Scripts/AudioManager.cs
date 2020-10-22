﻿using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;
using Random = System.Random;

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
        public object sender;

        public PlayingAudioSourceData(string soundName, AudioSource playingAudioSource, bool isPaused, object sender)
        {
            this.soundName = soundName;
            this.playingAudioSource = playingAudioSource;
            this.isPaused = isPaused;
            this.sender = sender;
        }
    }

    void Awake()
    {
        i = i == null ? this : i;

        for (int j = 0; j < sounds.Count; j++)
        {
            sounds[j].SubscribeMethodToEvents();
        }
    }

    /// <summary>
    /// Get an array of Audio Sources from a playing Sound
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="soundName"></param>
    /// <returns></returns>
    public static AudioSource[] GetAudioSource(object sender, string soundName)
    {
        return (from s in sources where s.sender == sender select s.playingAudioSource).ToArray();
    }


    void Update()
    {
        for (int j = 0; j < sources.Count; j++)
        {
            if (!sources[j].playingAudioSource.isPlaying && !sources[j].isPaused)
            {
                Destroy(sources[j].playingAudioSource.gameObject.GetComponent<AudioSource>());
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

    public static AudioSource Play(object sender, string soundName)
    {
        AudioSource source = i.gameObject.AddComponent<AudioSource>();
        source.hideFlags = HideFlags.HideInInspector;

        sources.Add(new PlayingAudioSourceData(soundName, source, false, sender));
        Sound sound = FindSoundFromName(soundName);


        source.clip = sound.clip;
        source.outputAudioMixerGroup = sound.mixer;
        source.loop = sound.loop;
        source.volume = sound.volumeIsRange
            ? UnityEngine.Random.Range(sound.volumeRange.x, sound.volumeRange.y)
            : sound.volume;
        ;
        source.pitch = sound.pitchIsRange
            ? UnityEngine.Random.Range(sound.pitchRange.x, sound.pitchRange.y)
            : sound.pitch;
        source.priority = sound.priorityIsRange
            ? (int) UnityEngine.Random.Range(sound.priorityRange.x, sound.priorityRange.y)
            : sound.priority;
        source.spatialBlend = 0;

        sound.e?.Invoke();
        source.Play();
        return source;
    }

    public static AudioSource Play(object sender, string soundName, Vector3 position, Transform parent)
    {
        AudioSource source = new GameObject(soundName).AddComponent<AudioSource>();
        source.gameObject.hideFlags = HideFlags.HideAndDontSave;
        source.transform.SetParent(parent);
        sources.Add(new PlayingAudioSourceData(soundName, source, false, sender));
        source.transform.position = position;
        Sound sound = FindSoundFromName(soundName);


        source.clip = sound.clip;
        source.outputAudioMixerGroup = sound.mixer;
        source.loop = sound.loop;
        source.volume = sound.volumeIsRange
            ? UnityEngine.Random.Range(sound.volumeRange.x, sound.volumeRange.y)
            : sound.volume;
        ;
        source.pitch = sound.pitchIsRange
            ? UnityEngine.Random.Range(sound.pitchRange.x, sound.pitchRange.y)
            : sound.pitch;
        source.priority = sound.priorityIsRange
            ? (int) UnityEngine.Random.Range(sound.priorityRange.x, sound.priorityRange.y)
            : sound.priority;
        source.spatialBlend = sound.spatialBlend ? 1 : 0;
        source.minDistance = sound.settings.minDistance;
        source.maxDistance = sound.settings.maxDistance;

        sound.e?.Invoke();
        source.Play();
        return source;
    }

    public static void TogglePause(string soundName)
    {
        PlayingAudioSourceData data = FindSoundSourceFromName(soundName);

        if (!data.playingAudioSource.isPlaying) data.playingAudioSource.UnPause();
        else if (data.playingAudioSource.isPlaying) data.playingAudioSource.Pause();

        data.isPaused = !data.isPaused;
    }

    [System.Serializable]
    public class EventHolder
    {
        public GameObject eventer;
        public bool spatialBlend;

        public EventHolder(bool spatialBlend)
        {
            this.spatialBlend = spatialBlend;
        }

        public List<string> eventInfoNames
        {
            get
            {
                List<string> temp = new List<string>();

                for (int j = 0; j < _mask.Length; j++)
                {
                    if (_mask[j]) temp.Add(usableEvents[j].Name);
                }

                return temp;
            }
        }

        public int selectedComponent;
        public int selectedEvents;

        private string _selectedEventsBin => System.Convert.ToString(selectedEvents, 2);

        public bool[] _mask
        {
            set => _mask = _mask;
            get
            {
                bool[] temp;
                if (selectedEvents != -1)
                {
                    temp = new bool[_selectedEventsBin.ToCharArray().Length];
                }
                else
                {
                    temp = new bool[usableEvents.Length];
                }

                for (int j = 0; j < temp.Length; j++)
                {
                    if (selectedEvents != -1)
                    {
                        temp[j] = _selectedEventsBin.ToCharArray()[temp.Length - j - 1] == '1';
                    }
                    else
                    {
                        temp[j] = true;
                    }
                }

                return temp;
            }
        }


        public Component[] components => eventer.GetComponents<Component>();

        public EventInfo[] usableEvents
        {
            get
            {
                EventInfo[] eventInfoes = components[selectedComponent].GetType().GetEvents();

                // Debug.Log(eventInfoes.Length);
                List<EventInfo> temp = new List<EventInfo>();
                for (int j = 0; j < eventInfoes.Length; j++)
                {
                    // if(eventInfoes[j].EventHandlerType.GetMethod("Invoke").GetParameters() == typeof(AudioManager.Sound).GetMethod("PlaySoundSubscribe").GetParameters()) 
                    // 	temp.Add(eventInfoes[j]);
                    MethodInfo subscriberMethod = null;

                    switch (spatialBlend)
                    {
                        case true:
                            subscriberMethod = typeof(AudioManager.Sound).GetMethod("PlaySoundSubscribe",
                                new Type[] {typeof(object), typeof(Transform), typeof(Vector3)});
                            break;
                        case false:
                            subscriberMethod = typeof(AudioManager.Sound).GetMethod("PlaySoundSubscribe",
                                new Type[] {typeof(object)});
                            break;
                    }
                    
                    // Debug.Log(subscriberMethod + " " + eventInfoes[j].EventHandlerType.GetMethod("Invoke") + " " + spatialBlend);

                    var parameterTypeNames =
                        from p in subscriberMethod.GetParameters()
                        select p.ParameterType.FullName;

                    var eventTypeNames =
                        from p in eventInfoes[j].EventHandlerType.GetMethod("Invoke").GetParameters()
                        select p.ParameterType.FullName;

                    var parameterTypeArray = parameterTypeNames.ToArray();
                    var eventTypeArray = eventTypeNames.ToArray();

                    if (EqualsList(parameterTypeArray.ToList(), eventTypeArray.ToList()))
                    {
                        temp.Add(eventInfoes[j]);
                    }
                }

                return temp.ToArray();
            }
        }

        public static bool EqualsList<T>(List<T> a, List<T> b)
        {
            if (a == null)
            {
                // Debug.Log("a == null");
                return b == null;
            }

            if (b == null || a.Count != b.Count)
            {
                // Debug.Log("not the same number of items");
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!object.Equals(a[i], b[i]))
                {
                    // Debug.Log("objects in position " + i +  " are different");

                    return false;
                }
            }

            return true;
        }
    }

    //Clase contenedora de sonidos y otras movidas
    [System.Serializable]
    public class Sound
    {
        public Color previewColor;
        public int selectedMixer;
        public bool isVeryImportant;
        public bool volumeIsRange;
        public bool priorityIsRange;
        public bool pitchIsRange;
        public bool soundVisibleInInspector;
        public bool mixersVisibleInInspector;
        public bool soundVisibleInScene;
        public bool paused;
        public string soundName;
        public AudioClip clip;
        public AudioMixerGroup mixer;
        public bool loop;
        [Range(0, 256)] public int priority;
        [Range(0, 1)] public float volume;

        [Range(0.1f, 3)] public float pitch;

        public Vector2 volumeRange;
        public Vector2 priorityRange;
        public Vector2 pitchRange;
        public bool unityEventsVisible;
        public UnityEvent e;
        public bool spatialBlend;
        public Transform soundPreviewer;
        public int selectedTab;
        public DimensionalSoundSettings settings = new DimensionalSoundSettings();
        public int eventTypeSelected;
        public List<EventHolder> eventHolderList;
        public Texture2D wave;
        [SerializeField] AudioSource _previewer;

        public AudioSource Previewer
        {
            get
            {
                if (_previewer)
                {
                    return _previewer;
                }
                else
                {
                    var s = new GameObject("AudioPreviewer", new Type[] {typeof(AudioSource)});
                    s.hideFlags = HideFlags.DontSave;
                    _previewer = s.GetComponent<AudioSource>();
                    return _previewer;
                }
            }
        }

        

        


        public void SubscribeMethodToEvents()
        {
            // Debug.Log(eventHolderList.Count);
            for (int l = 0; l < eventHolderList.Count; l++)
            {
                EventHolder holder = eventHolderList[l];

                // Debug.Log(holder.eventInfoNames.Count);


                for (int j = 0; j < holder.eventInfoNames.Count; j++)
                {
                    Component _selectedComponent =
                        holder.eventer.GetComponents(typeof(Component))[holder.selectedComponent];

                    EventInfo selectedEvent = null;
                    for (int k = 0; k < holder.usableEvents.Length; k++)
                    {
                        if (holder.usableEvents[k].Name == holder.eventInfoNames[j])
                        {
                            selectedEvent = holder.usableEvents[k];
                            break;
                        }
                    }

                    // Debug.Log(selectedEvent.EventHandlerType.GetMethod("Invoke").GetParameters().Length);

                    // for (int k = 0; k < selectedEvent.EventHandlerType.GetMethod("Invoke").GetParameters().Length; k++)
                    // {
                    //     Debug.Log(selectedEvent.EventHandlerType.GetMethod("Invoke").GetParameters()[k].ParameterType);
                    // }

                    if (selectedEvent == null) return;
                    Delegate d = Delegate.CreateDelegate(selectedEvent.EventHandlerType, this, "PlaySoundSubscribe");

                    selectedEvent.AddEventHandler(_selectedComponent, d);

                    EditorUtility.SetDirty(_selectedComponent);
                }
            }
        }


        public void PlaySoundSubscribe(object sender, Transform parent, Vector3 position)
        {
            if (parent)
                if (position == Vector3.zero)
                    Play(sender, soundName, parent.position, parent);
                else
                    Play(sender, soundName, position, parent);
            else
                Play(sender, soundName);
        }

        public void PlaySoundSubscribe(object sender)
        {
            Play(sender, soundName);
        }

        public Sound(AudioClip clip, AudioMixerGroup mixer, bool loop, int priority, float volume, float pitch,
            bool spatialBlend, DimensionalSoundSettings settings)
        {
            this.soundName = clip ? clip.name : "New Sound";
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
            this.minDistance = 1;
            this.maxDistance = 500;
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "Assets/Simple Audio Manager/Textures/sound icon.png");
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
                        sounds[i].previewColor =
                            Color.HSVToRGB((((float) EditorApplication.timeSinceStartup * 100) % 100) / 100, 1, 1);
                    }

                    Handles.color = new Color(.5f, .5f, 1);
                    Handles.DrawWireDisc(sounds[i].soundPreviewer.position, new Vector3(0, 1, 0),
                        sounds[i].settings.minDistance);
                    Handles.color = new Color(.5f, .5f, 1, .05f);
                    Handles.SphereHandleCap(0, sounds[i].soundPreviewer.position, Quaternion.identity,
                        sounds[i].settings.minDistance * 2, EventType.Repaint);
                    Handles.color = new Color(sounds[i].previewColor.r, sounds[i].previewColor.g,
                        sounds[i].previewColor.b, 1);

                    Handles.DrawWireDisc(sounds[i].soundPreviewer.position, new Vector3(0, 1, 0),
                        sounds[i].settings.maxDistance);
                    Handles.color = new Color(sounds[i].previewColor.r, sounds[i].previewColor.g,
                        sounds[i].previewColor.b, .05f);
                    Handles.SphereHandleCap(0, sounds[i].soundPreviewer.position, Quaternion.identity,
                        sounds[i].settings.maxDistance * 2, EventType.Repaint);
                    Handles.DrawBezier(transform.position, sounds[i].soundPreviewer.transform.position,
                        transform.position + Vector3.down *
                        Mathf.Abs(transform.position.y - sounds[i].soundPreviewer.transform.position.y),
                        sounds[i].soundPreviewer.transform.position + Vector3.up *
                        Mathf.Abs(transform.position.y - sounds[i].soundPreviewer.transform.position.y), Color.cyan,
                        Texture2D.whiteTexture, 1);
                    Handles.Label(
                        sounds[i].soundPreviewer.position + Vector3.up * (sounds[i].settings.maxDistance + 10),
                        sounds[i].soundName);
                }
        }
    }

#endif
}