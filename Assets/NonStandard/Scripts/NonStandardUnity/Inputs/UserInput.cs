using NonStandard.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Inputs {
	public class InputBind {
		/// <summary>
		/// how to name this key binding in any user interface that pops up.
		/// </summary>
		public string name;
	}

	// TODO put convenience functions in here for UnityEvents
	public class UserInput : MonoBehaviour {
		public List<KBind> KeyBinds = new List<KBind>();
		public List<AxBind> AxisBinds = new List<AxBind>();

		private void Start() {
			if (KeyBinds.Count > 0) {
				for (int i = 0; i < KeyBinds.Count; ++i) { KeyBinds[i].Init(); }
			}
			if (AxisBinds.Count > 0) {
				for (int i = 0; i < AxisBinds.Count; ++i) { AxisBinds[i].Init(); }
			}
		}

		private void OnEnable() {
			if(KeyBinds.Count > 0 && !AppInput.HasKeyBind(KeyBinds[0])) {
				for (int i = 0; i < KeyBinds.Count; ++i) { AppInput.AddListener(KeyBinds[i]); }
			}
			if(AxisBinds.Count > 0 && !AppInput.HasAxisBind(AxisBinds[0])) {
				for (int i = 0; i < AxisBinds.Count; ++i) { AppInput.AddListener(AxisBinds[i]); }
			}
		}

		private void OnDisable() {
			if (AppInput.IsQuitting) return;
			if (KeyBinds.Count > 0 && AppInput.HasKeyBind(KeyBinds[0])) {
				for (int i = 0; i < KeyBinds.Count; ++i) { AppInput.RemoveListener(KeyBinds[i]); }
			}
			if (AxisBinds.Count > 0 && AppInput.HasAxisBind(AxisBinds[0])) {
				for (int i = 0; i < AxisBinds.Count; ++i) { AppInput.RemoveListener(AxisBinds[i]); }
			}
		}

		public void KeyBind(KCode kCode, KModifier modifier, string name, string methodName, object value = null, object target = null) {
			if (target == null) target = this;
			KeyBinds.Add(new KBind(new KCombo(kCode, modifier), name, pressFunc: new EventBind(target, methodName, value)));
		}

		public bool RemoveKeybind(string name) {
			int index = KeyBinds.FindIndex(kb => kb.name == name);
			if (index >= 0) { KeyBinds.RemoveAt(index); return true; }
			return false;
		}
		public bool RemoveAxis(string name) {
			int index = KeyBinds.FindIndex(kb => kb.name == name);
			if (index >= 0) { KeyBinds.RemoveAt(index); return true; }
			return false;
		}

		public bool SetEnableKeybind(string name, bool enable) {
			KBind kBind = KeyBinds.Find(kb => kb.name == name);
			if(kBind != null) { kBind.disable = !enable; return true; }
			return false;
		}
		public bool SetEnableAxis(string name, bool enable) {
			KBind kBind = KeyBinds.Find(kb => kb.name == name);
			if (kBind != null) { kBind.disable = !enable; return true; }
			return false;
		}

		public void ForEachUserInput(Action<UserInput> action) {
			Array.ForEach(GetComponents<UserInput>(), ui => action(ui));
		}
		public void DisableKeybind(string name) { ForEachUserInput(ui=>ui.SetEnableKeybind(name, false)); }
		public void EnableKeybind(string name) { ForEachUserInput(ui => ui.SetEnableKeybind(name, true)); }
		public void DisableAxis(string name) { ForEachUserInput(ui => ui.SetEnableAxis(name, false)); }
		public void EnableAxis(string name) { ForEachUserInput(ui => ui.SetEnableAxis(name, true)); }
		public void RemoveAnyKeybind(string name) { ForEachUserInput(ui => ui.RemoveKeybind(name)); }
		public void RemoveAnyAxis(string name) { ForEachUserInput(ui => ui.RemoveAxis(name)); }
	}
}
