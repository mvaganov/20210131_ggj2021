using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instigator : MonoBehaviour
{
	Procedure om;
	void Start() {
		//Procedure.Delay(3000, () => { Debug.Log("HELLO!"); });
		//Procedure.OnIncident("Jump", occasion => {
		//	Debug.Log(NonStandard.Show.Stringify(occasion));
		//	return Procedure.Result.Success;
		//}, 3);
		//Procedure.Delay(2000, () => { Debug.Log("...."); });
		//Procedure.Delay(5000, () => { Debug.Log("WORLD!"); });

		int kPresses = 0;
		Strategy strat = new Strategy("the thing").AndThen("print hello", incident => {
			Debug.Log("Hello");
		}).ThenOnIncident("K", incident => {
			kPresses = 1;
			Procedure.OnIncident("K", kpress => {
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
		}).ThenDecideBestChoice("check k presses", new Decision[] {
			new Decision("do nothing"),
			new Decision("bad",()=>kPresses > 1 && kPresses <= 3 ? 1 : 0, incident=>{
				Debug.Log("pathetic.");
			}).ThenDelay("snark",1000,incident=>{
				Debug.Log("do better next time."); 
			}).RootDecision(),
			new Decision("medium",()=>kPresses > 3 && kPresses < 10 ? 2 : 0, incident=>{
				Debug.Log("you pressed K.");
			}),
			new Decision("best",()=>kPresses > 10 ? 1 : 0, incident=>{
				Debug.Log("pog");
			}),
		}).AndThen("the end", incident=> {
			Debug.Log("THE END");
		}).Root();
		Debug.Log("doing [" + strat.ListStrategies().JoinToString() + "]");
		strat.Invoke(new Incident("Instigator started"));
	}

	void Update() {
		if (Input.GetButtonDown("Jump")) {
			Procedure.NotifyIncident("Jump", this, "jump detail");
		}
		if (Input.GetKeyDown(KeyCode.K)) {
			Procedure.NotifyIncident("K", this, "k");
		}
		Procedure.Update();
	}
}
