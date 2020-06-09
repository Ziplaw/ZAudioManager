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
	List<SerializedProperty> soundPreviewers = new List<SerializedProperty>();
	List<AudioSource> previewers = new List<AudioSource>();

	bool initialized;

	void OnEnable()
	{

		manager = (AudioManager)target;
		manager.transform.hideFlags = HideFlags.HideInInspector;
		// previewers.Clear();

		foreach (AudioManager.Sound s in manager.sounds)
		{
			previewers.Add(null);
		}
		for (int i = 0; i < manager.sounds.Count; i++)
		{
			if (!manager.sounds[i].soundPreviewer) manager.sounds[i].soundPreviewer = manager.transform;


			SerializedProperty p = serializedObject.FindProperty("sounds").GetArrayElementAtIndex(i).FindPropertyRelative("soundPreviewer");

			soundPreviewers.Add(p);
		}
	}
	public override void OnInspectorGUI()
	{


		removeButtonStyle = new GUIStyle(GUI.skin.button);
		removeButtonStyle.normal.textColor = new Color(1, .5f, .5f);

		moveButtonStyle = new GUIStyle(GUI.skin.button);
		moveButtonStyle.normal.textColor = new Color(.5f, .5f, 1);


		using (new EditorGUILayout.VerticalScope("HelpBox"))
		{
			Undo.RecordObject(target, "Sound Manager Mixer Change");

			using (new EditorGUILayout.HorizontalScope("HelpBox"))
			{
				if (GUILayout.Button("Add Mixer"))
				{
					manager.mixers.Add(null);
				}

				if (GUILayout.Button("Mixers...", GUILayout.MaxWidth(60)))
				{
					System.Type windowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AudioMixerWindow");

					EditorWindow window = EditorWindow.GetWindow(windowType);
				}

			}

			for (int i = 0; i < manager.mixers.Count; i++)
			{
				using (new EditorGUILayout.HorizontalScope("HelpBox"))
				{
					manager.mixers[i] = (AudioMixerGroup)EditorGUILayout.ObjectField(manager.mixers[i], typeof(AudioMixerGroup), true);
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
			Undo.RecordObject(target, "Add Sound");

			if (GUILayout.Button("Add Sound"))
			{
				AddSound();
			}

			Undo.RecordObject(target, "Display All Sounds");

			if (GUILayout.Button("V", moveButtonStyle, GUILayout.MaxWidth(30)))
			{
				ToggleAllButtons();
			}

		}

		Undo.RecordObject(target, "Sound Manager Clip Change");


		for (int i = 0; i < manager.sounds.Count; i++)
		{
			using (new EditorGUILayout.VerticalScope("HelpBox"))
			{
				using (new EditorGUILayout.HorizontalScope("HelpBox"))
				{
					manager.soundOpen[i] = GUILayout.Toggle(manager.soundOpen[i], manager.sounds[i].soundName, EditorStyles.toolbarButton);
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
					}
				}

				if (manager.soundOpen.Count > i && manager.soundOpen[i])
				{



					using (new EditorGUILayout.HorizontalScope("HelpBox"))
					{
						AudioClip prevClip = manager.sounds[i].clip;

						manager.sounds[i].clip = (AudioClip)EditorGUILayout.ObjectField(manager.sounds[i].clip, typeof(AudioClip), true);

						if (manager.sounds[i].clip != prevClip)
						{
							manager.sounds[i].soundName = manager.sounds[i].clip.name;
						}

						using (new EditorGUI.DisabledGroupScope(!manager.sounds[i].clip))
						{
							manager.sounds[i].soundName = GUILayout.TextField(manager.sounds[i].soundName);
							this.Repaint();


							if (!manager.soundTesting[i])
							{
								manager.soundTesting[i] = GUILayout.Toggle(manager.soundTesting[i], "Preview", EditorStyles.miniButton, GUILayout.MaxWidth(100));
							}
							else
							{
								if (previewers[i] != null && previewers[i].isPlaying)
								{
									if (GUILayout.Button("❚❚", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
									{
										previewers[i].Pause();
										manager.paused[i] = true;
									}
								}
								if (previewers[i] != null && !previewers[i].isPlaying)
								{
									if (GUILayout.Button("►", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
									{
										previewers[i].UnPause();
										manager.paused[i] = false;

									}
								}
								manager.soundTesting[i] = GUILayout.Toggle(manager.soundTesting[i], "■", EditorStyles.miniButton, GUILayout.MaxWidth(50));
							}

							if (manager.soundTesting[i])
							{
								if (previewers[i] == null)
								{
									PreviewSound(manager.sounds[i], i);
								}
								else if (!previewers[i].isPlaying && !manager.paused[i])
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
								// Debug.Log(i);
								manager.chosenMixers[i] = GUILayout.Toolbar(manager.chosenMixers[i], names);
								manager.sounds[i].mixer = manager.mixers[manager.chosenMixers[i]];
							}
						}
					}

					GUIStyle fieldColor = new GUIStyle(GUI.skin.label);
					fieldColor.normal.textColor = new Color(1, .7f, 0);


					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Label("Volume", fieldColor, GUILayout.MaxWidth(80));
						manager.sounds[i].volume = EditorGUILayout.Slider(manager.sounds[i].volume, 0, 1);
					}
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Label("Priority", fieldColor, GUILayout.MaxWidth(80));
						manager.sounds[i].priority = EditorGUILayout.IntSlider(manager.sounds[i].priority, 0, 256);
					}
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Label("Pitch", fieldColor, GUILayout.MaxWidth(80));
						manager.sounds[i].pitch = EditorGUILayout.Slider(manager.sounds[i].pitch, 0, 3);
					}
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Label("Loop Sound", fieldColor, GUILayout.MaxWidth(80));
						manager.sounds[i].loop = EditorGUILayout.Toggle(manager.sounds[i].loop);
					}
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Label("3D Sound", fieldColor, GUILayout.MaxWidth(80));
						manager.sounds[i].spatialBlend = EditorGUILayout.Toggle(manager.sounds[i].spatialBlend);
					}


					if (manager.sounds[i].spatialBlend)
					{
						Undo.RecordObject(target, "3D Settings Changed");

						GUIStyle min = new GUIStyle(GUI.skin.label);
						GUIStyle max = new GUIStyle(GUI.skin.label);
						min.normal.textColor = new Color(.5f, .5f, 1);
						max.normal.textColor = new Color(.5f, 1, .5f);

						using (new EditorGUILayout.HorizontalScope())
						{
							GUILayout.Label("Minimum Distance", min);
							manager.sounds[i].settings.minDistance = EditorGUILayout.Slider(manager.sounds[i].settings.minDistance, 0.01f, 500);
						}
						using (new EditorGUILayout.HorizontalScope())
						{

							GUILayout.Label("Maximum Distance", max);
							manager.sounds[i].settings.maxDistance = EditorGUILayout.Slider(manager.sounds[i].settings.maxDistance, 0.01f, 500);
						}

						EditorGUILayout.PropertyField(soundPreviewers[i], true);


					}
				}
			}
			serializedObject.ApplyModifiedProperties();
		}
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
					Handles.color = new Color(.5f, 1, .5f);
					Handles.DrawWireDisc(manager.sounds[i].soundPreviewer.position, new Vector3(0, 1, 0), manager.sounds[i].settings.maxDistance);
					Handles.color = new Color(.5f, 1, .5f, .05f);
					Handles.SphereHandleCap(0, manager.sounds[i].soundPreviewer.position, Quaternion.identity, manager.sounds[i].settings.maxDistance * 2, EventType.Repaint);

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
		manager.chosenMixers.Add(0);
		manager.soundOpen.Add(true);
		manager.soundTesting.Add(true);
		manager.paused.Add(false);
		previewers.Add(null);
		soundPreviewers.Add(serializedObject.FindProperty("soundPreviewer"));
	}
	void RemoveSoundAt(int i)
	{
		manager.sounds.RemoveAt(i);
		manager.chosenMixers.RemoveAt(i);
		manager.soundOpen.RemoveAt(i);
		manager.soundTesting.RemoveAt(i);
		manager.paused.RemoveAt(i);
		previewers.RemoveAt(i);
		soundPreviewers.RemoveAt(i);
	}
	void PreviewSound(AudioManager.Sound sound, int i)
	{
		previewers[i] = EditorUtility.CreateGameObjectWithHideFlags("AudioPreview", HideFlags.DontSaveInEditor, typeof(AudioSource)).GetComponent<AudioSource>();

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
			manager.soundTesting[i] = false;
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


		for (int j = 0; j < manager.sounds.Count; j++)
		{
			if (manager.chosenMixers[j] == i)
			{
				manager.chosenMixers[j] = 0;

			}
			if (manager.chosenMixers[j] > i)
			{
				manager.chosenMixers[j]--;
			}
		}


	}
	void ToggleAllButtons()
	{
		bool hasToggled = false;
		bool hasUnToggled = false;

		for (int i = 0; i < manager.sounds.Count; i++)
		{
			if (manager.soundOpen[i] == false)
			{
				hasUnToggled = true;
			}

			if (manager.soundOpen[i] == true)
			{
				hasToggled = true;
			}
		}

		if (hasToggled && hasUnToggled)
		{
			for (int i = 0; i < manager.sounds.Count; i++)
			{
				manager.soundOpen[i] = true;
			}
		}

		if (hasUnToggled && !hasToggled)
		{
			for (int i = 0; i < manager.sounds.Count; i++)
			{
				manager.soundOpen[i] = true;
			}
		}

		if (!hasUnToggled && hasToggled)
		{
			for (int i = 0; i < manager.sounds.Count; i++)
			{
				manager.soundOpen[i] = false;
			}
		}
	}
}



