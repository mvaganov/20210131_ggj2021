using NonStandard.Utility.UnityEditor;
using UnityEngine;

namespace NonStandard.Process
{
	public class Timer : MonoBehaviour
	{
		[Tooltip("When to trigger")]
		public float seconds = 1;
		[Tooltip("Transform to teleport to\nSceneAsset to load a new scene\nAudioClip to play audio\nGameObject to SetActivate(true)")]
		public ObjectPtr whatToActivate = new ObjectPtr();
		[Tooltip("restart a timer after triggering")]
		public bool repeat = false;
		[Tooltip("attempt to deactivate instead of activate")]
		public bool deactivate = false;
		[Tooltip("make sure this event happens, even if this timer object is destroyed")]
		public bool relentlessTimer = false;

		private void DoTimer() {
			Timer self = this;
			long delay = (long)(seconds * 1000);
			if (repeat) {
				GameClock.Delay(delay, () => {
					if (self == null) return; DoTimer();
				});
			}
			Incident todo;
			object whatToDo = whatToActivate.Data;
			bool activate = !deactivate;
			if (relentlessTimer) {
				todo = GameClock.Delay(delay, ()=>ActivateAnything.DoActivate(whatToDo, this, null, activate));
			} else {
				todo = GameClock.Delay(delay, ()=> {
					if (self == null) return; // if this timer is destroyed, don't activate
					ActivateAnything.DoActivate(whatToDo, this, null, activate);
				});
			}
			todo.Source = this;
		}
		public void DoActivateTrigger() {
			ActivateAnything.DoActivate(whatToActivate.Data, this, null, !deactivate);
		}
		private void Awake() { GameClock.Instance(); }
		void Start() { if (whatToActivate.Data != null) { DoTimer(); } }
	}
}

