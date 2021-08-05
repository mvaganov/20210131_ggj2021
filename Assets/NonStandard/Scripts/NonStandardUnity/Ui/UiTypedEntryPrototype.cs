using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Ui {
	public class UiTypedEntryPrototype : MonoBehaviour {
		Dictionary<string, GameObject> cellTypes = new Dictionary<string, GameObject>();
		public void Init() {
			for (int i = 0; i < transform.childCount; ++i) {
				Transform t = transform.GetChild(i);
				if (t == null) continue;
				cellTypes[t.name] = t.gameObject;
			}
		}
		public void Awake() { Init(); }
		public GameObject GetElement(string name) {
			if (cellTypes.Count == 0) { Init(); }
			cellTypes.TryGetValue(name, out GameObject found);
			//Show.Log(name + ": " + found);
			return found;
		}
	}
}