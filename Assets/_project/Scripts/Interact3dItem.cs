using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interact3dItem : MonoBehaviour
{
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
			if (interactUi != null) {
				Button b = interactUi.GetComponentInChildren<Button>();
				if(b != null) {
					b.onClick.RemoveAllListeners();
					b.onClick.AddListener(_onInteract.Invoke);
				}
			}
		}
	}
	public RectTransform interactUi {
		get { return _interactUi; }
		set {
			_interactUi = value;
			Text = _text;
			OnInteract = _onInteract;
			showing = _showing;
		}
	}
	public TMPro.TMP_Text TmpText {
		get {
			return interactUi.GetComponentInChildren<TMPro.TMP_Text>();
		}
	}
	public string Text {
		get { return _text; }
		set {
			_text = value;
			if (interactUi != null) { TmpText.text = value; }
		}
	}
}
