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
	[System.Serializable] public class Callbacks {
		public bool enable = true;
		public UnityEvent WhenThisActivates, WhenThisDeactivates;
	}
	public Callbacks callbacks = new Callbacks(); // TODO
	RectTransform uiTransform;
	public Canvas _screenSpaceCanvas;
	public Canvas _worldSpaceCanvas;
	private void Reset() {
		KeyBind(KCode.BackQuote, KModifier.None, "activate console", nameof(SetScreenSpaceCanvas), null, this);
		KeyBind(KCode.Escape, KModifier.None, "deactivate console", nameof(SetWorldSpaceCanvas), null, this);
	}
	public Canvas ScreenSpaceCanvas => _screenSpaceCanvas ? _screenSpaceCanvas : _screenSpaceCanvas = GetScreenSpaceCanvas(null);
	public void SetScreenSpaceCanvas() {
		// TODO also activate console input
		uiTransform.SetParent(ScreenSpaceCanvas.transform, false);
	}
	public void SetWorldSpaceCanvas() {
		// TODO also deactivate console input
		uiTransform.SetParent(_worldSpaceCanvas.transform, false);
	}
	private Canvas FindCanvas(RenderMode mode) {
		Canvas[] canvases = GetComponentsInChildren<Canvas>();
		for (int i = 0; i < canvases.Length; ++i) {
			if (canvases[i].renderMode == mode) return canvases[i];
		}
		return null;
	}

	private void Awake() {
		UnityConsole console = GetComponent<UnityConsole>();
		uiTransform = console.GetUiTransform();
		if (_screenSpaceCanvas == null) {
			Canvas c = uiTransform.parent.GetComponent<Canvas>();
			if (c.renderMode == RenderMode.ScreenSpaceOverlay) { _screenSpaceCanvas = c; } else {
				_screenSpaceCanvas = FindCanvas(RenderMode.ScreenSpaceOverlay);
			}
		}
		if (_worldSpaceCanvas == null) {
			Canvas c = uiTransform.parent.GetComponent<Canvas>();
			if (c.renderMode == RenderMode.WorldSpace) { _worldSpaceCanvas = c; } else {
				_worldSpaceCanvas = FindCanvas(RenderMode.WorldSpace);
			}
		}
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
