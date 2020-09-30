using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using System;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework.Constraints;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[ExecuteInEditMode]
[CustomEditor(typeof(AudioManager)), CanEditMultipleObjects]
public class AudioManagerEditor : Editor
{
    GUIStyle removeButtonStyle,
        moveButtonStyle,
        buttonNameToggle,
        buttonNameToggleBig,
        mainToolbarButton,
        toolbarButton,
        fieldColor,
        nullFieldColor,
        min,
        max,
        hoverableButton,
        nullHoverableButton;

    AudioManager manager;
    List<AudioSource> previewers = new List<AudioSource>();
    SerializedProperty propMixers;
    SerializedProperty propsounds;


    private List<AudioManager.Sound> sounds => manager.sounds;


    void OnEnable()
    {
        manager = (AudioManager) target;
        manager.transform.hideFlags = HideFlags.HideInInspector;

        UpdateMixerList();
        UpdateSoundList();

        for (int i = 0; i < manager.sounds.Count; i++)
        {
            if (manager.sounds[i].eventHolderList == null)
            {
                manager.sounds[i].eventHolderList = new List<AudioManager.EventHolder>();
            }
        }


        // SetupAudioManagerStyles();
    }

    void SetupAudioManagerStyles()
    {
        buttonNameToggle = new GUIStyle(GUI.skin.button);
        buttonNameToggle.normal.textColor = new Color(1, .7f, 0);
        buttonNameToggle.active.textColor = Color.cyan;
        buttonNameToggle.hover.textColor = Color.cyan;
        buttonNameToggle.font = (Font) Resources.Load("Fonts/Retron2000");

        buttonNameToggleBig = new GUIStyle(buttonNameToggle);
        buttonNameToggleBig.fontSize = 30;

        toolbarButton = new GUIStyle(EditorStyles.miniButtonMid);
        toolbarButton.normal.textColor = new Color(1, .7f, 0);
        toolbarButton.active.textColor = Color.cyan;
        toolbarButton.font = (Font) Resources.Load("Fonts/Retron2000");
        toolbarButton.fontSize = 20;
        toolbarButton.fixedHeight = 30;

        mainToolbarButton = new GUIStyle(toolbarButton);

        mainToolbarButton.fontSize = 15;
        mainToolbarButton.fixedHeight = 25;


        fieldColor = new GUIStyle(GUI.skin.label);
        fieldColor.normal.textColor = new Color(1, .7f, 0);
        fieldColor.font = (Font) Resources.Load("Fonts/Retron2000");

        nullFieldColor = new GUIStyle(GUI.skin.label);
        nullFieldColor.normal.textColor = Color.red;
        nullFieldColor.font = (Font) Resources.Load("Fonts/Retron2000");

        min = new GUIStyle(GUI.skin.label);
        max = new GUIStyle(GUI.skin.label);
        min.normal.textColor = new Color(.5f, .5f, 1);
        min.font = (Font) Resources.Load("Fonts/Retron2000");
        max.normal.textColor = new Color(.5f, 1, .5f);
        max.font = (Font) Resources.Load("Fonts/Retron2000");

        removeButtonStyle = new GUIStyle(GUI.skin.button);
        removeButtonStyle.normal.textColor = new Color(1, .5f, .5f);
        removeButtonStyle.font = (Font) Resources.Load("Fonts/Retron2000");

        moveButtonStyle = new GUIStyle(GUI.skin.button);
        moveButtonStyle.normal.textColor = new Color(.5f, .5f, 1);
        moveButtonStyle.font = (Font) Resources.Load("Fonts/Retron2000");

        hoverableButton = new GUIStyle(EditorStyles.toolbarButton);
        hoverableButton.normal.textColor = fieldColor.normal.textColor;
        hoverableButton.hover.textColor = new Color(0, 1, .7f);
        hoverableButton.focused.textColor = new Color(0, 1, .9f);
        hoverableButton.active.textColor = new Color(0, 1, .9f);
        hoverableButton.font = (Font) Resources.Load("Fonts/Retron2000");

        nullHoverableButton = new GUIStyle(EditorStyles.toolbarButton);
        nullHoverableButton.normal.textColor = Color.red;
        nullHoverableButton.hover.textColor = new Color(0, 1, .7f);
        nullHoverableButton.focused.textColor = new Color(0, 1, .9f);
        nullHoverableButton.active.textColor = new Color(0, 1, .9f);
        nullHoverableButton.font = (Font) Resources.Load("Fonts/Retron2000");
    }

