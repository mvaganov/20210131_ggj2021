using UnityEngine;

public class FloatyText : MonoBehaviour
{
    private TMPro.TMP_Text tmpText;
    public float duration = 3;
    public float speed = 1;
    public Camera _cam;
    public Camera cam {
        get {
            if (_cam != null) { return _cam; }
            return _cam = Camera.main;
        }
        set { _cam = value; if (_cam != null) { transform.rotation = _cam.transform.rotation; } }
    }
    public TMPro.TMP_Text TmpText {
        get {
            if(tmpText != null) { return tmpText; }
            return tmpText = GetComponent<TMPro.TMP_Text>();
        }
	}
    public string Text {
        get { return name; }
        set {
            name = value;
            if(TmpText != null) { TmpText.text = name; }
		}
	}
    void Start() {
        Text = name;
        transform.rotation = cam.transform.rotation;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.up * speed;
        NonStandard.Clock.setTimeout(() => Destroy(gameObject), (long)(duration * 1000));
    }
}
