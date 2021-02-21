using NonStandard.GameUi;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.Ui {
	public class ListItemUi : MonoBehaviour {
		[SerializeField] private GameObject _text;
		public Button button;
		public object item;
		public GameObject text { get { if (_text != null) { return _text; } return _text = UiText.GetTextObject(gameObject); } }
		public Color TextColor {
			get { return UiText.GetColor(text); }
			set { UiText.SetColor(text, value); }
		}
		public string Text {
			get { return UiText.GetText(text); }
			set { UiText.SetText(text, value); }
		}
		public TextAnchor TextAlignment {
			get { return UiText.GetAlignment(text); }
			set { UiText.SetAlignment(text, value); }
		}
	}
}