    private void UpdateMixerList()
    {
        propMixers = serializedObject.FindProperty(nameof(manager.mixers));
    }

    void UpdateSoundList()
    {
        propsounds = serializedObject.FindProperty("sounds");
        foreach (var s in sounds)
        {
            previewers.Add(null);
        }
    }

    void DrawSoundProperty(SerializedProperty p)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(p, GUIContent.none);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawSoundPropertyAt(SerializedProperty pList, string propertyPath, string label, GUIStyle style, int i)
    {
        SerializedProperty tempProp = pList.GetArrayElementAtIndex(i).FindPropertyRelative(propertyPath);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(label, style, new GUILayoutOption[] {GUILayout.Width(120)});
            EditorGUILayout.PropertyField(tempProp, GUIContent.none);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawSoundPropertyAt(SerializedProperty pList, string propertyPath, int i)
    {
        SerializedProperty tempProp = pList.GetArrayElementAtIndex(i).FindPropertyRelative(propertyPath);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(tempProp, GUIContent.none);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawSoundPropertyAt(SerializedProperty pList, string propertyPath, int i, GUILayoutOption[] options)
    {
        SerializedProperty tempProp = pList.GetArrayElementAtIndex(i).FindPropertyRelative(propertyPath);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(tempProp, GUIContent.none, options);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawSoundPropertyAt(SerializedProperty pList, string propertyPath, int i, float maxWidth, string boolLable)
    {
        SerializedProperty tempProp = pList.GetArrayElementAtIndex(i).FindPropertyRelative(propertyPath);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(tempProp, new GUIContent("", boolLable), GUILayout.MaxWidth(maxWidth));
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawBoolLabel(ref bool condition, string label, GUIStyle style, GUILayoutOption[] options)
    {
        bool t = condition;
        condition = GUILayout.Toggle(condition, label, style, options);
        if (t != condition) EditorUtility.SetDirty(target);
    }

    private void DrawMinMaxSlider(ref float rangeX, ref float rangeY, int minLimit, int maxLimit,
        SerializedProperty serializedProperty, string propertyPath, int i, GUILayoutOption[] options)
    {
        EditorGUILayout.MinMaxSlider(ref rangeX, ref rangeY, minLimit, maxLimit, GUILayout.MinWidth(200));
        DrawSoundPropertyAt(serializedProperty, propertyPath, i, options);
    }

    void DrawParameterTab(int i)
    {
        using (new GUILayout.HorizontalScope())
        {
            DrawBoolLabel(ref sounds[i].volumeIsRange, "Volume", buttonNameToggle,
                new GUILayoutOption[] {GUILayout.Width(100)});
            if (!sounds[i].volumeIsRange)
                DrawSoundPropertyAt(propsounds, "volume", i);
            else
                DrawMinMaxSlider(ref sounds[i].volumeRange.x, ref sounds[i].volumeRange.y, 0, 1,
                    propsounds, "volumeRange", i, new GUILayoutOption[] {GUILayout.MaxWidth(200)});
        }

        using (new GUILayout.HorizontalScope())
        {
            DrawBoolLabel(ref sounds[i].priorityIsRange, "Priority", buttonNameToggle,
                new GUILayoutOption[] {GUILayout.Width(100)});

            if (!sounds[i].priorityIsRange)
                DrawSoundPropertyAt(propsounds, "priority", i);
            else
                DrawMinMaxSlider(ref sounds[i].priorityRange.x, ref sounds[i].priorityRange.y, 0,
                    256, propsounds, "priorityRange", i,
                    new GUILayoutOption[] {GUILayout.MaxWidth(200)});
        }

        using (new GUILayout.HorizontalScope())
        {
            DrawBoolLabel(ref sounds[i].pitchIsRange, "Pitch", buttonNameToggle,
                new GUILayoutOption[] {GUILayout.Width(100)});

            if (!sounds[i].pitchIsRange)
                DrawSoundPropertyAt(propsounds, "pitch", i);
            else
                DrawMinMaxSlider(ref sounds[i].pitchRange.x, ref sounds[i].pitchRange.y, 0, 3,
                    propsounds, "pitchRange", i, new GUILayoutOption[] {GUILayout.MaxWidth(200)});
        }

        DrawBoolLabel(ref sounds[i].loop, "Loop", buttonNameToggle,
            new GUILayoutOption[] {GUILayout.MinWidth(50), GUILayout.MinHeight(25)});
    }

    void DrawSpatialBlendTab(int i)
    {
        DrawBoolLabel(ref sounds[i].spatialBlend, "Use 3D Sound", buttonNameToggle,
            new GUILayoutOption[] {GUILayout.MinWidth(50), GUILayout.MinHeight(25)});
        // DrawSoundPropertyAt(propsounds, "loop", "Loop", fieldColor, i);
        // DrawSoundPropertyAt(propsounds, "spatialBlend", "3D Sound", fieldColor, i);
        if (sounds[i].spatialBlend)
        {
            DrawSoundPropertyAt(propsounds, "settings.minDistance", "Minimum Distance", min, i);
            DrawSoundPropertyAt(propsounds, "settings.maxDistance", "Maximum Distance", max, i);
            using (new GUILayout.HorizontalScope())
            {
                DrawSoundPropertyAt(propsounds, "soundPreviewer", "Sound Previewer", fieldColor, i);
                DrawSoundPropertyAt(propsounds, "previewColor", i, new GUILayoutOption[] {GUILayout.Width(50)});
                DrawSoundPropertyAt(propsounds, "soundVisibleInScene", i,
                    new GUILayoutOption[] {GUILayout.Width(15)});
            }
        }
    }

    void DrawMixersTab(int i)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (manager.mixers.Count > 0)
            {
                // GUILayout.Label("Mixers:", fieldColor, GUILayout.Height(30), GUILayout.MaxWidth(50));


                List<string> mixerNames = new List<string>();
                foreach (AudioMixerGroup m in manager.mixers)
                {
                    if (m)
                        mixerNames.Add(m.name);
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
                    var psm = sounds[i].selectedMixer;

                    sounds[i].selectedMixer = GUILayout.Toolbar(sounds[i].selectedMixer, names,
                        toolbarButton /*,GUILayout.MaxHeight(50)*/);
                    sounds[i].mixer = manager.mixers[sounds[i].selectedMixer];

                    if (psm != sounds[i].selectedMixer)
                    {
                        EditorUtility.SetDirty(target);
                    }
                }
            }
            else
            {
                GUILayout.Label("No mixers available.", fieldColor, GUILayout.Height(30));
            }
        }
    }

    void DrawEventsTab(int i)
    {
        sounds[i].eventTypeSelected = GUILayout.Toolbar(sounds[i].eventTypeSelected,
            new string[] {"Cause", "Effect"}, toolbarButton);
        switch (sounds[i].eventTypeSelected)
        {
            case 0:
                DrawSoundPropertyAt(propsounds, "e", i);
                break;
            case 1:
                DrawDelegateFinder(i);
                break;
        }
    }


    public override void OnInspectorGUI()
    {
        // base.DrawDefaultInspector();
        // return;
        SetupAudioManagerStyles();
        // EditorGUILayout.PropertyField(propMixers);


        using (new EditorGUILayout.VerticalScope("HelpBox"))
        {
            using (new EditorGUILayout.HorizontalScope("HelpBox"))
            {
                if (GUILayout.Button("Add Mixer", hoverableButton))
                {
                    AddMixer();
                }

                if (GUILayout.Button("Mixers...", hoverableButton, GUILayout.MaxWidth(60)))
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
                    SerializedProperty p = propMixers;

                    DrawSoundProperty(propMixers.GetArrayElementAtIndex(i));
                    if (GUILayout.Button("-", removeButtonStyle, GUILayout.MaxWidth(30)))
                    {
                        RemoveMixer(i);
                    }
                }
            }
        }

        GUILayout.Space(25);


        using (new EditorGUILayout.HorizontalScope("HelpBox"))
        {
            if (GUILayout.Button("Add Sound", hoverableButton))
            {
                AddSound();
            }

            if (GUILayout.Button("V", moveButtonStyle, GUILayout.MaxWidth(30)))
            {
                ToggleAllButtons();
            }
        }


        for (int i = 0; i < propsounds.arraySize; i++)
        {
            using (new EditorGUILayout.VerticalScope("HelpBox"))
            {
                using (new EditorGUILayout.HorizontalScope("HelpBox"))
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
                        sounds[i].soundName, hoverableButton);

                    if (prevVisibilityState != sounds[i].soundVisibleInInspector)
                    {
                        EditorUtility.SetDirty(target);
                    }


                    using (new EditorGUI.DisabledGroupScope(i == 0))
                        if (GUILayout.Button("↑", moveButtonStyle, GUILayout.MaxWidth(30)))
                        {
                            MoveSoundUp(i);
                        }

                    using (new EditorGUI.DisabledGroupScope(i == sounds.Count - 1))
                        if (GUILayout.Button("↓", moveButtonStyle, GUILayout.MaxWidth(30)))
                        {
                            MoveSoundDown(i);
                        }


                    if (GUILayout.Button("-", removeButtonStyle, GUILayout.MaxWidth(30)))
                    {
                        RemoveSoundAt(i);
                        return;
                    }
                }

                if (sounds[i].soundVisibleInInspector)
                {
                    using (new EditorGUILayout.HorizontalScope("HelpBox"))
                    {
                        AudioClip prevClip = sounds[i].clip;
                        DrawSoundPropertyAt(propsounds, "clip", i);


                        if (sounds[i].clip != prevClip && sounds[i].clip)
                        {
                            sounds[i].soundName = sounds[i].clip.name;
                        }

                        using (new EditorGUI.DisabledGroupScope(!sounds[i].clip))
                        {
                            DrawSoundPropertyAt(propsounds, "soundName", i);

                            if (!sounds[i].soundTesting)
                            {
                                sounds[i].soundTesting = GUILayout.Toggle(sounds[i].soundTesting, "Preview",
                                    buttonNameToggle);
                            }
                            else
                            {
                                if (previewers[i] != null && previewers[i].isPlaying)
                                {
                                    if (GUILayout.Button("❚❚", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                                    {
                                        previewers[i].Pause();
                                        sounds[i].paused = true;
                                    }
                                }

                                if (previewers[i] != null && !previewers[i].isPlaying)
                                {
                                    if (GUILayout.Button("►", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                                    {
                                        previewers[i].UnPause();
                                        sounds[i].paused = false;
                                    }
                                }

                                sounds[i].soundTesting = GUILayout.Toggle(sounds[i].soundTesting, "■",
                                    EditorStyles.miniButton, GUILayout.MaxWidth(50));
                            }
                            
                            if (sounds[i].soundTesting)
                            {
                                if (previewers[i] == null)
                                {
                                    PreviewSound(sounds[i], i);
                                }
                                else if (!previewers[i].isPlaying && !sounds[i].paused)
                                {
                                    DestroySound(previewers[i], i);
                                }
                            }
                            else
                            {
                                if (previewers[i] != null)
                                {
                                    DestroySound(previewers[i], i);
                                }
                            }
                        }
                    }

                    if (sounds[i].clip)
                    {
                        sounds[i].selectedTab = GUILayout.Toolbar(sounds[i].selectedTab,
                            new[] {"Parameters", "Spatial Blend", "Mixer", "Events"}, mainToolbarButton);

                        // using (new GUILayout.scop("HelpBox"))
                        {
                            switch (sounds[i].selectedTab)
                            {
                                case 0:
                                    DrawParameterTab(i);
                                    break;
                                case 1:
                                    DrawSpatialBlendTab(i);
                                    break;
                                case 2:
                                    DrawMixersTab(i);
                                    break;
                                case 3:
                                    DrawEventsTab(i);
                                    break;
                            }
                        }
                    }


                    // DrawBoolLabel(ref sounds[i].unityEventsVisible, "Events", buttonNameToggle,
                    //     new GUILayoutOption[] {GUILayout.MinWidth(100), GUILayout.MinHeight(15)});
                    // if (sounds[i].unityEventsVisible)
                    // {
                    //     
                    // }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }


    private void DrawDelegateFinder(int i)
    {
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button(" + ", buttonNameToggle))
            {
                sounds[i].eventHolderList.Add(new AudioManager.EventHolder());
                return;
            }
        }

        for (int l = 0; l < sounds[i].eventHolderList.Count; l++)
        {
            AudioManager.EventHolder holder = sounds[i].eventHolderList[l];
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope("HelpBox"))
                    {
                        // DrawSoundPropertyAt(propsounds, "eventHolders[l]", i);
                        EditorGUILayout.PropertyField(propsounds.GetArrayElementAtIndex(i)
                            .FindPropertyRelative("eventHolderList").GetArrayElementAtIndex(l)
                            .FindPropertyRelative("eventer"), GUIContent.none);
                        int j = holder.selectedComponent;
                        if (holder.eventer)
                        {
                            GUILayout.Label("Component: ", fieldColor);
                            sounds[i].eventHolderList[l].selectedComponent = EditorGUILayout.Popup(
                                holder.selectedComponent,
                                GetNamesFromComponents(holder.eventer
                                    .GetComponents(typeof(Component))));
                            if (j != holder.selectedComponent) EditorUtility.SetDirty(target);
                        }
                    }

                    if (holder.eventer)
                    {
                        List<EventInfo> eventInfos =
                            holder.eventer.GetComponents(typeof(Component))[holder.selectedComponent]
                                .GetType().GetEvents().ToList();

                        List<EventInfo> eventInfosTemp = new List<EventInfo>();

                        for (int j = 0; j < eventInfos.Count; j++)
                        {
                            if (eventInfos[j].EventHandlerType.GetMethod("Invoke").GetParameters().Length == 0)
                                eventInfosTemp.Add(eventInfos[j]);
                        }

                        eventInfos = eventInfosTemp;

                        // EventInfo selectedEvent = null;
                        // if (eventInfos.Count != 0)
                        // {
                        //     selectedEvent = eventInfos[holder.selectedEvents];
                        // }
                        // else return;

                        if (eventInfos.Count > 0)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("Event: ", fieldColor, GUILayout.MaxWidth(50));

                                int k = holder.selectedEvents;
                                // holder.selectedEvents = EditorGUILayout.Popup(holder.selectedEvents, GetNamesFromEvents(eventInfos.ToArray()));
                                holder.selectedEvents = EditorGUILayout.MaskField(holder.selectedEvents,
                                    GetNamesFromEvents(eventInfos.ToArray()));
                                if (k != holder.selectedEvents) EditorUtility.SetDirty(target);
                                // GUILayout.Label(holder.selectedEvents.ToString());
                                // GUILayout.Label(System.Convert.ToString(holder.selectedEvents, 2));


                                // GUILayout.Toggle(holder._mask[0],GUIContent.none);
                                // GUILayout.Toggle(holder._mask[1],GUIContent.none);
                                // GUILayout.Toggle(holder._mask[2],GUIContent.none);
                                // GUILayout.Toggle(holder._mask[3],GUIContent.none);
                                // GUILayout.Toggle(holder._mask[4],GUIContent.none);
                                // GUILayout.Toggle(holder._mask[5],GUIContent.none);
                                // GUILayout.Toggle(holder._mask[6],GUIContent.none);

                                // bool isSubscribed = false;
                                // for (int j = 0; j < holder.eventInfoNames.Count; j++)
                                // {
                                //     if (selectedEvent.Name == holder.eventInfoNames[j]) isSubscribed = true;
                                // }

                                // if (isSubscribed)
                                // {
                                //     if (GUILayout.Button("Unsubscribe", buttonNameToggle))
                                //     {
                                //         holder.eventInfoNames.Remove(selectedEvent.Name);
                                //         EditorUtility.SetDirty(target);
                                //     }
                                // }
                                // else if (GUILayout.Button("Subscribe", buttonNameToggle))
                                // {
                                //     holder.eventInfoNames.Add(selectedEvent.Name);
                                //     EditorUtility.SetDirty(target);
                                // }
                            }
                        }
                        else
                        {
                            GUILayout.Label("No usable events available in that component!", fieldColor);
                        }
                    }

                    // List<string> temp = holder.eventInfoNames;
                    // for (int j = 0; j < temp.Count; j++)
                    // {
                    //     GUILayout.Label(temp[j]);
                    // }
                }

                if (holder.eventer)
                    if (GUILayout.Button(" - ", buttonNameToggleBig))
                    {
                        sounds[i].eventHolderList.Remove(holder);
                    }
            }
        }
    }

