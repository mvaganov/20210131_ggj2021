using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Inputs {
	public class AxisInput : MonoBehaviour {
		public List<AxBind> AxisBinds = new List<AxBind>();

		public static void Init(IList<AxBind> AxisBinds) {
			if (AxisBinds.Count > 0) {
				for (int i = 0; i < AxisBinds.Count; ++i) { AxisBinds[i].Init(); }
			}
		}

		public static void OnEnable(IList<AxBind> AxisBinds) {
			if (AxisBinds.Count > 0 && !AppInput.HasAxisBind(AxisBinds[0])) {
				for (int i = 0; i < AxisBinds.Count; ++i) { AppInput.AddListener(AxisBinds[i]); }
			}
		}
		public static void OnDisable(IList<AxBind> AxisBinds) {
			if (AppInput.IsQuitting) return;
			if (AxisBinds.Count > 0 && AppInput.HasAxisBind(AxisBinds[0])) {
				for (int i = 0; i < AxisBinds.Count; ++i) { AppInput.RemoveListener(AxisBinds[i]); }
			}
		}
		public static bool RemoveBind(List<AxBind> AxisBinds, string name) {
			int index = AxisBinds.FindIndex(kb => kb.name == name);
			if (index >= 0) { AxisBinds.RemoveAt(index); return true; }
			return false;
		}
		public static bool SetEnableBind(List<AxBind> AxisBinds, string name, bool enable) {
			AxBind kBind = AxisBinds.Find(kb => kb.name == name);
			if (kBind != null) { kBind.disable = !enable; return true; }
			return false;
		}
		private void Start() { Init(AxisBinds); }
		private void OnEnable() { OnEnable(AxisBinds); }
		private void OnDisable() { OnDisable(AxisBinds); }
		public bool RemoveBind(string name) { return RemoveBind(AxisBinds, name); }
		public bool SetEnableBind(string name, bool enable) { return SetEnableBind(AxisBinds, name, enable); }
	}
}