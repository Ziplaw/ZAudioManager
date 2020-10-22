using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using System;
using System.Reflection;
using ZAudioManagerUtils;
using Object = UnityEngine.Object;
using Styles = ZAudioManagerUtils.Styles;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    [Serializable]
    public class Previewer
    {
        public AudioSource previewer;
        private AudioManager.Sound _sound;
        public float progress 
        {
            get
            {
                return previewer.time / (Sound.clip.length);
            }
        }

        public AudioManager.Sound Sound
        {
            get => _sound;
            set
            {
                _sound = value;
                previewer.clip = _sound.clip;
                SetupPreviewer();
            } 
        }

        public void SetupPreviewer()
        {
            int spatialBlend = Sound.spatialBlend ? 1 : 0;

            Sound.Previewer.clip = Sound.clip;
            Sound.Previewer.outputAudioMixerGroup = Sound.mixer;
            Sound.Previewer.loop = Sound.loop;
            Sound.Previewer.volume = Sound.volumeIsRange
                ? UnityEngine.Random.Range(Sound.volumeRange.x, Sound.volumeRange.y)
                : Sound.volume;
            Sound.Previewer.pitch = Sound.pitchIsRange
                ? UnityEngine.Random.Range(Sound.pitchRange.x, Sound.pitchRange.y)
                : Sound.pitch;
            Sound.Previewer.priority = Sound.priorityIsRange
                ? (int) UnityEngine.Random.Range(Sound.priorityRange.x, Sound.priorityRange.y)
                : Sound.priority;
            Sound.Previewer.minDistance = Sound.settings.minDistance;
            Sound.Previewer.maxDistance = Sound.settings.maxDistance;
        }

        public void Play()
        {
            Sound.Previewer.Play();
            Sound.e?.Invoke();
        }

        public Previewer(AudioSource previewer, AudioManager.Sound sound)
        {
            this.previewer = previewer;
            this.Sound = sound;
            SetupPreviewer();
        }
    }
    
    static AudioManager manager;
    SerializedProperty propMixers;
    SerializedProperty propsounds;
    public static Previewer currentPreviewer;

    


    private List<AudioManager.Sound> sounds => manager.sounds;


    void OnEnable()
    {
        manager = (AudioManager) target;
        manager.transform.hideFlags = HideFlags.HideInInspector;

        SetMixerList();
        SetSoundList();

        for (int i = 0; i < manager.sounds.Count; i++)
        {
            if (manager.sounds[i].eventHolderList == null)
            {
                manager.sounds[i].eventHolderList = new List<AudioManager.EventHolder>();
            }
        }
    }

    private void SetMixerList()
    {
        propMixers = serializedObject.FindProperty(nameof(manager.mixers));
    }

    void SetSoundList()
    {
        propsounds = serializedObject.FindProperty("sounds");
    }

    static void DrawParameterTab(AudioManager.Sound sound, SerializedProperty propsounds, int i,
        UnityEngine.Object manager)
    {
        using (new GUILayout.HorizontalScope())
        {
            EditorFunctions.DrawBoolLabel(ref sound.volumeIsRange, "Volume", Styles.buttonNameToggle,
                new GUILayoutOption[] {GUILayout.Width(100)}, manager);
            if (!sound.volumeIsRange)
                EditorFunctions.DrawSoundPropertyAt(propsounds, "volume", i);
            else
                EditorFunctions.DrawMinMaxSlider(ref sound.volumeRange.x, ref sound.volumeRange.y, 0, 1,
                    propsounds, "volumeRange", i, new GUILayoutOption[] { },
                    new GUILayoutOption[] {GUILayout.MaxWidth(200)});
        }

        using (new GUILayout.HorizontalScope())
        {
            EditorFunctions.DrawBoolLabel(ref sound.priorityIsRange, "Priority", Styles.buttonNameToggle,
                new GUILayoutOption[] {GUILayout.Width(100)}, manager);

            if (!sound.priorityIsRange)
                EditorFunctions.DrawSoundPropertyAt(propsounds, "priority", i);
            else
                EditorFunctions.DrawMinMaxSlider(ref sound.priorityRange.x, ref sound.priorityRange.y, 0,
                    256, propsounds, "priorityRange", i,
                    new GUILayoutOption[] { }, new GUILayoutOption[] {GUILayout.MaxWidth(200)});
        }

        using (new GUILayout.HorizontalScope())
        {
            EditorFunctions.DrawBoolLabel(ref sound.pitchIsRange, "Pitch", Styles.buttonNameToggle,
                new GUILayoutOption[] {GUILayout.Width(100)}, manager);

            if (!sound.pitchIsRange)
                EditorFunctions.DrawSoundPropertyAt(propsounds, "pitch", i);
            else
                EditorFunctions.DrawMinMaxSlider(ref sound.pitchRange.x, ref sound.pitchRange.y, 0, 3,
                    propsounds, "pitchRange", i, new GUILayoutOption[] { },
                    new GUILayoutOption[] {GUILayout.MaxWidth(200)});
        }

        EditorFunctions.DrawBoolLabel(ref sound.loop, "Loop", Styles.buttonNameToggle,
            new GUILayoutOption[] {GUILayout.MinWidth(50), GUILayout.MinHeight(25)}, manager);
    }

    static void DrawSpatialBlendTab(AudioManager.Sound sound, SerializedProperty propsounds, int i,
        UnityEngine.Object manager)
    {
        var t = sound.spatialBlend;
        EditorFunctions.DrawBoolLabel(ref sound.spatialBlend, "Use 3D Sound", Styles.buttonNameToggle,
            new GUILayoutOption[] {GUILayout.MinWidth(50), GUILayout.MinHeight(25)}, manager);
        if (t != sound.spatialBlend) sound.eventHolderList = new List<AudioManager.EventHolder>();

        if (sound.spatialBlend)
        {
            EditorFunctions.DrawSoundPropertyAt(propsounds, "settings.minDistance", "Minimum Distance", Styles.min, i);
            EditorFunctions.DrawSoundPropertyAt(propsounds, "settings.maxDistance", "Maximum Distance", Styles.max, i);
            using (new GUILayout.HorizontalScope())
            {
                EditorFunctions.DrawSoundPropertyAt(propsounds, "soundPreviewer", "Sound Previewer", Styles.fieldColor,
                    i);
                EditorFunctions.DrawSoundPropertyAt(propsounds, "previewColor", i,
                    new GUILayoutOption[] {GUILayout.Width(50)});
                EditorFunctions.DrawSoundPropertyAt(propsounds, "soundVisibleInScene", i,
                    new GUILayoutOption[] {GUILayout.Width(15)});
            }
        }
    }

    static void DrawMixersTab(List<AudioManager.Sound> sounds, int i, UnityEngine.Object _manager,
        SerializedProperty propMixers)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (manager.mixers.Count > 0)
            {
                List<string> mixerNames = new List<string>();
                foreach (AudioMixerGroup m in manager.mixers)
                {
                    if (m)
                        mixerNames.Add(m.name + " (" + m.audioMixer.name + ")");
                    else
                        mixerNames.Add("");
                }

                string[] names = mixerNames.ToArray();

                bool anyDisabled = false;

                foreach (string name in names)
                {
                    if (name == "") anyDisabled = true;
                }

                using (new EditorGUI.DisabledGroupScope(anyDisabled))
                {
                    int psm = sounds[i].selectedMixer;

                    sounds[i].selectedMixer = GUILayout.Toolbar(sounds[i].selectedMixer, names,
                        Styles.toolbarButton, GUILayout.MinWidth(0));
                    sounds[i].mixer = manager.mixers[sounds[i].selectedMixer];

                    if (psm != sounds[i].selectedMixer)
                    {
                        EditorUtility.SetDirty(manager);
                    }
                }
            }
            else
            {
                GUILayout.Label("No mixers available.", Styles.fieldColor, GUILayout.Height(30));
            }

            GUILayout.Space(10);
            EditorFunctions.DrawBoolLabel(ref sounds[i].mixersVisibleInInspector, "...", Styles.toolbarButton,
                new GUILayoutOption[] {GUILayout.Width(30)}, _manager);
        }


        if (sounds[i].mixersVisibleInInspector)
        {
            DrawMixerManager(sounds, propMixers);
        }
    }

    private static void DrawMixerManager(List<AudioManager.Sound> sounds, SerializedProperty propMixers)
    {
        using (new EditorGUILayout.VerticalScope())
        {
            using (new EditorGUILayout.HorizontalScope("HelpBox"))
            {
                if (GUILayout.Button("Add Mixer", Styles.hoverableButton))
                {
                    AddMixer();
                }

                if (GUILayout.Button("Mixers...", Styles.hoverableButton, GUILayout.MaxWidth(70)))
                {
                    System.Type windowType =
                        typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AudioMixerWindow");
                    EditorWindow window = EditorWindow.GetWindow(windowType);
                }
            }

            for (int i = 0; i < propMixers.arraySize; i++)
            {
                using (new EditorGUILayout.HorizontalScope("HelpBox"))
                {
                    EditorFunctions.DrawSoundProperty(propMixers.GetArrayElementAtIndex(i));
                    if (GUILayout.Button("-", Styles.removeButtonStyle, GUILayout.MaxWidth(30)))
                    {
                        RemoveMixer(sounds, i);
                    }
                }
            }
        }
    }

    static void DrawEventsTab(AudioManager.Sound sound, SerializedProperty propSounds, int i)
    {
        sound.eventTypeSelected = GUILayout.Toolbar(sound.eventTypeSelected,
            new string[] {"Cause", "Effect"}, Styles.toolbarButton);
        switch (sound.eventTypeSelected)
        {
            case 0:
                EditorFunctions.DrawSoundPropertyAt(propSounds, "e", i);
                break;
            case 1:
                DrawDelegateFinder(sound, propSounds, i);
                break;
        }
    }


    public override void OnInspectorGUI()
    {
        // base.DrawDefaultInspector();
        // return;

        if (currentPreviewer != null) Repaint();

        using (new EditorGUILayout.HorizontalScope("HelpBox"))
        {
            if (GUILayout.Button("Add Sound", Styles.hoverableButton))
            {
                AddSound();
            }

            if (GUILayout.Button("V", Styles.moveButtonStyle, GUILayout.MaxWidth(30)))
            {
                ToggleAllButtons();
            }
        }
        var p = (from s in sounds select s.pitch).ToArray();
        var v = (from s in sounds select s.volume).ToArray();

        DrawSound(sounds, propsounds, false, manager, serializedObject, propMixers);
        
        if(currentPreviewer != null)
        {
            for (var i = 0; i < p.Length; i++)
            {
                if (p[i] != sounds[i].pitch || v[i] != sounds[i].volume)
                {
                    currentPreviewer.Sound = sounds[i];
                }
            }
        }
    }

    public static void DrawSound(List<AudioManager.Sound> sounds, SerializedProperty _propSounds, 
        bool onlyDrawParameters, Object _manager, SerializedObject _serializedObject, SerializedProperty propMixers)
    {
        for (int i = 0; i < _propSounds.arraySize; i++)
        {

            using (new EditorGUILayout.VerticalScope("HelpBox"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool prevVisibilityState = sounds[i].soundVisibleInInspector;

                    if (!sounds[i].clip)
                        if (GUILayout.Button(
                            AssetDatabase.LoadAssetAtPath("Assets/Simple Audio Manager/Textures/warning icon.png",
                                typeof(Texture2D)) as Texture2D, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
                        {
                            AudioClip[] clips = Resources.FindObjectsOfTypeAll<AudioClip>();
                            sounds[i].clip = clips.Length > 0 ? clips[0] : null;
                        }

                    sounds[i].soundVisibleInInspector = GUILayout.Toggle(sounds[i].soundVisibleInInspector,
                        sounds[i].soundName, Styles.hoverableButton);

                    if (prevVisibilityState != sounds[i].soundVisibleInInspector)
                    {
                        EditorUtility.SetDirty(_manager);
                    }


                    using (new EditorGUI.DisabledGroupScope(i == 0))
                        if (GUILayout.Button("↑", Styles.moveButtonStyle, GUILayout.MaxWidth(30)))
                        {
                            MoveSoundUp(sounds, i);
                        }

                    using (new EditorGUI.DisabledGroupScope(i == sounds.Count - 1))
                        if (GUILayout.Button("↓", Styles.moveButtonStyle, GUILayout.MaxWidth(30)))
                        {
                            MoveSoundDown(sounds, i);
                        }


                    if (GUILayout.Button("-", Styles.removeButtonStyle, GUILayout.MaxWidth(30)))
                    {
                        RemoveSoundAt(sounds, i, _propSounds, _serializedObject);
                        DestroySound(currentPreviewer);
                        return;
                    }
                }


                if (sounds[i].soundVisibleInInspector)
                {
                    using (new EditorGUILayout.HorizontalScope("HelpBox"))
                    {
                        AudioClip prevClip = sounds[i].clip;
                        EditorFunctions.DrawSoundPropertyAt(_propSounds, "clip", i);


                        if (sounds[i].clip != prevClip && sounds[i].clip)
                        {
                            sounds[i].soundName = sounds[i].clip.name;
                        }

                        using (new EditorGUI.DisabledGroupScope(!sounds[i].clip))
                        {
                            EditorFunctions.DrawSoundPropertyAt(_propSounds, "soundName", i);

                            if (currentPreviewer == null)
                            {
                                if (GUILayout.Button("Preview", Styles.buttonNameToggle))
                                {
                                    currentPreviewer = PreviewSound(sounds[i]);
                                }
                            }

                            else if (currentPreviewer.previewer && currentPreviewer.Sound == sounds[i])
                            {
                                if (!currentPreviewer.previewer.isPlaying && sounds[i].paused)
                                {
                                    if (GUILayout.Button("►", Styles.buttonNameToggle, GUILayout.MaxWidth(50)))
                                    {
                                        sounds[i].Previewer.UnPause();
                                        sounds[i].paused = false;
                                    }
                                }

                                if (currentPreviewer.previewer.isPlaying)
                                {
                                    if (GUILayout.Button("❚❚", Styles.buttonNameToggle, GUILayout.MaxWidth(50)))
                                    {
                                        sounds[i].Previewer.Pause();
                                        sounds[i].paused = true;
                                    }
                                }

                                if (GUILayout.Button("■", Styles.buttonNameToggle, GUILayout.MaxWidth(50)))
                                {
                                    DestroySound(currentPreviewer);
                                    sounds[i].paused = false;
                                    return;
                                }

                                if (!currentPreviewer.previewer.isPlaying && !sounds[i].paused)
                                {
                                    DestroySound(currentPreviewer);
                                    sounds[i].paused = false;
                                    return;
                                }
                            }


                            // if (!sounds[i].soundTesting)
                            // {
                            //     sounds[i].soundTesting = GUILayout.Toggle(sounds[i].soundTesting, "Preview",
                            //         Styles.buttonNameToggle);
                            // }
                            // else
                            // {
                            //     if (sounds[i].Previewer != null && sounds[i].Previewer.isPlaying)
                            //     {
                            //         if (GUILayout.Button("❚❚", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                            //         {
                            //             sounds[i].Previewer.Pause();
                            //             sounds[i].paused = true;
                            //         }
                            //     }
                            //
                            //     if (sounds[i].Previewer != null && !sounds[i].Previewer.isPlaying)
                            //     {
                            //         if (GUILayout.Button("►", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                            //         {
                            //             sounds[i].Previewer.UnPause();
                            //             sounds[i].paused = false;
                            //         }
                            //     }
                            //
                            //     sounds[i].soundTesting = GUILayout.Toggle(sounds[i].soundTesting, "■",
                            //         EditorStyles.miniButton, GUILayout.MaxWidth(50));
                            // }
                            //
                            // if (sounds[i].soundTesting)
                            // {
                            //     PreviewSound(sounds[i]);
                            //
                            //     if (!sounds[i].Previewer.isPlaying && !sounds[i].paused)
                            //     {
                            //         DestroySound(sounds[i].Previewer, i);
                            //         sounds[i].soundTesting = false;
                            //     }
                            // }
                            // else
                            // {
                            //     if (sounds[i].Previewer != null)
                            //     {
                            //         DestroySound(sounds[i].Previewer, i);
                            //     }
                            // }
                        }
                    }

                    if (sounds[i].clip)
                    {
                        
                        
                        sounds[i].selectedTab =  
                            GUILayout.Toolbar(sounds[i].selectedTab, onlyDrawParameters ? new[] {"Parameters"} : new [] { "Parameters", "Spatial Blend", "Mixer", "Events"}, Styles.mainToolbarButton);
                      
                        switch (sounds[i].selectedTab)
                        {
                            case 0:
                                var p = sounds[i].pitch;
                                DrawParameterTab(sounds[i], _propSounds, i, _manager);
                                if(sounds[i].pitch != p) Debug.Log("si");
                                break;
                            case 1:
                                DrawSpatialBlendTab(sounds[i], _propSounds, i, _manager);
                                break;
                            case 2:
                                DrawMixersTab(sounds, i, _manager, propMixers);
                                break;
                            case 3:
                                DrawEventsTab(sounds[i], _propSounds, i);
                                break;
                        }
                    }
                }
            }
        }

        _serializedObject.ApplyModifiedProperties();
        _serializedObject.Update();
    }
    private static void DrawDelegateFinder(AudioManager.Sound sound, SerializedProperty propSounds, int i)
    {
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button(" + ", Styles.buttonNameToggle))
            {
                sound.eventHolderList.Add(new AudioManager.EventHolder(sound.spatialBlend));
                Debug.Log(sound.spatialBlend);
                return;
            }
        }

        for (int l = 0; l < sound.eventHolderList.Count; l++)
        {
            AudioManager.EventHolder holder = sound.eventHolderList[l];
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope("HelpBox"))
                    {
                        // DrawSoundPropertyAt(propsounds, "eventHolders[l]", i);
                        EditorGUILayout.PropertyField(propSounds.GetArrayElementAtIndex(i)
                            .FindPropertyRelative("eventHolderList").GetArrayElementAtIndex(l)
                            .FindPropertyRelative("eventer"), GUIContent.none);
                        int j = holder.selectedComponent;
                        if (holder.eventer)
                        {
                            GUILayout.Label("Component: ", Styles.fieldColor);
                            sound.eventHolderList[l].selectedComponent = EditorGUILayout.Popup(
                                holder.selectedComponent,
                                GetNamesFromComponents(holder.eventer
                                    .GetComponents(typeof(Component))));
                            if (j != holder.selectedComponent) EditorUtility.SetDirty(manager);
                        }
                    }

                    if (holder.eventer)
                    {
                        List<EventInfo> eventInfos =
                            holder.eventer.GetComponents(typeof(Component))[holder.selectedComponent]
                                .GetType().GetEvents().ToList();

                        List<EventInfo> eventInfosTemp = new List<EventInfo>();
                        List<string> usableEvents = new List<string>();

                        for (int j = 0; j < eventInfos.Count; j++)
                        {
                            // Debug.Log(typeof(AudioManager.Sound).GetMethod("PlaySoundSubscribe").GetParameters() == eventInfos[j].EventHandlerType.GetMethod("Invoke").GetParameters());
                            // Debug.Log(eventInfos[j].EventHandlerType.GetMethod("Invoke").GetParameters().Length);

                            MethodInfo subscriberMethod = null;

                            switch (sound.spatialBlend)
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

                            // Debug.Log(subscriberMethod);


                            var parameterTypeNames =
                                from p in subscriberMethod.GetParameters()
                                select p.ParameterType.FullName;

                            var eventTypeNames =
                                from p in eventInfos[j].EventHandlerType.GetMethod("Invoke").GetParameters()
                                select p.ParameterType.FullName;

                            var parameterTypeArray = parameterTypeNames.ToArray();
                            var eventTypeArray = eventTypeNames.ToArray();

                            if (EqualsList(parameterTypeArray.ToList(), eventTypeArray.ToList()))
                            {
                                eventInfosTemp.Add(eventInfos[j]);
                                usableEvents.Add(eventInfos[j].Name);
                            }
                        }

                        eventInfos = eventInfosTemp;

                        if (eventInfos.Count > 0)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("Event: ", Styles.fieldColor, GUILayout.MaxWidth(50));

                                int k = holder.selectedEvents;
                                // holder.selectedEvents = EditorGUILayout.MaskField(holder.selectedEvents, GetNamesFromEvents(eventInfos.ToArray()));
                                holder.selectedEvents =
                                    EditorGUILayout.MaskField(holder.selectedEvents, usableEvents.ToArray());
                                if (k != holder.selectedEvents) EditorUtility.SetDirty(manager);
                            }
                        }
                        else
                        {
                            GUILayout.Label("No usable events available in that component!", Styles.fieldColor);
                        }
                    }
                }

                if (holder.eventer)
                    if (GUILayout.Button(" - ", Styles.buttonNameToggleBig))
                    {
                        sound.eventHolderList.Remove(holder);
                    }
            }
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


    private static string[] GetNamesFromComponents(Component[] components)
    {
        string[] names = new string[components.Length];

        for (int i = 0; i < names.Length; i++)
        {
            names[i] = components[i].GetType().ToString();
        }

        return names;
    }

    private string[] GetNamesFromEvents(EventInfo[] events)
    {
        List<string> names = new List<string>();

        for (int i = 0; i < events.Length; i++)
        {
            if (events[i].EventHandlerType.GetMethod("Invoke").GetParameters().Length == 0)
            {
                names.Add(events[i].Name);
            }
        }

        return names.ToArray();
    }


    private static void AddMixer()
    {
        manager.mixers.Add(manager.mixers.Count > 0 ? manager.mixers[0] : null);
    }

    void OnSceneGUI()
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            if (sounds[i].soundVisibleInScene)
            {
                Handles.color = new Color(sounds[i].previewColor.r, sounds[i].previewColor.g,
                    sounds[i].previewColor.b,
                    1);
                EditorGUI.BeginChangeCheck();

                float s = Handles.ScaleValueHandle(sounds[i].settings.maxDistance,
                    sounds[i].soundPreviewer.position,
                    Quaternion.identity, sounds[i].settings.maxDistance * .5f, Handles.CubeHandleCap, .1f);
                if (EditorGUI.EndChangeCheck())
                {
                    sounds[i].settings.maxDistance = s;
                }
            }
        }
    }

    private void AddSound()
    {
        sounds.Add(new AudioManager.Sound(null, null, false, 128, 1, 1, false,
            new AudioManager.DimensionalSoundSettings()));
        serializedObject.Update();
    }

    static void RemoveSoundAt(List<AudioManager.Sound> sounds, int i, SerializedProperty propSounds,
        SerializedObject serializedObject)
    {
        sounds.RemoveAt(i);
        serializedObject.Update();
    }

    public static Previewer PreviewSound(AudioManager.Sound sound)
    {
        // Debug.Log(soundStartedPreviewing);
        // if (!soundStartedPreviewing)
        {
            // soundStartedPreviewing = true;

            var p = new Previewer(sound.Previewer, sound);
            p.Play();
            return p;
        }
    }

    static void DestroySound(Previewer source)
    {
        if (source != null)
        {
            DestroyImmediate(source.previewer.gameObject);
            currentPreviewer = null;
        }
    }

    static void MoveSoundUp(List<AudioManager.Sound> sounds, int index) // 1
    {
        List<AudioManager.Sound> stemp = new List<AudioManager.Sound>(sounds); //0, 1
        sounds[index - 1] = sounds[index]; // 1,1
        sounds[index] = stemp[index - 1]; //1,0  
    }

    static void MoveSoundDown(List<AudioManager.Sound> sounds, int index) // 0
    {
        List<AudioManager.Sound> stemp = new List<AudioManager.Sound>(sounds); //0, 1
        sounds[index] = sounds[index + 1]; // 1,1
        sounds[index + 1] = stemp[index]; //1,0
    }

    static void RemoveMixer(List<AudioManager.Sound> sounds, int i)
    {
        manager.mixers.RemoveAt(i);
        // propMixers.RemoveAt(i);

        for (int j = 0; j < sounds.Count; j++)
        {
            if (sounds[j].selectedMixer == i)
            {
                sounds[j].selectedMixer = 0;
            }

            if (sounds[j].selectedMixer > i)
            {
                sounds[j].selectedMixer--;
            }
        }
    }

    void ToggleAllButtons()
    {
        bool hasToggled = false;
        bool hasUnToggled = false;

        for (int i = 0; i < sounds.Count; i++)
        {
            if (sounds[i].soundVisibleInInspector == false)
            {
                hasUnToggled = true;
            }

            if (sounds[i].soundVisibleInInspector == true)
            {
                hasToggled = true;
            }
        }

        if (hasToggled && hasUnToggled)
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                sounds[i].soundVisibleInInspector = true;
            }
        }

        if (hasUnToggled && !hasToggled)
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                sounds[i].soundVisibleInInspector = true;
            }
        }

        if (!hasUnToggled && hasToggled)
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                sounds[i].soundVisibleInInspector = false;
            }
        }
    }
}