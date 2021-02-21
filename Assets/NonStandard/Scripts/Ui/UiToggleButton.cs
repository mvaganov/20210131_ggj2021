using NonStandard.GameUi;
using UnityEngine;
using UnityEngine.UI;

public class UiToggleButton : MonoBehaviour {
    public GameObject uiToControlVisibility;
    public Button uiToggleClose;
    public bool hideThisWhenUiVisible;
    public bool triggerOnStart;
    [TextArea(1,5)] public string alternateText;
    public void DoActivateTrigger() {
        uiToControlVisibility.SetActive(!uiToControlVisibility.activeSelf);
        if (hideThisWhenUiVisible) {
            gameObject.SetActive(!uiToControlVisibility.activeSelf);
        }
        if (!string.IsNullOrEmpty(alternateText)) {
            string temp = UiText.GetText(gameObject);
            UiText.SetText(gameObject, alternateText);
            alternateText = temp;
        }
	}
    void Start() {
        GetComponent<Button>().onClick.AddListener(DoActivateTrigger);
        if(uiToggleClose != null) { uiToggleClose.onClick.AddListener(DoActivateTrigger); }
        if(triggerOnStart) { DoActivateTrigger(); }
    }
}
