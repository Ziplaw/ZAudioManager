using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using System;
using NUnit.Framework.Constraints;

[ExecuteInEditMode]
[CustomEditor(typeof(AudioManager)), CanEditMultipleObjects]
public class AudioManagerEditor : Editor
{
	GUIStyle removeButtonStyle, moveButtonStyle;
	AudioManager manager;
	List<AudioSource> previewers = new List<AudioSource>();
	SerializedProperty propMixers;
	SerializedProperty propsounds;


	private List<AudioManager.Sound> sounds => manager.sounds;

	void OnEnable()
	{
		
		manager = (AudioManager)target;
		manager.transform.hideFlags = HideFlags.HideInInspector;

		UpdateMixerList();
		UpdateSoundList();
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
			GUILayout.Label(label, style, GUILayout.MaxWidth(120));
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
	void DrawSoundPropertyAt(SerializedProperty pList, string propertyPath, int i, float maxWidth)
	{
		SerializedProperty tempProp = pList.GetArrayElementAtIndex(i).FindPropertyRelative(propertyPath);

		using (new EditorGUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(tempProp, GUIContent.none, GUILayout.MaxWidth(maxWidth));
		}
		serializedObject.ApplyModifiedProperties();
	}


	public override void OnInspectorGUI()
	{
		// serializedObject.Update();
		GUIStyle fieldColor = new GUIStyle(GUI.skin.label);
		fieldColor.normal.textColor = new Color(1, .7f, 0);
		
		GUIStyle nullFieldColor = new GUIStyle(GUI.skin.label);
		nullFieldColor.normal.textColor = Color.red;

		GUIStyle min = new GUIStyle(GUI.skin.label);
		GUIStyle max = new GUIStyle(GUI.skin.label);
		min.normal.textColor = new Color(.5f, .5f, 1);
		max.normal.textColor = new Color(.5f, 1, .5f);

		removeButtonStyle = new GUIStyle(GUI.skin.button);
		removeButtonStyle.normal.textColor = new Color(1, .5f, .5f);

		moveButtonStyle = new GUIStyle(GUI.skin.button);
		moveButtonStyle.normal.textColor = new Color(.5f, .5f, 1);

		GUIStyle hoverableButton = new GUIStyle(EditorStyles.toolbarButton);
		hoverableButton.normal.textColor = fieldColor.normal.textColor;
		hoverableButton.hover.textColor = new Color(0, 1, .7f);
		hoverableButton.focused.textColor = new Color(0, 1, .9f);
		hoverableButton.active.textColor = new Color(0, 1, .9f);
		
		GUIStyle nullHoverableButton = new GUIStyle(EditorStyles.toolbarButton);
		nullHoverableButton.normal.textColor = Color.red;
		nullHoverableButton.hover.textColor = new Color(0, 1, .7f);
		nullHoverableButton.focused.textColor = new Color(0, 1, .9f);
		nullHoverableButton.active.textColor = new Color(0, 1, .9f);
		
		

		// base.DrawDefaultInspector();

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
					System.Type windowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AudioMixerWindow");
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

					sounds[i].soundVisibleInInspector = GUILayout.Toggle(sounds[i].soundVisibleInInspector, sounds[i].soundName, hoverableButton);

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
							DrawSoundPropertyAt(propsounds, "soundName", i, 100);

							if (!sounds[i].soundTesting)
							{
								sounds[i].soundTesting = GUILayout.Toggle(sounds[i].soundTesting, "Preview", EditorStyles.miniButton, GUILayout.MaxWidth(100));
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
								sounds[i].soundTesting = GUILayout.Toggle(sounds[i].soundTesting, "■", EditorStyles.miniButton, GUILayout.MaxWidth(50));
							}

							DrawSoundPropertyAt(propsounds, "previewColor", i, 50);
							DrawSoundPropertyAt(propsounds, "isVeryImportant", i, 15);
							

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
						using (new EditorGUILayout.HorizontalScope("HelpBox"))
						{
							if (manager.mixers.Count > 0)
							{
								GUILayout.Label("Mixers:");



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
									// NEEDS FIXING, DOESN'T STICK THROUGH PLAYS
									var psm = sounds[i].selectedMixer;

									sounds[i].selectedMixer = GUILayout.Toolbar(sounds[i].selectedMixer, names);
									sounds[i].mixer = manager.mixers[sounds[i].selectedMixer];

									if (psm != sounds[i].selectedMixer)
									{
										EditorUtility.SetDirty(target);
									}
								}
							}
						}

						DrawSoundPropertyAt(propsounds, "volume", "Volume", fieldColor, i);
						DrawSoundPropertyAt(propsounds, "priority", "Priority", fieldColor, i);
						DrawSoundPropertyAt(propsounds, "pitch", "Pitch", fieldColor, i);
						DrawSoundPropertyAt(propsounds, "loop", "Loop", fieldColor, i);
						DrawSoundPropertyAt(propsounds, "spatialBlend", "3D Sound", fieldColor, i);
						if (sounds[i].spatialBlend)
						{
							DrawSoundPropertyAt(propsounds, "settings.minDistance", "Minimum Distance", min, i);
							DrawSoundPropertyAt(propsounds, "settings.maxDistance", "Maximum Distance", max, i);
						}

						using (new GUILayout.HorizontalScope())
						{
							DrawSoundPropertyAt(propsounds, "soundPreviewer", "Sound Previewer", fieldColor, i);
							DrawSoundPropertyAt(propsounds, "soundVisibleInScene", i, 15);
						}
					}
				}
			}
			
			serializedObject.ApplyModifiedProperties();
		}
	}

	private void AddMixer()
	{
		manager.mixers.Add(manager.mixers[0]);
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
		sounds.Add(new AudioManager.Sound(null, null, false, 128, 1, 1, false, new AudioManager.DimensionalSoundSettings()));
		UpdateSoundList();
	}
	void RemoveSoundAt(int i)
	{
		sounds.RemoveAt(i);
		UpdateSoundList();
	}
	void PreviewSound(AudioManager.Sound sound, int i)
	{
		previewers[i] = EditorUtility.CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();

		int spatialBlend = sound.spatialBlend ? 1 : 0;

		previewers[i].clip = sound.clip;
		previewers[i].outputAudioMixerGroup = sound.mixer;
		previewers[i].loop = sound.loop;
		previewers[i].volume = sound.volume;
		previewers[i].pitch = sound.pitch;
		previewers[i].priority = sound.priority;
		previewers[i].spatialBlend = spatialBlend;
		previewers[i].minDistance = sound.settings.minDistance;
		previewers[i].maxDistance = sound.settings.maxDistance;

		previewers[i].Play();

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



