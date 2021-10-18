using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.Ui {
	[RequireComponent(typeof(Button))]
	public class UiToggleButton : MonoBehaviour {
		public GameObject uiToControlVisibility;
		public Button uiToggleClose;
		public bool uiStartsHidden;
		public bool hideThisWhenUiVisible;
		public bool clickMeAfterStart;
		[TextArea(1, 5)] public string alternateText;
		public void ClickButton() {
			Button b = GetComponent<Button>();
			if (b != null) { b.onClick.Invoke(); }
		}
		public void DoActivateTrigger() {
			//Debug.Log("doactivate " + this);
			if (uiToControlVisibility) {
				uiToControlVisibility.SetActive(!uiToControlVisibility.activeSelf);
				if (hideThisWhenUiVisible) {
					gameObject.SetActive(!uiToControlVisibility.activeSelf);
				}
			}
			if (!string.IsNullOrEmpty(alternateText)) {
				string temp = UiText.GetText(gameObject);
				UiText.SetText(gameObject, alternateText);
				alternateText = temp;
			}
		}
		void Start() {
			Button b = GetComponent<Button>();
			b.onClick.AddListener(DoActivateTrigger);
			if (uiToggleClose != null) { uiToggleClose.onClick.AddListener(DoActivateTrigger); }
			if (uiStartsHidden || clickMeAfterStart) GameClock.Delay(0, () => {
				if (uiStartsHidden) { uiToControlVisibility.SetActive(false); }
				if (clickMeAfterStart) {
					//Debug.Log("first click " + b);
					UiClick.Click(b);
				}
			});
		}
	}
}