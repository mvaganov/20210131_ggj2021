using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NonStandard.Inputs {
	public class InputBind {
		/// <summary>
		/// how to name this key binding in any user interface that pops up.
		/// </summary>
		public string name;
	}

	public class UserInput : MonoBehaviour {
		public List<KBind> keyBinds = new List<KBind>();
		public List<AxBind> axisBinds = new List<AxBind>();

		private void Start() {
			if (keyBinds.Count > 0) {
				for (int i = 0; i < keyBinds.Count; ++i) { keyBinds[i].Init(); }
			}
			if (axisBinds.Count > 0) {
				for (int i = 0; i < axisBinds.Count; ++i) { axisBinds[i].Init(); }
			}
		}

		private void OnEnable() {
			if(keyBinds.Count > 0 && !AppInput.HasKeyBind(keyBinds[0])) {
				for (int i = 0; i < keyBinds.Count; ++i) { AppInput.AddListener(keyBinds[i]); }
			}
			if(axisBinds.Count > 0 && !AppInput.HasAxisBind(axisBinds[0])) {
				for (int i = 0; i < axisBinds.Count; ++i) { AppInput.AddListener(axisBinds[i]); }
			}
		}

		private void OnDisable() {
			if (AppInput.IsQuitting) return;
			if (keyBinds.Count > 0 && AppInput.HasKeyBind(keyBinds[0])) {
				for (int i = 0; i < keyBinds.Count; ++i) { AppInput.RemoveListener(keyBinds[i]); }
			}
			if (axisBinds.Count > 0 && AppInput.HasAxisBind(axisBinds[0])) {
				for (int i = 0; i < axisBinds.Count; ++i) { AppInput.RemoveListener(axisBinds[i]); }
			}
		}

		public bool RemoveKeybind(string name) {
			int index = keyBinds.FindIndex(kb => kb.name == name);
			if (index >= 0) { keyBinds.RemoveAt(index); return true; }
			return false;
		}
		public bool RemoveAxis(string name) {
			int index = keyBinds.FindIndex(kb => kb.name == name);
			if (index >= 0) { keyBinds.RemoveAt(index); return true; }
			return false;
		}

		public bool SetEnableKeybind(string name, bool enable) {
			KBind kBind = keyBinds.Find(kb => kb.name == name);
			if(kBind != null) { kBind.disable = !enable; return true; }
			return false;
		}
		public bool SetEnableAxis(string name, bool enable) {
			KBind kBind = keyBinds.Find(kb => kb.name == name);
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
