using NonStandard.Data;
using NonStandard.Utility.UnityEditor;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.Ui {
	public class UiText : MonoBehaviour {
		public UnityEvent_string setText;
		public Func<string> getText = null;
		private void Reset() { Init(); }
		public void Init() {
			TMPro.TMP_InputField tif = GetComponentInChildren<TMPro.TMP_InputField>();
			if (tif != null) { EventBind.IfNotAlready(setText, tif, "set_text"); getText = () => tif.text; }
			TMPro.TMP_Text tmp = GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { EventBind.IfNotAlready(setText, tmp, "set_text"); getText = () => tmp.text; }
			InputField inf = GetComponentInChildren<InputField>();
			if (inf != null) { EventBind.IfNotAlready(setText, inf, "set_text"); getText = () => inf.text; }
			Text txt = GetComponentInChildren<Text>();
			if (txt != null) { EventBind.IfNotAlready(setText, txt, "set_text"); getText = () => txt.text; }
			Image img = GetComponentInChildren<Image>();
			SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
			if (img != null || sr != null) { EventBind.IfNotAlready(setText, this, SetImageByName); getText = GetImageName; }
		}
		public void SetImageByName(string name) {
			Image img = GetComponentInChildren<Image>();
			if (img != null) { img.sprite = GetImageByName(name); return; }
			SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
			if (sr != null) { sr.sprite = GetImageByName(name); return; }
		}
		public string GetImageName() {
			Image img = GetComponentInChildren<Image>();
			if (img != null) { return img.sprite.name; }
			SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
			if (sr != null) { return sr.sprite.name; }
			return null;
		}
		public Sprite GetImageByName(string img) {
			Show.Log("need to get image "+img);
			return null;
		}
		public static GameObject GetTextObject(GameObject go) {
			UiText uit = go.GetComponentInChildren<UiText>();
			if (uit != null) { return uit.gameObject; }
			TMPro.TMP_InputField tif = go.GetComponentInChildren<TMPro.TMP_InputField>();
			if (tif != null) { return tif.gameObject; }
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { return tmp.gameObject; }
			InputField inf = go.GetComponentInChildren<InputField>();
			if (inf != null) { return inf.gameObject; }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { return txt.gameObject; }
			return null;
		}
		public static void SetText(GameObject go, string value) {
			UiText uit = go.GetComponentInChildren<UiText>();
			if (uit != null) { uit.setText.Invoke(value); return; }
			TMPro.TMP_InputField tif = go.GetComponentInChildren<TMPro.TMP_InputField>();
			if (tif != null) { tif.text = value; return; }
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { tmp.text = value; return; }
			InputField inf = go.GetComponentInChildren<InputField>();
			if (inf != null) { inf.text = value; return; }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { txt.text = value; return; }
		}
		public static string GetText(GameObject go) {
			UiText uit = go.GetComponentInChildren<UiText>();
			if (uit != null && uit.getText != null) { return uit.getText.Invoke(); }
			TMPro.TMP_InputField tif = go.GetComponentInChildren<TMPro.TMP_InputField>();
			if (tif != null) { return tif.text; }
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { return tmp.text; }
			InputField inf = go.GetComponentInChildren<InputField>();
			if (inf != null) { return inf.text; }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { return txt.text; }
			return null;
		}
		public static float GetFontSize(GameObject go) {
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { return tmp.fontSize; }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { return txt.fontSize; }
			return -1;
		}
		public static void SetFontSize(GameObject go, float value) {
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { tmp.fontSize = value; return; }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { txt.fontSize = (int)value; return; }
		}
		public static void SetColor(GameObject go, Color value) {
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { tmp.faceColor = value; return; }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { txt.color = value; }
		}
		public static Color GetColor(GameObject go) {
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { return tmp.faceColor; }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { return txt.color; }
			return Color.clear;
		}
		public static TextAnchor GetAlignment(GameObject go) {
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { return ConvertTextAnchor(tmp.alignment); }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { return txt.alignment; }
			return TextAnchor.MiddleCenter;
		}
		public static TextAnchor SetAlignment(GameObject go, TextAnchor ta) {
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { tmp.alignment = ConvertTextAnchor(ta); return ConvertTextAnchor(tmp.alignment); }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { return txt.alignment = ta; }
			return TextAnchor.MiddleCenter;
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
}