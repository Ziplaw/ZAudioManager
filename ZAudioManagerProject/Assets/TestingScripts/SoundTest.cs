using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
	public string soundName;
	// Start is called before the first frame update
	void Start()
	{
		// AudioManager.Play(soundName, transform.position, transform);
		testEvent?.Invoke();
		testEvent2?.Invoke();
		testEvent3?.Invoke();

		Debug.Log(testEvent.GetInvocationList());
	}

	public void TestMethod() => Debug.Log("Test Method Played!");
	public void TestMethod2(int i) => Debug.Log("Test Method 2 Played! : " + i);

	
	public event Action  testEvent;
	public event Action  testEvent2;
	public event Action  testEvent3;

}
