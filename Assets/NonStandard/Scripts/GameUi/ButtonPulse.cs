using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPulse : MonoBehaviour
{
    public List<Color> pulseColor = new List<Color>() { Color.red };
    public float pulseRate = 1;
    float timer;
    int colorIndex;
    Button b;
	private void Start() {
        b = GetComponent<Button>();
        pulseColor.Insert(0, b.colors.normalColor);
	}
	void Update()
    {
        timer += Time.deltaTime;
        float p = 1;
        if(timer < pulseRate) { p = timer / pulseRate; }
        Color s = pulseColor[colorIndex], e = pulseColor[(colorIndex + 1) % pulseColor.Count];
        ColorBlock cb = b.colors;
        cb.normalColor = Color.Lerp(s, e, p);
        b.colors = cb;
        if(p >= 1) {
            ++colorIndex;
            if (colorIndex >= pulseColor.Count) { colorIndex = 0; }
            timer = 0;
		}
    }
}
