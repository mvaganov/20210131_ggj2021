using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixelInteraction : MonoBehaviour {
    public KeyCode key = KeyCode.Mouse0;
    public RectTransform rect;
    //public RawImage rimg;
    //public Texture2D tex;
    public Color color = Color.green;
    //Color32[] pixels;
    void Start() {
        rect = GetComponent<RectTransform>();
        Lines.DrawLine(rect, Vector2.zero, Vector2.zero, Color.clear);
        //tex = Lines.GetRawImageTexture(rect);
        //pixels = tex.GetPixels32();
        RawImage rimg = GetComponent<RawImage>();
        rimg.raycastTarget = false;
    }

    Vector2 pressStarted, currentPress, lastPosition;

    void Update() {
        bool show = false, released = false;
        Vector3 s = transform.lossyScale;
        if (Input.GetKeyDown(key)) {
            pressStarted = Input.mousePosition;
            pressStarted.x /= s.x;
            pressStarted.y /= s.y;
        }
        if (Input.GetKey(key) || Input.GetKeyUp(key)) {
            currentPress = Input.mousePosition;
            currentPress.x /= s.x;
            currentPress.y /= s.y;
            released = Input.GetKeyUp(key);
            show = lastPosition != currentPress || released;
        }
        if (show) {
            if (!released) {
                Lines.DrawAABB(rect, pressStarted, lastPosition, Color.clear, false);
                Lines.DrawAABB(rect, pressStarted, currentPress, color);
			} else {
                Lines.DrawAABB(rect, pressStarted, currentPress, Color.clear);
            }
            lastPosition = currentPress;
        }
    }
}
