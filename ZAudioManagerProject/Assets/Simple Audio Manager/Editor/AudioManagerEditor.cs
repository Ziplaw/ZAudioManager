﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework.Constraints;
using UnityEditor.EditorTools;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(AudioManager))]
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
        buttonNameToggle.onNormal.textColor = Color.cyan;


        buttonNameToggleBig = new GUIStyle(buttonNameToggle);
        buttonNameToggleBig.fontSize = 30;

        toolbarButton = new GUIStyle(EditorStyles.objectFieldThumb);
        toolbarButton.normal.textColor = new Color(1f, .7f, 0);
        toolbarButton.active.textColor = new Color(0, 1f, .5f);
        toolbarButton.font = (Font) Resources.Load("Fonts/Retron2000");
        toolbarButton.fontSize = 10;
        toolbarButton.fixedHeight = 20;
        toolbarButton.alignment = TextAnchor.MiddleCenter;
        toolbarButton.onNormal.textColor =new Color(0, 1, .5f);
        toolbarButton.clipping = TextClipping.Clip;


        mainToolbarButton = new GUIStyle(toolbarButton);

        mainToolbarButton.fontSize = 15;
        mainToolbarButton.fixedHeight = 25;
        mainToolbarButton.normal.textColor = new Color(1f, .7f, 0);
        mainToolbarButton.active.textColor = Color.cyan;
        mainToolbarButton.onNormal.textColor = Color.cyan;





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

        hoverableButton = new GUIStyle(EditorStyles.miniButton);
        hoverableButton.normal.textColor = fieldColor.normal.textColor;
        hoverableButton.hover.textColor = new Color(0, 1, .7f);
        hoverableButton.focused.textColor = new Color(0, 1, .9f);
        hoverableButton.active.textColor = new Color(0, 1, .9f);
        hoverableButton.font = (Font) Resources.Load("Fonts/Retron2000");
        hoverableButton.fixedHeight = 20;

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
        var t = sounds[i].spatialBlend;
        DrawBoolLabel(ref sounds[i].spatialBlend, "Use 3D Sound", buttonNameToggle,
            new GUILayoutOption[] {GUILayout.MinWidth(50), GUILayout.MinHeight(25)});
        if(t != sounds[i].spatialBlend) sounds[i].eventHolderList = new List<AudioManager.EventHolder>();
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
                        toolbarButton,GUILayout.MinWidth(0));
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
            GUILayout.Space(10);
            DrawBoolLabel(ref sounds[i].mixersVisibleInInspector,"...",toolbarButton,new GUILayoutOption[] {GUILayout.Width(30)});

        }

        

        if (sounds[i].mixersVisibleInInspector)
        {
            DrawMixerManager();
        }
        
    }

    private void DrawMixerManager()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            using (new EditorGUILayout.HorizontalScope("HelpBox"))
            {
                if (GUILayout.Button("Add Mixer", hoverableButton))
                {
                    AddMixer();
                }

                if (GUILayout.Button("Mixers...", hoverableButton, GUILayout.MaxWidth(70)))
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

        

        bool anyOpen = false;
        foreach (var sound in sounds)
        {
            if (sound.soundVisibleInInspector) anyOpen = true;
        }
        
        if(!anyOpen)
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
            if(!anyOpen || sounds[i].soundVisibleInInspector) 
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
                sounds[i].eventHolderList.Add(new AudioManager.EventHolder(sounds[i].spatialBlend));
                Debug.Log(sounds[i].spatialBlend);
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
                        List<string> usableEvents = new List<string>();

                        for (int j = 0; j < eventInfos.Count; j++)
                        {
                            // Debug.Log(typeof(AudioManager.Sound).GetMethod("PlaySoundSubscribe").GetParameters() == eventInfos[j].EventHandlerType.GetMethod("Invoke").GetParameters());
                            // Debug.Log(eventInfos[j].EventHandlerType.GetMethod("Invoke").GetParameters().Length);

                            MethodInfo subscriberMethod = null;
                            
                            switch (sounds[i].spatialBlend)
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

                            if (EqualsList(parameterTypeArray.ToList(),eventTypeArray.ToList()))
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
                                GUILayout.Label("Event: ", fieldColor, GUILayout.MaxWidth(50));

                                int k = holder.selectedEvents;
                                // holder.selectedEvents = EditorGUILayout.MaskField(holder.selectedEvents, GetNamesFromEvents(eventInfos.ToArray()));
                                holder.selectedEvents = EditorGUILayout.MaskField(holder.selectedEvents, usableEvents.ToArray());
                                if (k != holder.selectedEvents) EditorUtility.SetDirty(target);
                            }
                        }
                        else
                        {
                            GUILayout.Label("No usable events available in that component!", fieldColor);
                        }
                    }
                }

                if (holder.eventer)
                    if (GUILayout.Button(" - ", buttonNameToggleBig))
                    {
                        sounds[i].eventHolderList.Remove(holder);
                    }
            }
        }
    }
    
    public static bool EqualsList<T>(List<T> a, List<T> b) {
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
        for (int i = 0; i < a.Count; i++) {
            if (!object.Equals(a[i], b[i])) {
                // Debug.Log("objects in position " + i +  " are different");

                return false;
            }
        }
        return true;
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