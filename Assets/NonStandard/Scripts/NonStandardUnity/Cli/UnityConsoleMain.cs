using NonStandard.Inputs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityConsoleMain : MonoBehaviour
{
	public bool activeOnStart = false; // TODO
	[System.Serializable] public class KeyUsedTo { public KCode Activate = KCode.BackQuote, Deactivate = KCode.Escape; }
	public KeyUsedTo keyUsedTo = new KeyUsedTo(); // TODO
	[System.Serializable]
	public class Callbacks {
		public bool enable = true;
		public UnityEvent WhenThisActivates, WhenThisDeactivates;
	}
	public Callbacks callbacks = new Callbacks(); // TODO

	public void FullScreen(bool setToFullScreen = true) {
		// generate the full screen UI
	}
	private void Start() {
		if (activeOnStart) { FullScreen(); }
	}
	private void Update() {
		
	}
}
