﻿using NonStandard.Procedure;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instigator : MonoBehaviour
{
	void Start() {
		Proc.Delay(200, () => { Debug.Log("HELLO!"); });
		Proc.OnIncident("Jump", occasion => {
			Debug.Log(NonStandard.Show.Stringify(occasion));
		}, 3);
		Proc.Delay(100, () => { Debug.Log("...."); });
		Proc.Delay(300, () => { Debug.Log("WORLD!"); });

		int kPresses = 0;
		Strategy strat = new Strategy("the thing").AndThen("print hello", incident => {
			Debug.Log("Hello");
		}).ThenOnIncident("K", incident => {
			kPresses = 1;
			Proc.OnIncident("K", kpress => {
				kPresses++;
				Debug.Log("~~ " + kPresses + " ~~" + kpress);
			});
			Debug.Log("pressed k");
		}).ThenDelay("count down 3", 1000, incident=> {
			Debug.Log("3: "+NonStandard.Show.Stringify(incident));
		}).ThenDelay("count down 2", 1000, incident => {
			Debug.Log("2: " + NonStandard.Show.Stringify(incident));
		}).ThenDelay("count down 1", 1000, incident => {
			Debug.Log("1: " + NonStandard.Show.Stringify(incident));
		}).ThenDelay("count down 0", 1000, incident => {
			Debug.Log("DONE! " + kPresses + "\n" + NonStandard.Show.Stringify(incident));
		}).ThenDecideBestChoice("check k presses",
			new Strategy("do nothing"),

			new Strategy("bad",()=>kPresses > 1 && kPresses <= 3 ? 1 : 0, incident=>{
				Debug.Log("pathetic.");
			}).ThenDelay("snark",1000,incident=>{
				Debug.Log("do better next time."); 
			}).ThenDelay("pause",3000),

			new Strategy("medium",()=>kPresses > 3 && kPresses < 10 ? 2 : 0, incident=>{
				Debug.Log("you pressed K.");
			}),

			new Strategy("best",()=>kPresses > 10 ? 1 : 0, incident=>{
				Debug.Log("pog");
			})
		).AndThen("the end", incident=> {
			Debug.Log("THE END");
		}).Root();
		Debug.Log("doing [" + strat.ListStrategies().JoinToString() + "]");
		strat.Invoke(new Incident("Instigator started"));
	}

	void Update() {
		if (Input.GetButtonDown("Jump")) {
			Proc.NotifyIncident("Jump", this, "jump detail");
		}
		if (Input.GetKeyDown(KeyCode.K)) {
			Proc.NotifyIncident("K", this, "k");
		}
		Proc.Update();
	}
}
