using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.GameUi {
	public class ButtonPulse : MonoBehaviour {
		public List<Color> pulseColor = new List<Color>() { Color.red };
		public float pulseRate = 1;
		float timer;
		int colorIndex;
		Button b;
		public void ToggleEnabled() {
			enabled = !enabled;
			if (!enabled) { SetButtonColor(pulseColor[0]); }
		}
		private void Start() {
			b = GetComponent<Button>();
			pulseColor.Insert(0, b.colors.normalColor);
		}
		public void SetButtonColor(Color c) {
			ColorBlock cb = b.colors;
			cb.normalColor = c;
			b.colors = cb;
		}
		void Update() {
			timer += Time.unscaledDeltaTime;
			float p = 1;
			if (timer < pulseRate) { p = timer / pulseRate; }
			Color s = pulseColor[colorIndex], e = pulseColor[(colorIndex + 1) % pulseColor.Count];
			SetButtonColor(Color.Lerp(s, e, p));
			if (p >= 1) {
				++colorIndex;
				if (colorIndex >= pulseColor.Count) { colorIndex = 0; }
				timer = 0;
			}
		}
	}
}