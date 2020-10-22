using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
        var rect = position;
        rect.height = 20;

        // Debug.Log(serializedObject);
        // Debug.Log(propSounds);
        // Debug.Log(propMixers);

        init = true;
    }

    Vector2 scrollPos1;
    Vector2 scrollPos2;
    private List<AudioManagerEditor.Previewer> previewers = new List<AudioManagerEditor.Previewer>();
    private int longestPreviewerIndex;

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

                    using (new GUILayout.VerticalScope())
                    {
                        var p = (from s in comboManager.sounds select s.pitch).ToArray();
                        var v = (from s in comboManager.sounds select s.volume).ToArray();

                        AudioManagerEditor.DrawSound(comboManager.sounds, propSounds, true, this,
                            serializedObject,
                            propMixers);

                        for (var i = 0; i < p.Length; i++)
                        {
                            if (p[i] != comboManager.sounds[i].pitch || v[i] != comboManager.sounds[i].volume)
                            {
                                comboManager.sounds[i].wave = EditorFunctions.PaintWaveformSpectrum(
                                    comboManager.sounds[i].clip, 1f,
                                    600, 60, new Color(1f, .7f, 0));
                                if(previewers.Count > 0)
                                    previewers[i].Sound = comboManager.sounds[i];
                            }
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }
            }

            using (new GUILayout.VerticalScope("box", GUILayout.Width(515), GUILayout.MinHeight(500)))
            {
                using (var scrollView =
                    new EditorGUILayout.ScrollViewScope(scrollPos2))
                {
                    scrollPos2 = scrollView.scrollPosition;
                    using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(500)))
                    {
                        if (previewers.Count == 0)
                        {
                            if (GUILayout.Button("Preview Combo",
                                new GUIStyle(Styles.hoverableButton)
                                    {fontSize = 20, fixedHeight = 35, fixedWidth = 500}))
                            {
                                PreviewCombo(comboManager.sounds);
                            }
                        }

                        else
                        {
                            Repaint();

                            

                            // Debug.Log(previewers[longestPreviewerIndex].progress);

                            bool anyPlaying = false;

                            for (var i = 0; i < previewers.Count; i++)
                            {
                                if (previewers[i].previewer.isPlaying) anyPlaying = true;
                            }

                            if (anyPlaying)
                            {
                                if (GUILayout.Button("❚❚",
                                    new GUIStyle(Styles.hoverableButton)
                                        {fontSize = 20, fixedHeight = 35, fixedWidth = 250}))
                                {
                                    previewers.ForEach((x) =>
                                    {
                                        x.previewer.Pause();
                                        x.Sound.paused = true;
                                    });
                                }
                            }
                            else
                            {
                                bool paused = false;

                                for (var i = 0; i < previewers.Count; i++)
                                {
                                    if (previewers[i].Sound.paused) paused = true;
                                }

                                if (paused)
                                {
                                    if (GUILayout.Button("►",
                                        new GUIStyle(Styles.hoverableButton)
                                            {fontSize = 20, fixedHeight = 35, fixedWidth = 250}))
                                    {
                                        previewers.ForEach((x) =>
                                        {
                                            x.previewer.UnPause();
                                            x.Sound.paused = false;
                                        });
                                    }
                                }
                                else
                                {
                                    previewers.ForEach((x) =>
                                    {
                                        DestroyImmediate(x.previewer.gameObject);

                                        x.Sound.paused = false;
                                    });
                                    previewers = new List<AudioManagerEditor.Previewer>();
                                }
                            }
                            if (GUILayout.Button("■",
                                new GUIStyle(Styles.hoverableButton)
                                    {fontSize = 20, fixedHeight = 35, fixedWidth = 250}))
                            {
                                previewers.ForEach((x) =>
                                {
                                    DestroyImmediate(x.previewer.gameObject);

                                    x.Sound.paused = false;
                                });
                                previewers = new List<AudioManagerEditor.Previewer>();
                            }
                        }
                    }

                    for (int i = 0; i < comboManager.sounds.Count; i++)
                    {
                        if (!comboManager.sounds[i].clip)
                        {
                            EditorGUILayout.PropertyField(
                                serializedObject.FindProperty("sounds").GetArrayElementAtIndex(i)
                                    .FindPropertyRelative("clip"),
                                GUIContent.none, GUILayout.Height(60));
                        }
                        else
                        {
                            DrawSoundWave(comboManager.sounds[i], i, 600);
                        }
                    }
                }
            }
        }
    }

    private void DrawSoundWave(AudioManager.Sound clip, int i, int width)
    {
        
        
        if (!clip.wave)
        {
            clip.wave = EditorFunctions.PaintWaveformSpectrum(clip.clip, 1f, width, 60, new Color(1f, .7f, 0));
        }

        Texture image = clip.wave;


        using (var scope = new GUILayout.VerticalScope(image, new GUIStyle("box") {alignment = TextAnchor.MiddleLeft},
            GUILayout.Height(60), GUILayout.Width(500)))
        {
            {
                // var rect = new Rect(300, 50, 515, position.height);
                // var content = new GUIContent( clip.wave);
                
                // var rect = GUILayoutUtility.GetRect(20,20, new GUIStyle("box"));
                // EditorGUI.DrawRect(rect, new Color(0,0,0,0.5f));

                if (previewers.Count > 0)
                {
                    Handles.color = new Color(1, .7f, 0);
                    UnityEditor.Handles.DrawAAPolyLine(new Vector3(5 + 500 * previewers[i].progress, 45 + 65 * i, 0),
                        new Vector3(5 + 500 * previewers[i].progress, 100 + 65 * i, 0));
                    Handles.color = Color.white;
                }
                // UnityEditor.Handles.DrawAAPolyLine(new Vector3(5, 110, 0), new Vector3(5, 165, 0));
                


                // float yMin = -1; 
                // float yMax = 1;
                // float step = 1 / position.width; 
                //
                // Vector3 prevPos = new Vector3(0, curveFunc(0), 0);
                // for (float t = step; t < 1; t += step) {
                //     Vector3 pos = new Vector3(t, curveFunc(t), 0);
                //     UnityEditor.Handles.DrawLine(
                //         new Vector3(rect.xMin + prevPos.x * rect.width, rect.yMax - ((prevPos.y - yMin) / (yMax - yMin)) * rect.height, 0), 
                //         new Vector3(rect.xMin + pos.x * rect.width, rect.yMax - ((pos.y - yMin) / (yMax - yMin)) * rect.height, 0));
                //
                //     prevPos = pos;
                // }
            }
            
            GUILayout.Label(clip.soundName);
            GUILayout.Toggle(true, GUIContent.none);
        }

        // GUILayout.Box(clip.wave);
        // clip.wave = EditorFunctions.PaintWaveformSpectrum(clip.clip, 1f,
        //     width,
        //     60, new Color(1f, .7f, 0));
    }
    
    float curveFunc(float t) {
        return Mathf.Sin(t * 2 * Mathf.PI);
    }

    public void PreviewCombo(List<AudioManager.Sound> sounds)
    {
        previewers = new List<AudioManagerEditor.Previewer>();

        foreach (var sound in sounds)
        {
            previewers.Add(AudioManagerEditor.PreviewSound(sound));
        }
        
        float p = 0;

        for (int i = 0; i < previewers.Count; i++)
        {
            // Debug.LogWarning(previewers[i].previewer.clip.length / previewers[i].Sound.pitch);

            
            if (previewers[i].previewer.clip.length / previewers[i].Sound.pitch > p)
            {
                p = previewers[i].previewer.clip.length / previewers[i].Sound.pitch;
                longestPreviewerIndex = i;
            }
        }
    }


    // for (var i = 0; i < sounds.Length; i++)
    // {
    //     int spatialBlend = sounds[i].spatialBlend ? 1 : 0;
    //
    //     sounds[i].Previewer.clip = sounds[i].clip;
    //     sounds[i].Previewer.outputAudioMixerGroup = sounds[i].mixer;
    //     sounds[i].Previewer.loop = sounds[i].loop;
    //     sounds[i].Previewer.volume = sounds[i].volumeIsRange
    //         ? UnityEngine.Random.Range(sounds[i].volumeRange.x, sounds[i].volumeRange.y)
    //         : sounds[i].volume;
    //     sounds[i].Previewer.pitch = sounds[i].pitchIsRange
    //         ? UnityEngine.Random.Range(sounds[i].pitchRange.x, sounds[i].pitchRange.y)
    //         : sounds[i].pitch;
    //     sounds[i].Previewer.priority = sounds[i].priorityIsRange
    //         ? (int) UnityEngine.Random.Range(sounds[i].priorityRange.x, sounds[i].priorityRange.y)
    //         : sounds[i].priority;
    //     sounds[i].Previewer.minDistance = sounds[i].settings.minDistance;
    //     sounds[i].Previewer.maxDistance = sounds[i].settings.maxDistance;
    //
    //
    //     sounds[i].Previewer.Play();
    // }
}