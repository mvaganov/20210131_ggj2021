using NonStandard.Inputs;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Dialog {
	public class UiExclusion : MonoBehaviour {
		public List<RectTransform> uiToDisableDuringDialog = new List<RectTransform>();
		List<Transform> home = new List<Transform>();
		RectTransform hiddenPlace;
		// Start is called before the first frame update
		void Start() {
			hiddenPlace = new GameObject("hidden").AddComponent<RectTransform>();
			hiddenPlace.SetParent(transform);
			hiddenPlace.gameObject.SetActive(false);
		}
		private void OnEnable() {
			if (!enabled || hiddenPlace == null) return;
			home.Clear();
			for (int i = 0; i < uiToDisableDuringDialog.Count; ++i) {
				home.Add(uiToDisableDuringDialog[i].parent);
				uiToDisableDuringDialog[i].SetParent(hiddenPlace, false);
			}
		}
		private void OnDisable() {
			if (!enabled || hiddenPlace == null || AppInput.IsQuitting) return;
			for (int i = 0; i < home.Count; ++i) {
				uiToDisableDuringDialog[i].SetParent(home[i], false);
			}
		}
	}
}