using NonStandard.GameUi;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.Ui {
	public class ListItemUi : MonoBehaviour {
		[SerializeField] private Component _text;
		public Button button;
		public object item;
		public Component text { get { if (_text != null) { return _text; } return _text = UiText.GetTextComponent(gameObject); } }
		public Color TextColor {
			get { UiText.TryGetColor(text, out Color c); return c; }
			set { UiText.TrySetColor(text, value); }
		}
		public string Text {
			get { UiText.TryGetText(text, out string t); return t; }
			set { UiText.TrySetText(text, value); }
		}
		public TextAnchor TextAlignment {
			get { UiText.TryGetAlignment(text, out TextAnchor a); return a; }
			set { UiText.TrySetAlignment(text, value); }
		}
	}
}