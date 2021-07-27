using NonStandard.Cli;
using NonStandard.Inputs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// TODO Reset shouild generate Activate and Deactivate key bindings
public class UnityConsoleUiToggle : UserInput
{
	public bool isMain = true; // TODO
	public bool activeOnStart = false; // TODO
	public bool selectableAsMain = true; // TODO
	[System.Serializable] public class KeyUsedTo { public KCode Activate = KCode.BackQuote, Deactivate = KCode.Escape; }
	public KeyUsedTo keyUsedTo = new KeyUsedTo(); // TODO
	[System.Serializable]
	public class Callbacks {
		public bool enable = true;
		public UnityEvent WhenThisActivates, WhenThisDeactivates;
	}
	public Callbacks callbacks = new Callbacks(); // TODO
	RectTransform uiTransform;
	public Canvas _screenSpaceCanvas;
	Canvas _originalCanvas;
	public Canvas ScreenSpaceCanvas => _screenSpaceCanvas ? _screenSpaceCanvas : _screenSpaceCanvas = GetScreenSpaceCanvas(null);
	public void SetScreenSpaceCanvas() {
		// generate the full screen UI
		Canvas c = ScreenSpaceCanvas;
		uiTransform.SetParent(c.transform, false);
	}
	public void SetWorldSpaceCanvas() {
		Canvas c = _originalCanvas;
		uiTransform.SetParent(c.transform, false);
	}
	private void Awake() {
		uiTransform = GetComponent<RectTransform>();
		_originalCanvas = GetComponentInParent<Canvas>();
	}
	private void Start() {
		if (activeOnStart) { SetScreenSpaceCanvas(); }
	}
	private void Update() {
		
	}

	public static Canvas GetScreenSpaceCanvas(Transform self, string canvasObjectNameOnCreate = "<console screen canvas>") {
		Canvas canvas;// = self.GetComponentInParent<Canvas>();
		//if (!canvas) {
			canvas = (new GameObject(canvasObjectNameOnCreate)).AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			if (!canvas.GetComponent<UnityEngine.UI.CanvasScaler>()) {
				canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>(); // so that text is pretty when zoomed
			}
			if (!canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>()) {
				canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>(); // so that mouse can select input area
			}
			canvas.transform.SetParent(self);
		//}
		return canvas;
	}
}
