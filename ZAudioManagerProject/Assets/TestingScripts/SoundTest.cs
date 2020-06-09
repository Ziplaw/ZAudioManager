using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
	public string soundName;
	// Start is called before the first frame update
	void Start()
	{
		AudioManager.Play(soundName, transform.position);
	}
}
