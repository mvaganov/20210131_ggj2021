using NonStandard.Utility.UnityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Inputs {
	public class KeyInput : MonoBehaviour {
		public List<KBind> KeyBinds = new List<KBind>();

		public static void Init(IList<KBind> KeyBinds) {
			if (KeyBinds.Count > 0) {
				for (int i = 0; i < KeyBinds.Count; ++i) { KeyBinds[i].Init(); }
			}
		}

		public static void OnEnable(IList<KBind> KeyBinds) {
			if (KeyBinds.Count > 0 && !AppInput.HasKeyBind(KeyBinds[0])) {
				for (int i = 0; i < KeyBinds.Count; ++i) { AppInput.AddListener(KeyBinds[i]); }
			}
		}
		public static void OnDisable(IList<KBind> KeyBinds) {
			if (AppInput.IsQuitting) return;
			if (KeyBinds.Count > 0 && AppInput.HasKeyBind(KeyBinds[0])) {
				for (int i = 0; i < KeyBinds.Count; ++i) { AppInput.RemoveListener(KeyBinds[i]); }
			}
		}
		public static void Bind(IList<KBind> KeyBinds, KCode kCode, KModifier modifier, string name, string methodName, object value = null, object target = null) {
			KeyBinds.Add(new KBind(new KCombo(kCode, modifier), name, pressFunc: new EventBind(target, methodName, value)));
		}
		public static bool RemoveBind(List<KBind> KeyBinds, string name) {
			int index = KeyBinds.FindIndex(kb => kb.name == name);
			if (index >= 0) { KeyBinds.RemoveAt(index); return true; }
			return false;
		}
		public static bool SetEnableBind(List<KBind> KeyBinds, string name, bool enable) {
			KBind kBind = KeyBinds.Find(kb => kb.name == name);
			if (kBind != null) { kBind.disable = !enable; return true; }
			return false;
		}

		private void Start() { Init(KeyBinds); }
		private void OnEnable() { OnEnable(KeyBinds); }
		private void OnDisable() { OnDisable(KeyBinds); }
		public void KeyBind(KCode kCode, KModifier modifier, string name, string methodName, object value = null, object target = null) {
			Bind(KeyBinds, kCode, modifier, name, methodName, value, target);
		}
		public bool RemoveBind(string name) { return RemoveBind(KeyBinds, name); }
		public bool SetEnableBind(string name, bool enable) { return SetEnableBind(KeyBinds, name, enable); }
		public void ClickButton(UnityEngine.UI.Button button) { button.onClick.Invoke(); }
	}
}