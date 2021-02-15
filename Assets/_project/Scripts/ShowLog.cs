using NonStandard;
using UnityEngine;

public class ShowLog : MonoBehaviour {
    public TMPro.TMP_Text tmpText;
    void Awake() { Show.AddListener(s => tmpText.text += s+"\n"); }
}
