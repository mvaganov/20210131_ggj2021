using NonStandard;
using NonStandard.Extension;
using NonStandard.Process;
using UnityEngine;

public class Instigator : MonoBehaviour
{
	void Start() {
		Proc.Delay(200, () => { Debug.Log("HELLO!"); });
		Proc.OnIncident(Proc.Id.CreateIfNotFound("Jump"), occasion => {
			Debug.Log(occasion.Stringify());
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
			Debug.Log("3: "+ incident.Stringify());
		}).ThenDelay("count down 2", 1000, incident => {
			Debug.Log("2: " + incident.Stringify());
		}).ThenDelay("count down 1", 1000, incident => {
			Debug.Log("1: " + incident.Stringify());
		}).ThenDelay("count down 0", 1000, incident => {
			Debug.Log("DONE! " + kPresses + "\n" + incident.Stringify());
		}).ThenDecideBestChoice("check k presses",
			new Strategy("do nothing"),

			new Strategy(()=>kPresses > 1 && kPresses <= 3 ? 1 : 0, "bad", incident =>{
				Debug.Log("pathetic.");
			}).ThenDelay("snark",1000,incident=>{
				Debug.Log("do better next time."); 
			}).ThenDelay("pause",3000),

			new Strategy(()=>kPresses > 3 && kPresses < 10 ? 2 : 0, "medium", incident =>{
				Debug.Log("you pressed K.");
			}),

			new Strategy(()=>kPresses > 10 ? 1 : 0, "best", incident=>{
				Debug.Log("pog");
			})
		).AndThen("the end", incident=> {
			Debug.Log("THE END");
		}).Root();
		//Debug.Log("doing [" + strat.ListStrategies().JoinToString() + "]");
		strat.Invoke(new Incident("Instigator started"));
		GameClock.Instance();
	}

	void Update() {
		if (Input.GetButtonDown("Jump")) {
			Proc.NotifyIncident("Jump", this, "jump detail");
		}
		if (Input.GetKeyDown(KeyCode.K)) {
			Proc.NotifyIncident("K", this, "k");
		}
		//Proc.Update();
	}
}
