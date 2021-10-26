using NonStandard;
using NonStandard.Inputs;
using NonStandard.Utility.UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PixelInteraction : MonoBehaviour {
	public KCode key = KCode.Mouse0;
	public RectTransform rect;
	//public RawImage rimg;
	//public Texture2D tex;
	public Color color = Color.green;
	Color32[] pixels;
	bool held = false, dirtyRectangle;
	Vector2 pressStarted, currentPress, lastPosition;

	void Start() {
		rect = GetComponent<RectTransform>();
		Lines.DrawLine(rect, Vector2.zero, Vector2.zero, Color.clear);
		//tex = Lines.GetRawImageTexture(rect);
		//pixels = tex.GetPixels32();
		RawImage rimg = GetComponent<RawImage>();
		rimg.raycastTarget = false;
		KeyInput ki = GetComponent<KeyInput>();
		if (ki == null) { ki = gameObject.AddComponent<KeyInput>(); }
		ki.AddListener(new KBind(key, "draw selection rectangle",
			pressFunc: new EventBind(this, nameof(StartPress)),
			holdFunc: new EventBind(this, nameof(ContinuePress)),
			releaseFunc: new EventBind(this, nameof(ReleasePress))
		));
	}
	private void OnEnable() {
		KeyInput ki = GetComponent<KeyInput>();
		if (ki != null) { ki.enabled = true; }
	}
	private void OnDisable() {
		KeyInput ki = GetComponent<KeyInput>();
		if (ki != null) { ki.enabled = false; }
	}

	public void StartPress() {
		if (held) return;
		pressStarted = AppInput.MousePosition;
		Vector3 s = transform.lossyScale;
		pressStarted.x /= s.x;
		pressStarted.y /= s.y;
		dirtyRectangle = true;
		held = true;
	}
	public void ContinuePress() {
		currentPress = AppInput.MousePosition;
		Vector3 s = transform.lossyScale;
		currentPress.x /= s.x;
		currentPress.y /= s.y;
		held = true;
		dirtyRectangle |= lastPosition != currentPress;
	}
	public void ReleasePress() {
		Vector3 s = transform.lossyScale;
		currentPress.x /= s.x;
		currentPress.y /= s.y;
		dirtyRectangle = true;
		held = false;
	}
	void Update() {
		if (dirtyRectangle) {
			if (held) {
				Lines.DrawAABB(rect, pressStarted, lastPosition, Color.clear, false);
				Lines.DrawAABB(rect, pressStarted, currentPress, color);
				dirtyRectangle = false;
			} else {
				Clear();
			}
			lastPosition = currentPress;
		}
	}

	internal void DrawRect(Rect r, Color color) { Lines.DrawAABB(rect, r.min, r.max, color); }

	public static bool C32Equals(Color32 a, Color32 b) { return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a; }
	/// <param name="rectTransform">rectangle to draw on. should have RawImage (or no Renderer at all)</param>
	/// <param name="buffer">used to write pixel data</param>
	/// <param name="color">what color to fill. if no value given, will set entire texture to <see cref="UnityEngine.Color.clear"/></param>
	public static Texture2D FillTexture(RectTransform rectTransform, ref Color32[] buffer, Color32 color = default(Color32)) {
		Texture2D tex = Lines.GetRawImageTexture(rectTransform);
		if (buffer == null) {
			buffer = new Color32[tex.height * tex.width];
		}
		if (!C32Equals(color, default(Color32))) {
			for (int i = 0; i < buffer.Length; ++i) { buffer[i] = color; }
		}
		tex.SetPixels32(buffer);
		return tex;
	}

	public void Clear() {
		FillTexture(rect, ref pixels, Color.clear).Apply();
	}
}
