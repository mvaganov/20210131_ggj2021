using NonStandard.Process;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Utility {
	public class ActivateAfterStart : MonoBehaviour {
		public UnityEvent afterStart;
		void Start() { Proc.Delay(0, afterStart.Invoke); }
	}
}