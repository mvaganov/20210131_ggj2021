using NonStandard;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour {
    long started, pausedTime;
    public int min, sec, ms;
    public bool paused;
    public Text label;
    public void Start() {
		if (label == null) { label = GetComponent<Text>(); }
        paused = false;
        pausedTime = 0;
        started = Clock.Now;
    }

    public void Pause() { paused = true; }
    public void Unpause() { paused = false; }
    void Update() {
        if (!paused) {
			if (pausedTime != 0) {
                long delta = pausedTime - started;
                started = Clock.Now - delta;
                pausedTime = 0;
            }
            ms = GetDuration();
            sec = Math.DivRem(ms, 1000, out ms);
            min = Math.DivRem(sec, 60, out sec);
            StringBuilder sb = new StringBuilder();
		    if (min > 0) { sb.Append(min).Append(":").Append(sec.ToString("D2")); } else {
                sb.Append(sec.ToString());
            }
            label.text = sb.ToString();
            if (!label.enabled) { label.enabled = true; }
        } else {
            if(pausedTime == 0) { pausedTime = Clock.Now; }
            label.enabled = (Clock.NowRealTicks & (1<<blinkDuration)) == 0;
        }
    }
    int blinkDuration = 9;

    public int GetDuration() {
        return (int)(Clock.Now - started);
    }
}
