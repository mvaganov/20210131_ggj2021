using UnityEngine;
using UnityEngine.UI;
using NonStandard.Ui;
using NonStandard;

public class UiToggleButton : MonoBehaviour {
    public GameObject uiToControlVisibility;
    public Button uiToggleClose;
    public bool uiStartsHidden;
    public bool hideThisWhenUiVisible;
    public bool clickMeAfterStart;
    [TextArea(1,5)] public string alternateText;
    public void DoActivateTrigger() {
        //Debug.Log("doactivate " + this);
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
        Button b = GetComponent<Button>();
        b.onClick.AddListener(DoActivateTrigger);
        if(uiToggleClose != null) { uiToggleClose.onClick.AddListener(DoActivateTrigger); }
        if(uiStartsHidden || clickMeAfterStart) Clock.setTimeout(() => {
			if (uiStartsHidden) { uiToControlVisibility.SetActive(false); }
            if (clickMeAfterStart) {
                //Debug.Log("first click " + b);
                UiClick.Click(b);
            }
        }, 0);
    }
}
