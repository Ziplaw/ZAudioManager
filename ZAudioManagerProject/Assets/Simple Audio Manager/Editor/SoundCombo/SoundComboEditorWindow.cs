using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using ExtensionMethods;
using UnityEditor.UIElements;
using ZAudioManagerUtils;

public class SoundComboEditorWindow : ExtendedEditorWindow
{
    public static SoundCombo comboManager;
    public SerializedProperty propSounds;
    public SerializedProperty propMixers;

    public static void Open(SoundCombo combo)
    {
        SoundComboEditorWindow.comboManager = combo;
        SoundComboEditorWindow window = GetWindow<SoundComboEditorWindow>("Sound Combo Maker");
        window.serializedObject = new SerializedObject(combo);
    }

    private bool init;

    void Init()
    {
        propSounds = serializedObject.FindProperty("sounds");
        propMixers = serializedObject.FindProperty("mixers");

        // Debug.Log(serializedObject);
        // Debug.Log(propSounds);
        // Debug.Log(propMixers);

        init = true;
    }
    
    Vector2 scrollPos1;
    Vector2 scrollPos2;

    private void OnGUI()
    {
        if (!init) Init();


        if (comboManager == null)
        {
            Close();
            return;
        }

        using (new GUILayout.HorizontalScope())
        {

            using (new GUILayout.VerticalScope(GUILayout.Width(300)))
            {
                using (var scrollView =
                    new EditorGUILayout.ScrollViewScope(scrollPos1))
                {
                    scrollPos1 = scrollView.scrollPosition;


                    if (GUILayout.Button("Add Sound"))
                    {
                        comboManager.sounds.Add(new AudioManager.Sound(null, null, false, 128, 1, 1, false,
                            new AudioManager.DimensionalSoundSettings(1, 500)));
                        serializedObject.Update();
                    }

                    for (var i = 0; i < comboManager.sounds.Count; i++)
                    {
                        using (new GUILayout.VerticalScope("helpBox"))
                        {

                            AudioManagerEditor.DrawSound(comboManager.sounds, propSounds, i, true, this,
                                serializedObject,
                                propMixers);
                        }

                        if (i != comboManager.sounds.Count - 1) GUILayout.Space(5);

                        // if (!comboManager.combo[i].clip)
                        // {
                        //     EditorGUILayout.PropertyField(
                        //         serializedObject.FindProperty("combo").GetArrayElementAtIndex(i).FindPropertyRelative("clip"),
                        //         GUIContent.none, GUILayout.Height(60));
                        // }
                        // else
                        // {
                        //     DrawSound(comboManager.combo[i], i);
                        // }
                    }

                    serializedObject.ApplyModifiedProperties();
                }
            }

            using (new GUILayout.VerticalScope("box", GUILayout.MinHeight(500)))
            {
                using (var scrollView =
                    new EditorGUILayout.ScrollViewScope(scrollPos1))
                {
                    scrollPos1 = scrollView.scrollPosition;
                    for (int i = 0; i < comboManager.sounds.Count; i++)
                    {
                        DrawSoundWave(comboManager.sounds[i], i, 500);
                    }
                }
            }
        }
    }

    private void DrawSoundWave(AudioManager.Sound clip, int i, int width)
    {
        if(!clip.wave) clip.wave = EditorFunctions.PaintWaveformSpectrum(clip.clip, 1f, width, 60, new Color(1f, .7f, 0));

        Texture image = clip.wave;
        
        using (new GUILayout.VerticalScope(image,new GUIStyle("box"),GUILayout.Height(60),GUILayout.MinWidth(500)))
        {
            GUILayout.Label(clip.soundName);
            GUILayout.Toggle(true,GUIContent.none);
        }
        
        // GUILayout.Box(clip.wave);
        // clip.wave = EditorFunctions.PaintWaveformSpectrum(clip.clip, 1f,
        //     width,
        //     60, new Color(1f, .7f, 0));
    }

    public void PreviewCombo(AudioManager.Sound[] sounds)
    {
        for (var i = 0; i < sounds.Length; i++)
        {
            int spatialBlend = sounds[i].spatialBlend ? 1 : 0;

            sounds[i].Previewer.clip = sounds[i].clip;
            sounds[i].Previewer.outputAudioMixerGroup = sounds[i].mixer;
            sounds[i].Previewer.loop = sounds[i].loop;
            sounds[i].Previewer.volume = sounds[i].volumeIsRange
                ? UnityEngine.Random.Range(sounds[i].volumeRange.x, sounds[i].volumeRange.y)
                : sounds[i].volume;
            sounds[i].Previewer.pitch = sounds[i].pitchIsRange
                ? UnityEngine.Random.Range(sounds[i].pitchRange.x, sounds[i].pitchRange.y)
                : sounds[i].pitch;
            sounds[i].Previewer.priority = sounds[i].priorityIsRange
                ? (int) UnityEngine.Random.Range(sounds[i].priorityRange.x, sounds[i].priorityRange.y)
                : sounds[i].priority;
            sounds[i].Previewer.minDistance = sounds[i].settings.minDistance;
            sounds[i].Previewer.maxDistance = sounds[i].settings.maxDistance;


            sounds[i].Previewer.Play();
        }
    }
}