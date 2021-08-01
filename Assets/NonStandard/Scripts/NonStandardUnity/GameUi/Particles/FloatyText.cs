using NonStandard.Process;
using UnityEngine;

namespace NonStandard.GameUi.Particles {
    public class FloatyText : MonoBehaviour {
        private TMPro.TMP_Text tmpText;
        public float duration = 3;
        public float speed = 1;
        public bool fade = true;
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
                if (tmpText != null) { return tmpText; }
                return tmpText = GetComponent<TMPro.TMP_Text>();
            }
        }
        public string Text {
            get { return name; }
            set {
                name = value;
                if (TmpText != null) { TmpText.text = name; }
            }
        }
        void Start() {
            Text = name;
            transform.rotation = cam.transform.rotation;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.up * speed;
            long timing = (long)(duration * 1000);
            GameClock.Delay(timing, () => Destroy(gameObject));
            if (fade) {
                TMPro.TMP_Text tt = TmpText;
                Color originalFace = tt.faceColor, originalOutline = tt.outlineColor;
                Proc.SystemClock.Lerp(p => {
                    if (tt != null) {
                        tt.faceColor = Color.Lerp(originalFace, Color.clear, p);
                        tt.outlineColor = Color.Lerp(originalOutline, Color.clear, p);
                        transform.rotation = cam.transform.rotation;
                    }
                    //Show.Log(p);
                }, timing, 10);
            }
        }
    }
}