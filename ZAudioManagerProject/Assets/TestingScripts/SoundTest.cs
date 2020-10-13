using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
	public string soundName;
	// Start is called before the first frame update
	IEnumerator Start()
	{
		// AudioManager.Play(soundName, transform.position, transform);
		testEvent?.Invoke(1);
		testEvent2?.Invoke(this, null, new Vector3());
		testEvent3?.Invoke(this, new Vector3(), null);
		testEvent4?.Invoke(this);
		yield return new WaitForSeconds(1);
		testEvent5?.Invoke(this);
		testEvent6?.Invoke();
		testEvent7?.Invoke();
		testEvent8?.Invoke();

		// Debug.Log(testEvent.GetInvocationList());
		
		yield return new WaitForSeconds(1);
		
		var a = AudioManager.GetAudioSource(this,"s1");
		// a[0].transform.position += Vector3.one * 50;

	}

	private void Update()
	{
		if(Input.GetMouseButtonDown(0)) testEvent4?.Invoke(this);

		
		// AudioManager.Play(this,"s1");
	}

	public void TestMethod() => Debug.Log("Test Method Played!");
	public void TestMethod2(int i) => Debug.Log("Test Method 2 Played! : " + i);
	
	
	public event Action<int>  testEvent;
	public event Action<object, Transform, Vector3>  testEvent2;
	public event Action<object,Vector3,Transform>  testEvent3;
	public event Action<object>  testEvent4;
	public event Action<object>  testEvent5;
	public event Action  testEvent6;
	public event Action  testEvent7;
	public event Action  testEvent8;
}
