using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour
{
	public string message = "Hello World!";
	private void Start() {
		Debug.Log(message);
	}
}