    private string[] GetNamesFromComponents(Component[] components)
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


    private void AddMixer()
    {
        manager.mixers.Add(manager.mixers.Count > 0 ? manager.mixers[0] : null);
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            if (sounds[i].soundVisibleInScene)
            {
                Handles.color = new Color(sounds[i].previewColor.r, sounds[i].previewColor.g, sounds[i].previewColor.b,
                    1);
                EditorGUI.BeginChangeCheck();

                float s = Handles.ScaleValueHandle(sounds[i].settings.maxDistance, sounds[i].soundPreviewer.position,
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
        UpdateSoundList();
    }

    void RemoveSoundAt(int i)
    {
        sounds.RemoveAt(i);
        UpdateSoundList();
    }

    void PreviewSound(AudioManager.Sound sound, int i)
    {
        previewers[i] = EditorUtility
            .CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave, typeof(AudioSource))
            .GetComponent<AudioSource>();

        int spatialBlend = sound.spatialBlend ? 1 : 0;

        previewers[i].clip = sound.clip;
        previewers[i].outputAudioMixerGroup = sound.mixer;
        previewers[i].loop = sound.loop;
        previewers[i].volume = sound.volumeIsRange
            ? UnityEngine.Random.Range(sound.volumeRange.x, sound.volumeRange.y)
            : sound.volume;
        previewers[i].pitch = sound.pitchIsRange
            ? UnityEngine.Random.Range(sound.pitchRange.x, sound.pitchRange.y)
            : sound.pitch;
        previewers[i].priority = sound.priorityIsRange
            ? (int) UnityEngine.Random.Range(sound.priorityRange.x, sound.priorityRange.y)
            : sound.priority;
        previewers[i].minDistance = sound.settings.minDistance;
        previewers[i].maxDistance = sound.settings.maxDistance;


        previewers[i].Play();
        sound.e?.Invoke();
        // EditorCoroutineUtility.StartCoroutine(DestroyOnDelay(sound.clip.length, previewer, i), this);
    }

    void DestroySound(AudioSource source, int i)
    {
        if (source != null)
        {
            DestroyImmediate(source.gameObject);
            sounds[i].soundTesting = false;
        }
    }

    void MoveSoundUp(int index) // 1
    {
        List<AudioManager.Sound> stemp = new List<AudioManager.Sound>(sounds); //0, 1
        sounds[index - 1] = sounds[index]; // 1,1
        sounds[index] = stemp[index - 1]; //1,0  
    }

    void MoveSoundDown(int index) // 0
    {
        List<AudioManager.Sound> stemp = new List<AudioManager.Sound>(sounds); //0, 1
        sounds[index] = sounds[index + 1]; // 1,1
        sounds[index + 1] = stemp[index]; //1,0
    }

    void RemoveMixer(int i)
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

        serializedObject.ApplyModifiedProperties();
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