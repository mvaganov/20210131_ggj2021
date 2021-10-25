using NonStandard.Data;
using NonStandard.Utility.UnityEditor;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.Ui {
	public class UiText : MonoBehaviour {
		public UnityEvent_string setText;
		public UnityEvent_string OnSetText => setText;
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
			if (UiImage.HasImageHolder(gameObject)) { EventBind.IfNotAlready(setText, this, SetImageByName); getText = GetImageName; }
		}
		public void SetImageByName(string name) { UiImage.SetSpriteByName(gameObject, name); }
		public string GetImageName() { return UiImage.GetImageName(gameObject); }
		public static bool HasText(GameObject go) { return GetTextComponent(go) != null; }
		public static Component GetTextComponent(GameObject go) {
			UiText uit = go.GetComponentInChildren<UiText>();
			if (uit != null) { return uit; }
			TMPro.TMP_InputField tif = go.GetComponentInChildren<TMPro.TMP_InputField>();
			if (tif != null) { return tif; }
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { return tmp; }
			InputField inf = go.GetComponentInChildren<InputField>();
			if (inf != null) { return inf; }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { return txt; }
			return null;
		}
		public static Component SetText(GameObject go, string value) {
			//Show.Log("setting "+go.name+" to \""+value+"\"");
			UiText uit = go.GetComponentInChildren<UiText>();
			if (uit != null) { uit.setText.Invoke(value); return uit; }
			TMPro.TMP_InputField tif = go.GetComponentInChildren<TMPro.TMP_InputField>();
			if (tif != null) { tif.text = value; return tif; }
			TMPro.TMP_Text tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
			if (tmp != null) { tmp.text = value; return tmp; }
			InputField inf = go.GetComponentInChildren<InputField>();
			if (inf != null) { inf.text = value; return inf; }
			Text txt = go.GetComponentInChildren<Text>();
			if (txt != null) { txt.text = value; return txt; }
			return null;
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
		public static bool TryGetText(Component c, out string text) {
			switch (c) {
			case UiText uit: if (uit.getText == null) { text = null; return false; } text = uit.getText.Invoke(); return true;
			case TMPro.TMP_InputField tif: text = tif.text; return true;
			case TMPro.TMP_Text tmp: text = tmp.text; return true;
			case InputField inf: text = inf.text; return true;
			case Text txt: text = txt.text; return true;
			}
			text = null;
			return false;
		}
		public static bool TrySetText(Component c, string text) {
			switch (c) {
			case UiText uit: if (uit.getText == null) { return false; } uit.setText.Invoke(text); return true;
			case TMPro.TMP_InputField tif: tif.text = text; return true;
			case TMPro.TMP_Text tmp: tmp.text = text; return true;
			case InputField inf: inf.text = text; return true;
			case Text txt: txt.text = text; return true;
			}
			return false;
		}
		public static bool TryGetColor(Component c, out Color color) {
			switch (c) {
			case TMPro.TMP_Text tmp: color = tmp.faceColor; return true;
			case Text txt: color = txt.color; return true;
			}
			color = Color.magenta;
			return false;
		}
		public static bool TrySetColor(Component c, Color color) {
			switch (c) {
			case TMPro.TMP_Text tmp: tmp.faceColor = color; return true;
			case Text txt: txt.color = color; return true;
			}
			return false;
		}
		public static bool TryGetAlignment(Component c, out TextAnchor anchor) {
			switch (c) {
			case TMPro.TMP_Text tmp: anchor = ConvertTextAnchor(tmp.alignment); return true;
			case Text txt: anchor = txt.alignment; return true;
			}
			anchor = TextAnchor.MiddleCenter;
			return false;
		}
		public static bool TrySetAlignment(Component c, TextAnchor anchor) {
			switch (c) {
			case TMPro.TMP_Text tmp: tmp.alignment = ConvertTextAnchor(anchor); return true;
			case Text txt: txt.alignment = anchor; return true;
			}
			return false;
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