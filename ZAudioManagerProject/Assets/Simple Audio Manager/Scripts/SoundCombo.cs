using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Combo",menuName = "ZAudioManager/Sound Combo", order = 1)]
public class SoundCombo : ScriptableObject
{
    public List<AudioManager.Sound> sounds = new List<AudioManager.Sound>();
    public List<AudioManager.Sound> combo = new List<AudioManager.Sound>();
    public AudioManager audioManager;
}
