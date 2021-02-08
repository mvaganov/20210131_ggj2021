using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interact3dItem : MonoBehaviour
{
	public string interactText = "interact";
	public RectTransform interactUi;
	public Vector3 worldOffset;
	public bool alwaysOn = false;
	public Action onInteract;
	public Action onInteractVisible;
	public Action onInteractHidden;
	public void Start() {
		
	}
	public string Text {
		get { return interactUi.GetComponentInChildren<Text>().text; }
		set { interactUi.GetComponentInChildren<Text>().text= value; }
	}
	public Button.ButtonClickedEvent OnButton {
		get { return interactUi.GetComponentInChildren<Button>().onClick; }
		set { interactUi.GetComponentInChildren<Button>().onClick= value; }
	}
}
