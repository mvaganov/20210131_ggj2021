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

	public class UserInput : MonoBehaviour {
		public List<KBind> KeyBinds = new List<KBind>();
		public List<AxBind> AxisBinds = new List<AxBind>();

		private void Start() { KeyInput.Init(KeyBinds); AxisInput.Init(AxisBinds); }
		private void OnEnable() { KeyInput.OnEnable(KeyBinds); AxisInput.OnEnable(AxisBinds); }
		private void OnDisable() { KeyInput.OnDisable(KeyBinds); AxisInput.OnDisable(AxisBinds); }

		public void KeyBind(KCode kCode, KModifier modifier, string name, string methodName, object value = null, object target = null) {
			KeyInput.Bind(KeyBinds, kCode, modifier, name, methodName, value, target);
		}
		public bool RemoveKeyBind(string name) { return KeyInput.RemoveBind(KeyBinds, name); }
		public bool RemoveAxisBind(string name) { return AxisInput.RemoveBind(AxisBinds, name); }

		public bool SetEnableKeyBind(string name, bool enable) { return KeyInput.SetEnableBind(KeyBinds, name, enable); }
		public bool SetEnableAxisBind(string name, bool enable) { return AxisInput.SetEnableBind(AxisBinds, name, enable); }
	}
}
