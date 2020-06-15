using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using System;

[ExecuteInEditMode]
[CustomEditor(typeof(AudioManager)), CanEditMultipleObjects]
public class AudioManagerEditor : Editor
{
	GUIStyle removeButtonStyle, moveButtonStyle;
	AudioManager manager;
	List<AudioSource> previewers = new List<AudioSource>();
	SerializedProperty propMixers;
	SerializedProperty propSounds;
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
		propSounds = serializedObject.FindProperty("sounds");
		foreach (var s in manager.sounds)
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
	void DrawSoundPropertyAt(SerializedProperty pList, string propertyPath, string label, GUIStyle style, float maxWidth, int i)
	{
		SerializedProperty tempProp = pList.GetArrayElementAtIndex(i).FindPropertyRelative(propertyPath);

		using (new EditorGUILayout.HorizontalScope())
		{
			GUILayout.Label(label, style, GUILayout.MaxWidth(maxWidth));
			EditorGUILayout.PropertyField(tempProp, GUIContent.none);
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

		// Debug.Log(propMixers.Count);
		GUIStyle fieldColor = new GUIStyle(GUI.skin.label);
		fieldColor.normal.textColor = new Color(1, .7f, 0);

		GUIStyle min = new GUIStyle(GUI.skin.label);
		GUIStyle max = new GUIStyle(GUI.skin.label);
		min.normal.textColor = new Color(.5f, .5f, 1);
		max.normal.textColor = new Color(.5f, 1, .5f);

		removeButtonStyle = new GUIStyle(GUI.skin.button);
		removeButtonStyle.normal.textColor = new Color(1, .5f, .5f);

		moveButtonStyle = new GUIStyle(GUI.skin.button);
		moveButtonStyle.normal.textColor = new Color(.5f, .5f, 1);

		GUIStyle hoverableButton = EditorStyles.toolbarButton;
		hoverableButton.normal.textColor = fieldColor.normal.textColor;
		hoverableButton.hover.textColor = new Color(0, 1, .7f);
		hoverableButton.focused.textColor = new Color(0, 1, .9f);
		hoverableButton.active.textColor = new Color(0, 1, .9f);

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
					// manager.mixers[i] = (AudioMixerGroup)EditorGUILayout.ObjectField(manager.mixers[i], typeof(AudioMixerGroup), true);
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



		for (int i = 0; i < propSounds.arraySize; i++)
		{
			using (new EditorGUILayout.VerticalScope("HelpBox"))
			{
				using (new EditorGUILayout.HorizontalScope("HelpBox"))
				{
					// DrawSoundPropertyAt(propSounds, "soundVisibleInInspector", manager.sounds[i].soundName, fieldColor, 20, i);
					bool prevVisibilityState = manager.sounds[i].soundVisibleInInspector;

					manager.sounds[i].soundVisibleInInspector = GUILayout.Toggle(manager.sounds[i].soundVisibleInInspector, manager.sounds[i].soundName, hoverableButton);

					if (prevVisibilityState != manager.sounds[i].soundVisibleInInspector)
					{
						EditorUtility.SetDirty(target);
					}


					using (new EditorGUI.DisabledGroupScope(i == 0))
						if (GUILayout.Button("↑", moveButtonStyle, GUILayout.MaxWidth(30)))
						{
							MoveSoundUp(i);
						}
					using (new EditorGUI.DisabledGroupScope(i == manager.sounds.Count - 1))
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

				if (manager.sounds[i].soundVisibleInInspector)
				{
					using (new EditorGUILayout.HorizontalScope("HelpBox"))
					{
						AudioClip prevClip = manager.sounds[i].clip;
						DrawSoundPropertyAt(propSounds, "clip", i);


						if (manager.sounds[i].clip != prevClip)
						{
							manager.sounds[i].soundName = manager.sounds[i].clip.name;
						}

						using (new EditorGUI.DisabledGroupScope(!manager.sounds[i].clip))
						{
							DrawSoundPropertyAt(propSounds, "soundName", i, 100);

							if (!manager.sounds[i].soundTesting)
							{
								manager.sounds[i].soundTesting = GUILayout.Toggle(manager.sounds[i].soundTesting, "Preview", EditorStyles.miniButton, GUILayout.MaxWidth(100));
							}
							else
							{
								if (previewers[i] != null && previewers[i].isPlaying)
								{
									if (GUILayout.Button("❚❚", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
									{
										previewers[i].Pause();
										manager.sounds[i].paused = true;
									}
								}
								if (previewers[i] != null && !previewers[i].isPlaying)
								{
									if (GUILayout.Button("►", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
									{
										previewers[i].UnPause();
										manager.sounds[i].paused = false;

									}
								}
								manager.sounds[i].soundTesting = GUILayout.Toggle(manager.sounds[i].soundTesting, "■", EditorStyles.miniButton, GUILayout.MaxWidth(50));
							}

							DrawSoundPropertyAt(propSounds, "previewColor", i, 50);

							if (manager.sounds[i].soundTesting)
							{
								if (previewers[i] == null)
								{
									PreviewSound(manager.sounds[i], i);
								}
								else if (!previewers[i].isPlaying && !manager.sounds[i].paused)
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
								var psm = manager.sounds[i].selectedMixer;

								manager.sounds[i].selectedMixer = GUILayout.Toolbar(manager.sounds[i].selectedMixer, names);
								manager.sounds[i].mixer = manager.mixers[manager.sounds[i].selectedMixer];

								if (psm != manager.sounds[i].selectedMixer)
								{
									EditorUtility.SetDirty(target);
								}
							}
						}
					}

					DrawSoundPropertyAt(propSounds, "volume", "Volume", fieldColor, i);
					DrawSoundPropertyAt(propSounds, "priority", "Priority", fieldColor, i);
					DrawSoundPropertyAt(propSounds, "pitch", "Pitch", fieldColor, i);
					DrawSoundPropertyAt(propSounds, "loop", "Loop", fieldColor, i);
					DrawSoundPropertyAt(propSounds, "spatialBlend", "3D Sound", fieldColor, i);
					if (manager.sounds[i].spatialBlend)
					{
						DrawSoundPropertyAt(propSounds, "settings.minDistance", "Minimum Distance", min, i);
						DrawSoundPropertyAt(propSounds, "settings.maxDistance", "Maximum Distance", max, i);
					}
					DrawSoundPropertyAt(propSounds, "soundPreviewer", "Sound Previewer", fieldColor, i);

				}
			}
			serializedObject.ApplyModifiedProperties();
			// serializedObject.Update();
		}
	}

	private void AddMixer()
	{
		manager.mixers.Add(manager.mixers[0]);
		serializedObject.ApplyModifiedProperties();
	}

	[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
	void DrawSoundSpheres()
	{
		for (int i = 0; i < manager.sounds.Count; i++)
		{
			if (Event.current.type == EventType.Repaint)
				if (manager.sounds[i].spatialBlend && manager.sounds[i].soundPreviewer)
				{
					Handles.color = new Color(.5f, .5f, 1);
					Handles.DrawWireDisc(manager.sounds[i].soundPreviewer.position, new Vector3(0, 1, 0), manager.sounds[i].settings.minDistance);
					Handles.color = new Color(.5f, .5f, 1, .05f);
					Handles.SphereHandleCap(0, manager.sounds[i].soundPreviewer.position, Quaternion.identity, manager.sounds[i].settings.minDistance * 2, EventType.Repaint);
					Handles.color = new Color(manager.sounds[i].previewColor.r, manager.sounds[i].previewColor.g, manager.sounds[i].previewColor.b, 1);

					Handles.DrawWireDisc(manager.sounds[i].soundPreviewer.position, new Vector3(0, 1, 0), manager.sounds[i].settings.maxDistance);
					Handles.color = new Color(manager.sounds[i].previewColor.r, manager.sounds[i].previewColor.g, manager.sounds[i].previewColor.b, .05f);
					Handles.SphereHandleCap(0, manager.sounds[i].soundPreviewer.position, Quaternion.identity, manager.sounds[i].settings.maxDistance * 2, EventType.Repaint);

					Handles.DrawBezier(manager.transform.position, manager.sounds[i].soundPreviewer.transform.position, manager.transform.position + Vector3.down * Mathf.Abs(manager.transform.position.y - manager.sounds[i].soundPreviewer.transform.position.y), manager.sounds[i].soundPreviewer.transform.position + Vector3.up * Mathf.Abs(manager.transform.position.y - manager.sounds[i].soundPreviewer.transform.position.y), Color.cyan, Texture2D.whiteTexture, 1);

				}
		}
	}

	void OnSceneGUI()
	{
		DrawSoundSpheres();
	}
	private void AddSound()
	{
		manager.sounds.Add(new AudioManager.Sound(null, null, false, 128, 1, 1, false, new AudioManager.DimensionalSoundSettings()));
		UpdateSoundList();
	}
	void RemoveSoundAt(int i)
	{
		manager.sounds.RemoveAt(i);
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
			manager.sounds[i].soundTesting = false;
		}
	}
	void MoveSoundUp(int index) // 1
	{
		List<AudioManager.Sound> stemp = new List<AudioManager.Sound>(manager.sounds); //0, 1
		manager.sounds[index - 1] = manager.sounds[index]; // 1,1
		manager.sounds[index] = stemp[index - 1]; //1,0  
	}
	void MoveSoundDown(int index) // 0
	{
		List<AudioManager.Sound> stemp = new List<AudioManager.Sound>(manager.sounds); //0, 1
		manager.sounds[index] = manager.sounds[index + 1]; // 1,1
		manager.sounds[index + 1] = stemp[index]; //1,0
	}
	void RemoveMixer(int i)
	{
		manager.mixers.RemoveAt(i);
		// propMixers.RemoveAt(i);

		for (int j = 0; j < manager.sounds.Count; j++)
		{
			if (manager.sounds[j].selectedMixer == i)
			{
				manager.sounds[j].selectedMixer = 0;

			}
			if (manager.sounds[j].selectedMixer > i)
			{
				manager.sounds[j].selectedMixer--;
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
	void ToggleAllButtons()
	{
		bool hasToggled = false;
		bool hasUnToggled = false;

		for (int i = 0; i < manager.sounds.Count; i++)
		{
			if (manager.sounds[i].soundVisibleInInspector == false)
			{
				hasUnToggled = true;
			}

			if (manager.sounds[i].soundVisibleInInspector == true)
			{
				hasToggled = true;
			}
		}

		if (hasToggled && hasUnToggled)
		{
			for (int i = 0; i < manager.sounds.Count; i++)
			{
				manager.sounds[i].soundVisibleInInspector = true;
			}
		}

		if (hasUnToggled && !hasToggled)
		{
			for (int i = 0; i < manager.sounds.Count; i++)
			{
				manager.sounds[i].soundVisibleInInspector = true;
			}
		}

		if (!hasUnToggled && hasToggled)
		{
			for (int i = 0; i < manager.sounds.Count; i++)
			{
				manager.sounds[i].soundVisibleInInspector = false;
			}
		}
	}
}



