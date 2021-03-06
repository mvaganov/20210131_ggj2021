﻿using UnityEngine;
using UnityEngine.UI;

public static class UiText {
	public static GameObject GetTextObject(GameObject go) {
		TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
		if (tmp != null) { return tmp.gameObject; }
		return go.GetComponentInChildren<Text>().gameObject;
	}
	public static void SetText(GameObject go, string value) {
		TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
		if (tmp != null) { tmp.text = value; return; }
		Text txt = go.GetComponentInChildren<Text>();
		txt.text = value;
	}
	public static string GetText(GameObject go) {
		TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
		if (tmp != null) { return tmp.text; }
		Text txt = go.GetComponentInChildren<Text>();
		return txt.text;
	}
	public static float GetFontSize(GameObject go) {
		TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
		if (tmp != null) { return tmp.fontSize; }
		return go.GetComponentInChildren<Text>().fontSize;
	}
	public static void SetFontSize(GameObject go, float value) {
		TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
		if (tmp != null) { tmp.fontSize = value; return; }
		go.GetComponentInChildren<Text>().fontSize = (int)value;
	}
	public static void SetColor(GameObject go, Color value) {
		TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
		if (tmp != null) { tmp.faceColor = value; return; }
		go.GetComponentInChildren<Text>().color = value;
	}
	public static Color GetColor(GameObject go) {
		TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
		if (tmp != null) { return tmp.faceColor; }
		return go.GetComponentInChildren<Text>().color;
	}
	public static TextAnchor GetAlignment(GameObject go) {
		TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
		if (tmp != null) { return ConvertTextAnchor(tmp.alignment); }
		return go.GetComponentInChildren<Text>().alignment;
	}
	public static TextAnchor SetAlignment(GameObject go, TextAnchor ta) {
		TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
		if (tmp != null) { return ConvertTextAnchor(tmp.alignment); }
		return go.GetComponentInChildren<Text>().alignment = ta;
	}
	public static TextAnchor ConvertTextAnchor(TMPro.TextAlignmentOptions ta) {
		TextAnchor t = TextAnchor.MiddleCenter;
		switch (ta) {
		case TMPro.TextAlignmentOptions.Left:
		case TMPro.TextAlignmentOptions.MidlineLeft: t = TextAnchor.MiddleLeft; break;
		case TMPro.TextAlignmentOptions.TopLeft: t = TextAnchor.UpperLeft; break;
		case TMPro.TextAlignmentOptions.BottomLeft: t = TextAnchor.LowerLeft; break;
		case TMPro.TextAlignmentOptions.Right:
		case TMPro.TextAlignmentOptions.MidlineRight: t = TextAnchor.MiddleRight; break;
		case TMPro.TextAlignmentOptions.TopRight: t = TextAnchor.UpperRight; break;
		case TMPro.TextAlignmentOptions.BottomRight: t = TextAnchor.LowerRight; break;
		case TMPro.TextAlignmentOptions.Center:
		case TMPro.TextAlignmentOptions.MidlineJustified: t = TextAnchor.MiddleCenter; break;
		case TMPro.TextAlignmentOptions.TopJustified: t = TextAnchor.UpperCenter; break;
		case TMPro.TextAlignmentOptions.BottomJustified: t = TextAnchor.LowerCenter; break;
		}
		return t;
	}
	public static TMPro.TextAlignmentOptions ConvertTextAnchor(TextAnchor ta) {
		TMPro.TextAlignmentOptions t = TMPro.TextAlignmentOptions.Center;
		switch (ta) {
		case TextAnchor.MiddleLeft: t = TMPro.TextAlignmentOptions.MidlineLeft; break;
		case TextAnchor.UpperLeft: t = TMPro.TextAlignmentOptions.TopLeft; break;
		case TextAnchor.LowerLeft: t = TMPro.TextAlignmentOptions.BottomLeft; break;
		case TextAnchor.MiddleRight: t = TMPro.TextAlignmentOptions.MidlineRight; break;
		case TextAnchor.UpperRight: t = TMPro.TextAlignmentOptions.TopRight; break;
		case TextAnchor.LowerRight: t = TMPro.TextAlignmentOptions.BottomRight; break;
		case TextAnchor.MiddleCenter: t = TMPro.TextAlignmentOptions.MidlineJustified; break;
		case TextAnchor.UpperCenter: t = TMPro.TextAlignmentOptions.TopJustified; break;
		case TextAnchor.LowerCenter: t = TMPro.TextAlignmentOptions.BottomJustified; break;
		}
		return t;
	}
}
