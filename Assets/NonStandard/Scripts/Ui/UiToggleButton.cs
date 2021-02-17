using UnityEngine;
using UnityEngine.UI;

public class UiToggleButton : MonoBehaviour {
    public GameObject uiToControlVisibility;
    public Button uiToggleClose;
    public bool hideThisWhenUiVisible;
    public bool triggerOnStart;
    public void DoActivateTrigger() {
        uiToControlVisibility.SetActive(!uiToControlVisibility.activeSelf);
        if (hideThisWhenUiVisible) {
            gameObject.SetActive(!uiToControlVisibility.activeSelf);
        }
	}
    void Start() {
        GetComponent<Button>().onClick.AddListener(DoActivateTrigger);
        if(uiToggleClose != null) { uiToggleClose.onClick.AddListener(DoActivateTrigger); }
        if(triggerOnStart) { DoActivateTrigger(); }
    }
}
