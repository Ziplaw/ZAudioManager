using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZAudioManagerUtils
{
    public static class EditorFunctions
    {
        public static Texture2D PaintWaveformSpectrum(AudioClip audio, float saturation, int width,
            int height, Color col)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float[] samples = new float[audio.samples];
            float[] waveform = new float[width];
            audio.GetData(samples, 0);
            int packSize = (audio.samples / width) + 1;
            int s = 0;
            for (int i = 0; i < audio.samples; i += packSize)
            {
                waveform[s] = Mathf.Abs(samples[i]);
                s++;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }

            for (int x = 0; x < waveform.Length; x++)
            {
                for (int y = 0; y <= waveform[x] * ((float) height * .75f); y++)
                {
                    tex.SetPixel(x, (height / 2) + y, col);
                    tex.SetPixel(x, (height / 2) - y, col);
                }
            }

            tex.Apply();
            return tex;
        }
        
        public static void DrawSoundProperty(SerializedProperty p)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(p, GUIContent.none);
            }
        }

        public static void DrawSoundPropertyAt(SerializedProperty pList, string propertyPath, string label, GUIStyle style,
            int i)
        {
            SerializedProperty tempProp = pList.GetArrayElementAtIndex(i).FindPropertyRelative(propertyPath);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, style, new GUILayoutOption[] {GUILayout.Width(120)});
                EditorGUILayout.PropertyField(tempProp, GUIContent.none);
            }
        }

        public static void DrawSoundPropertyAt(SerializedProperty pList, string propertyPath, int i)
        {
            SerializedProperty tempProp = pList.GetArrayElementAtIndex(i).FindPropertyRelative(propertyPath);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(tempProp, GUIContent.none);
            }
        }

        public static void DrawSoundPropertyAt(SerializedProperty pList, string propertyPath, int i, GUILayoutOption[] options)
        {
            SerializedProperty tempProp = pList.GetArrayElementAtIndex(i).FindPropertyRelative(propertyPath);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(tempProp, GUIContent.none, options);
            }
        }

        public static void DrawBoolLabel(ref bool condition, string label, GUIStyle style, GUILayoutOption[] options,
            UnityEngine.Object manager)
        {
            bool t = condition;
            condition = GUILayout.Toggle(condition, label, style, options);
            if (t != condition) EditorUtility.SetDirty(manager);
        }

        public static void DrawMinMaxSlider(ref float rangeX, ref float rangeY, int minLimit, int maxLimit,
            SerializedProperty serializedProperty, string propertyPath, int i, GUILayoutOption[] sliderOptions, GUILayoutOption[] options)
        {
            EditorGUILayout.MinMaxSlider(ref rangeX, ref rangeY, minLimit, maxLimit, sliderOptions);
            DrawSoundPropertyAt(serializedProperty, propertyPath, i, options);
        }
    }

    public class Styles
    {
        public static GUIStyle removeButtonStyle
        {
            get
            {
                var s = new GUIStyle(GUI.skin.button);
                s.normal.textColor = new Color(1, .5f, .5f);
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                return s;
            }
        }

        public static GUIStyle moveButtonStyle
        {
            get
            {
                var s = new GUIStyle(GUI.skin.button);
                s.normal.textColor = new Color(.5f, .5f, 1);
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                return s;
            }
        }

        public static GUIStyle buttonNameToggle
        {
            get
            {
                var s = new GUIStyle(GUI.skin.button);
                s.normal.textColor = new Color(1, .7f, 0);
                s.active.textColor = Color.cyan;
                s.hover.textColor = Color.cyan;
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                s.onNormal.textColor = Color.cyan;
                return s;
            }
        }

        public static GUIStyle buttonNameToggleBig
        {
            get
            {
                var s = new GUIStyle(buttonNameToggle);
                s.fontSize = 30;
                return s;
            }
        }

        public static GUIStyle mainToolbarButton
        {
            get
            {
                var s = new GUIStyle(toolbarButton);
                s.fontSize = 15;
                s.fixedHeight = 25;
                s.normal.textColor = new Color(1f, .7f, 0);
                s.active.textColor = Color.cyan;
                s.onNormal.textColor = Color.cyan;
                return s;
            }
        }

        public static GUIStyle toolbarButton
        {
            get
            {
                var s = new GUIStyle(EditorStyles.objectFieldThumb);
                s.normal.textColor = new Color(1f, .7f, 0);
                s.active.textColor = new Color(0, 1f, .5f);
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                s.fontSize = 10;
                s.fixedHeight = 20;
                s.alignment = TextAnchor.MiddleCenter;
                s.onNormal.textColor = new Color(0, 1, .5f);
                s.clipping = TextClipping.Clip;
                return s;
            }
        }

        public static GUIStyle fieldColor
        {
            get
            {
                var s = new GUIStyle(GUI.skin.label);
                s.normal.textColor = new Color(1, .7f, 0);
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                return s;
            }
        }

        public static GUIStyle nullFieldColor
        {
            get
            {
                var s = new GUIStyle(GUI.skin.label);
                s.normal.textColor = Color.red;
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                return s;
            }
        }

        public static GUIStyle min
        {
            get
            {
                var s = new GUIStyle(GUI.skin.label);
                s.normal.textColor = new Color(.5f, .5f, 1);
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                return s;
            }
        }

        public static GUIStyle max
        {
            get
            {
                var s = new GUIStyle(GUI.skin.label);
                s.normal.textColor = new Color(.5f, 1, .5f);
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                return s;
            }
        }

        public static GUIStyle hoverableButton
        {
            get
            {
                var s = new GUIStyle(EditorStyles.miniButton);
                s.normal.textColor = fieldColor.normal.textColor;
                s.hover.textColor = new Color(0, 1, .7f);
                s.focused.textColor = new Color(0, 1, .9f);
                s.active.textColor = new Color(0, 1, .9f);
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                s.fixedHeight = 20;
                return s;
            }
        }

        public static GUIStyle nullHoverableButton
        {
            get
            {
                var s = new GUIStyle(EditorStyles.toolbarButton);
                s.normal.textColor = Color.red;
                s.hover.textColor = new Color(0, 1, .7f);
                s.focused.textColor = new Color(0, 1, .9f);
                s.active.textColor = new Color(0, 1, .9f);
                s.font = (Font) Resources.Load("Fonts/Retron2000");
                return s;
            }
        }
    }
}