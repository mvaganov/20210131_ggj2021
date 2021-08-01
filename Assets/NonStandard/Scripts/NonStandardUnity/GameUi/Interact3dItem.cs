using NonStandard.Ui;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.GameUi {
	public class Interact3dItem : MonoBehaviour {
		[SerializeField] private string _text = "interact";
		[SerializeField] private Action _onInteract;
		public Action onInteractVisible;
		public Action onInteractHidden;
		public InternalState internalState = new InternalState();

		[System.Serializable] public class InternalState {
			public RectTransform _interactUi;
			public Vector3 worldOffset;
			public bool _showing = true;
			public bool alwaysOn = false;
			public float size = 1;
			public float fontCoefficient = 1;
		}


		public void Start() { if (internalState.alwaysOn) { Interact3dUi.Instance.Add(this); } }
		private void OnDestroy() { if (internalState._interactUi) { Destroy(internalState._interactUi.gameObject); } }
		public bool showing {
			get { return internalState._showing; }
			set {
				internalState._showing = value;
				if (internalState._interactUi) { internalState._interactUi.gameObject.SetActive(internalState._showing); }
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
			get { return internalState._interactUi; }
			set {
				internalState._interactUi = value;
				Text = _text;
				OnInteract = _onInteract;
				showing = internalState._showing;
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