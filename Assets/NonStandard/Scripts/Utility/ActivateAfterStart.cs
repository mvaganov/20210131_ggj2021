using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Utility {
	public class ActivateAfterStart : MonoBehaviour {
		public UnityEvent afterStart;
		void Start() { Clock.setTimeout(afterStart.Invoke,0); }
	}
}