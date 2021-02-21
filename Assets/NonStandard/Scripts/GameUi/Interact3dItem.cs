using System;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.GameUi {
	public class Interact3dItem : MonoBehaviour {
		[SerializeField] private string _text = "interact";
		[SerializeField] private RectTransform _interactUi;
		[SerializeField] private Action _onInteract;
		public Vector3 worldOffset;
		[SerializeField] private bool _showing = true;
		public bool alwaysOn = false;
		public float size = 1;
		public float fontCoefficient = 1;
		public Action onInteractVisible;
		public Action onInteractHidden;
		public void Start() { if (alwaysOn) { Interact3dUi.Instance.Add(this); } }
		private void OnDestroy() { if (_interactUi) { Destroy(_interactUi.gameObject); } }
		public bool showing {
			get { return _showing; }
			set {
				_showing = value;
				if (_interactUi) { _interactUi.gameObject.SetActive(_showing); }
			}
		}
		public Action OnInteract {
			get { return _onInteract; }
			set {
				_onInteract = value;
				if (screenUi != null) {
					Button b = screenUi.GetComponentInChildren<Button>();
					if (b != null) {
						if(b.onClick == null) { b.onClick = new Button.ButtonClickedEvent(); }
						b.onClick.RemoveAllListeners();
						if (_onInteract != null) {
							b.onClick.AddListener(_onInteract.Invoke);
						}
					}
				}
			}
		}
		public RectTransform screenUi {
			get { return _interactUi; }
			set {
				_interactUi = value;
				Text = _text;
				OnInteract = _onInteract;
				showing = _showing;
			}
		}
		public string Text {
			get { return _text; }
			set {
				_text = value;
				if (screenUi != null) { UiText.SetText(screenUi.gameObject, value); }
			}
		}
		public float fontSize {
			get { return UiText.GetFontSize(screenUi.gameObject); }
			set { UiText.SetFontSize(screenUi.gameObject, value); }
		}
	}
}