using NonStandard.Process;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace NonStandard.Inputs {
	public class AppInput : UserInput {
		public enum KBindChange { None = 0, Add = 1, Remove = 2, Update = 3 }

		[TextArea(1, 30), SerializeField]
		protected string CurrentKeyBindings;
		public bool updateText = true;
		private bool textInputHappening = false;
		[HideInInspector] public bool debugPrintPossibleKeyConflicts = false;
		[HideInInspector] public bool debugPrintActivatedEvents = false;
		private KBindGroup[] keyBindGroups;
		public int updates { get; protected set; }
		public static bool IsQuitting { get; private set; }

		protected List<KBind> kBindPresses = new List<KBind>();
		protected List<KBind> kBindHolds = new List<KBind>();
		protected List<KBind> kBindReleases = new List<KBind>();

		/// <summary>
		/// not an array because all elements aren't managed by AppInput. Unity's KeyCode system does most of the work.
		/// </summary>
		private static readonly Dictionary<KCode, KState> _pressState = new Dictionary<KCode, KState>();

		public static void Log(string text) { Debug.Log(text); }
		public static void Log(object obj) { Log(obj.ToString()); }

		public static HashSet<KCode> heldKeys = new HashSet<KCode>();
		[Serializable] public class KeyRepeatRate {
			public static KeyRepeatRate instance;

			public KCode lastHeldKey;
			public static KCode softwareEmulatedPress;
			[NonSerialized] public ulong lastKeyHeldTime = 0;
			[NonSerialized] public ulong heldDuration = 0;
			public ulong initialDelay = 750;
			public ulong repeatDelay = 50;
			public KeyRepeatRate() { instance = this; }
			public void Update() {
				if (softwareEmulatedPress != KCode.None) { softwareEmulatedPress = KCode.None; }
				if (heldKeys.Count > 0) {
					KCode thisHeldKey = KCode.None;
					foreach (KCode k in heldKeys) { thisHeldKey = k; break; }
					heldKeys.Clear();
					if (thisHeldKey != KCode.None && (thisHeldKey < KCode.Mouse0 || thisHeldKey > KCode.Mouse6)) {
						ulong now = Proc.Now;
						if (thisHeldKey == lastHeldKey && lastKeyHeldTime != 0) {
							heldDuration += now - lastKeyHeldTime;
							if (heldDuration > initialDelay) {
								heldDuration -= repeatDelay;
								softwareEmulatedPress = thisHeldKey;
							}
						}
						lastKeyHeldTime = now;
						lastHeldKey = thisHeldKey;
					}
				} else {
					lastHeldKey = KCode.None;
					lastKeyHeldTime = 0;
					heldDuration = 0;
				}
			}
		}
		public KeyRepeatRate keyRepeatRate = new KeyRepeatRate();

		private static AppInput _instance;
		public static AppInput Instance {
			get {
				if (!Application.isPlaying) { throw new Exception("something is trying to create AppInput during edit time"); }
				if (_instance != null) return _instance;
				_instance = FindObjectOfType<AppInput>();
				if (_instance == null) {
					GameObject go = GetEventSystem().gameObject;
					_instance = go.AddComponent<AppInput>();
				}
				return _instance;
			}
		}
		public static EventSystem GetEventSystem() {
			EventSystem es = EventSystem.current;
			if (es == null) {
				GameObject evOb = new GameObject("EventSystem");
				es = evOb.AddComponent<EventSystem>();
				evOb.AddComponent<StandaloneInputModule>();
			}
			return es;
		}

		public static bool RemoveListener(string name) { return Instance.RemoveKeyBind(name); }
		public static bool RemoveListener(KBind kBind) { return Instance.RemoveKeyBind(kBind); }
		public static bool RemoveListener(AxBind axBind) { return Instance.RemoveAxisBind(axBind); }
		public static bool AddListener(AxBind axBind) { return Instance.AddAxisBind(axBind); }
		public static bool AddListener(KBind kBind) { return Instance.AddKeyBind(kBind); }
		public static bool AddListener(KCode key, Func<bool> whatToDo, string name) { return AddListener(new KBind(key, whatToDo, name)); }

		public static bool HasKeyBind(string name) {
			if (string.IsNullOrEmpty(name)) return false;
			int index = Instance.KeyBinds.FindIndex(kb => kb.name == name);
			return index >= 0;
		}

		public static bool HasAxisBind(string name) {
			if (string.IsNullOrEmpty(name)) return false;
			int index = Instance.AxisBinds.FindIndex(kb => kb.name == name);
			return index >= 0;
		}

		public static bool HasKeyBind(KBind kBind) {
			int index = kBind != null ? Instance.KeyBinds.IndexOf(kBind) : -1;
			return index >= 0;
		}

		public static bool HasAxisBind(AxBind axBind) {
			int index = axBind != null ? Instance.AxisBinds.IndexOf(axBind) : -1;
			return index >= 0;
		}

		public bool RemoveKeyBind(KBind kBind) {
			int index = KeyBinds.IndexOf(kBind);
			if (index < 0) return false;
			return RemoveListener(kBind, index);
		}

		public bool RemoveAxisBind(AxBind axBind) {
			int index = AxisBinds.IndexOf(axBind);
			if (index < 0) return false;
			axBind.DoAxis(0);
			AxisBinds.RemoveAt(index);
			if (updateText) { UpdateCurrentKeyBindText(); }
			return true;
		}

		private bool RemoveListener(KBind kBind, int kBindIndex) {
			KeyBinds.RemoveAt(kBindIndex);
			UpdateKeyBindGroups(kBind, KBindChange.Remove);
			return true;
		}

		public bool AddKeyBind(KBind kBind) {
			int index = (!string.IsNullOrEmpty(kBind.name)) ? KeyBinds.FindIndex(kb => kb.name == kBind.name) : -1;
			KBindChange kindOfChange = KBindChange.Add;
			if (index >= 0) {
				kindOfChange = KBindChange.Update; // will cause lists to Remove then re-Add
				return false;
			} else {
				KeyBinds.Add(kBind);
			}
			return UpdateKeyBindGroups(kBind, kindOfChange);
		}

		public bool AddAxisBind(AxBind axisBind) {
			int index = (!string.IsNullOrEmpty(axisBind.name)) ? KeyBinds.FindIndex(kb => kb.name == axisBind.name) : -1;
			if (index < 0) {
				AxisBinds.Add(axisBind);
				if (updateText) { UpdateCurrentKeyBindText(); }
				return true;
			}
			return false;
		}

		private static bool UpdateLists(KBind kBind, KBindChange change) {
			return Instance.UpdateKeyBindGroups(kBind, change);
		}

		/// <summary>
		/// used to add/remove/update a specific <see cref="KBind"/>
		/// </summary>
		/// <param name="kBind"></param>
		/// <param name="add"></param>
		private bool UpdateKeyBindGroups(KBind kBind, KBindChange change) {
			bool changeHappened = false;
			kBind.Init();
			//if(kBind.keyCombinations != null && kBind.keyCombinations[0].modifiers != null)
			//	Log(kBind.keyCombinations[0].modifiers[0]);
			//Log(kBind);
			EnsureInitializedKeyBindGroups();
			if (change == KBindChange.Update) {
				changeHappened |= UpdateKeyBindGroups(kBind, KBindChange.Remove);
				change = KBindChange.Add;
			}
			for (int k = 0; k < keyBindGroups.Length; ++k) {
				KBindGroup group = keyBindGroups[k];
				changeHappened |= group.UpdateKeyBinding(kBind, change);
			}
			if (updateText && changeHappened) {
				UpdateCurrentKeyBindText();
			}
			return changeHappened;
		}

		public string DebugBindings(IList<KBind> list) {
			StringBuilder output = new StringBuilder();
			for (int i = 0; i < list.Count; ++i) {
				if (i > 0) output.Append(", ");
				output.Append(list[i].name).Append("[");
				for (int k = 0; k < list[i].keyCombinations.Length; ++k) {
					if (k > 0) output.Append(", ");
					output.Append(list[i].keyCombinations[k].key);
				}
				output.Append("]");
			}
			return output.ToString();
		}

		public static void BeginIgnoreKeyBinding(string text) { Instance.textInputHappening = true; }
		public static void EndIgnoreKeyBinding(string text) { Instance.textInputHappening = false; }
		protected static UnityAction<string> s_BeginIgnoreKeyBinding = BeginIgnoreKeyBinding;
		protected static UnityAction<string> s_EndIgnoreKeyBinding = EndIgnoreKeyBinding;

		//public void TextInputDisablesAppInput(TMPro.TMP_InputField _inputField) {
		//	_inputField.onSelect.AddListener( s_BeginIgnoreKeyBinding );
		//	_inputField.onDeselect.AddListener( s_EndIgnoreKeyBinding );
		//}

		public static Vector3 MousePosition {
			get {
				#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				return Mouse.current.position.ReadValue();
				#else
				return Input.mousePosition;
				#endif
			}
		}

		public static Vector3 MousePositionDelta {
			get {
				#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				return Mouse.current.delta.ReadValue();
				#else
				return new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
				#endif
			}
		}
		public static Vector3 MouseScrollDelta {
			get {
				#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				return Mouse.current.scroll.ReadValue();
				#else
				return new Vector3(0, Input.GetAxis("Mouse ScrollWheel"));
				#endif
			}
		}
		public static bool IsOldKeyCode(KCode code) { return Enum.IsDefined(typeof(KeyCode), (int)code); }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		private static bool GetKey_internal_InputSystem(KCode key) {
			switch (key) {
				case KCode.NoShift: return !Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.rightShiftKey.isPressed;
				case KCode.NoAlt: return !Keyboard.current.leftAltKey.isPressed && !Keyboard.current.rightAltKey.isPressed;
				case KCode.NoCtrl: return !Keyboard.current.leftCtrlKey.isPressed && !Keyboard.current.rightCtrlKey.isPressed;
				case KCode.AnyShift: return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;;
				case KCode.AnyAlt: return Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed;
				case KCode.AnyCtrl: return Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
				case KCode.MouseWheelUp: return Mouse.current.scroll.y.ReadValue() > 0;
				case KCode.MouseWheelDown: return Mouse.current.scroll.y.ReadValue() < 0;
				case KCode.MouseXUp: return Mouse.current.delta.x.ReadValue() > 0;
				case KCode.MouseXDown: return Mouse.current.delta.x.ReadValue() < 0;
				case KCode.MouseYUp: return Mouse.current.delta.y.ReadValue() > 0;
				case KCode.MouseYDown: return Mouse.current.delta.y.ReadValue() < 0;
			}
			return false;
		}
		private static bool TryGetMouseButton(KCode key, out ButtonControl buttonControl) {
			switch (key) {
				case KCode.Mouse0: buttonControl = Mouse.current.leftButton; return true;
				case KCode.Mouse1: buttonControl = Mouse.current.rightButton; return true;
				case KCode.Mouse2: buttonControl = Mouse.current.middleButton; return true;
				case KCode.Mouse3: buttonControl = Mouse.current.forwardButton; return true; // TODO testme
				case KCode.Mouse4: buttonControl = Mouse.current.backButton; return true; // TODO testme
			}
			buttonControl = null;
			return false;
		}
		private static bool TryGetKeyButton(KCode key, out ButtonControl buttonControl) {
			if (!KCodeExtensionUnity.kCodeToInputSystem.TryGetValue(key, out UnityEngine.InputSystem.Key k)) {
				buttonControl = null;
				return false;
			}
			buttonControl = Keyboard.current[k];
			return true;
		}
#else
		private static bool GetKey_internal_legacy(KCode key) {
			switch (key) {
				case KCode.NoShift: return !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift);
				case KCode.NoCtrl: return !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl);
				case KCode.NoAlt: return !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt);
				case KCode.AnyAlt: return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
				case KCode.AnyCtrl: return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
				case KCode.AnyShift: return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
				case KCode.MouseWheelUp: return Input.GetAxis("Mouse ScrollWheel") > 0;
				case KCode.MouseWheelDown: return Input.GetAxis("Mouse ScrollWheel") < 0;
				case KCode.MouseXUp: return Input.GetAxis("Mouse X") > 0;
				case KCode.MouseXDown: return Input.GetAxis("Mouse X") < 0;
				case KCode.MouseYUp: return Input.GetAxis("Mouse Y") > 0;
				case KCode.MouseYDown: return Input.GetAxis("Mouse Y") < 0;
				// default: throw new Exception($"can't handle {key}");
			}
			return false;
		}
#endif
		private static bool GetKey_internal(KCode key) {
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			return GetKey_internal_InputSystem(key);
			#else
			return GetKey_internal_legacy(key);
			#endif
		}

		private List<KCode> advanceToPressed = new List<KCode>();
		private List<KCode> advanceToReleased = new List<KCode>();
		public void LateUpdate() {
			// figure out which keys have just been pressed or released, and adjust those states
			advanceToPressed.Clear();
			advanceToReleased.Clear();
			foreach (KeyValuePair<KCode, KState> kvp in _pressState) {
				switch (kvp.Value) {
				case KState.KeyDown: advanceToPressed.Add(kvp.Key); break;
				case KState.KeyUp: advanceToReleased.Add(kvp.Key); break;
				}
			}
			// values must be set out of dictionary traversal because of collection rules
			for (int i = 0; i < advanceToPressed.Count; ++i) { _pressState[advanceToPressed[i]] = KState.KeyHeld; }
			for (int i = 0; i < advanceToReleased.Count; ++i) { _pressState[advanceToReleased[i]] = KState.KeyReleased; }
		}

		public static bool GetKey(KCode key) {
			bool pressed;
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			if (TryGetKeyButton(key, out ButtonControl keyControl) || TryGetMouseButton(key, out keyControl)) {
				pressed = keyControl.isPressed;
				if (pressed) { heldKeys.Add(key); }
				return pressed;
			}
			#else
			if (IsOldKeyCode(key)) {
				pressed = UnityEngine.Input.GetKey((KeyCode) key);
				if (pressed && (key < KCode.NoShift || key > KCode.NoAlt)) { heldKeys.Add(key); }
				return pressed;
			}
			#endif
			pressed = GetKey_internal(key);
			if (pressed && (key < KCode.NoShift || key > KCode.NoAlt)) { heldKeys.Add(key); }
			KState ks;
			_pressState.TryGetValue(key, out ks);
			if (pressed && ks == KState.KeyReleased) { _pressState[key] = KState.KeyDown; }
			if (!pressed && ks == KState.KeyHeld) { _pressState[key] = KState.KeyUp; }
			return pressed;
		}

		public static bool GetKeyDown(KCode key) {
			if (KeyRepeatRate.softwareEmulatedPress == key) return true;
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			if (TryGetKeyButton(key, out ButtonControl keyControl) || TryGetMouseButton(key, out keyControl)) {
				//if (keyControl.wasPressedThisFrame) Debug.Log("checking "+key+" "+keyControl.wasPressedThisFrame+" "+keyControl.name+" "+keyControl.displayName+" "+keyControl.shortDisplayName);
				return keyControl.wasPressedThisFrame;
			}
			#else
			if (IsOldKeyCode(key)) { return UnityEngine.Input.GetKeyDown((KeyCode)key); }
			#endif
			KState ks;
			_pressState.TryGetValue(key, out ks);
			if (ks == KState.KeyHeld || ks == KState.KeyUp) { return false; }
			bool pressed = GetKey_internal(key);
			if (pressed && ks == KState.KeyReleased) { _pressState[key] = KState.KeyDown; }
			return pressed;
		}

		public static bool GetKeyUp(KCode key) {
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			if (TryGetKeyButton(key, out ButtonControl keyControl) || TryGetMouseButton(key, out keyControl)) {
				return keyControl.wasReleasedThisFrame;
			}
			#else
			if (IsOldKeyCode(key)) { return UnityEngine.Input.GetKeyUp((KeyCode)key); }
			#endif
			KState ks;
			_pressState.TryGetValue(key, out ks);
			if (ks == KState.KeyReleased || ks == KState.KeyDown) { return false; }
			bool pressed = GetKey_internal(key);
			if (!pressed && ks == KState.KeyHeld) { _pressState[key] = KState.KeyReleased; }
			return !pressed;
		}

		public enum StandardAxis { None, Horizontal, Vertical, MouseX, MouseY, MouseScrollY }
		/// <summary>
		/// TODO reimplement the input code to be truly event-driven, and remove the need for methods that do polling like these
		/// </summary>
		/// <param name="axis"></param>
		/// <returns></returns>
		public static float GetStandardAxis(StandardAxis axis) {
			float v = 0;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			switch (axis) {
			case StandardAxis.Horizontal:
				if (GetKey(KCode.A) || GetKey(KCode.LeftArrow)) v += -1;
				if (GetKey(KCode.D) || GetKey(KCode.RightArrow)) v += 1;
				break;
			case StandardAxis.Vertical:
				if (GetKey(KCode.S) || GetKey(KCode.UpArrow)) v += -1;
				if (GetKey(KCode.W) || GetKey(KCode.DownArrow)) v += 1;
				break;
			case StandardAxis.MouseX: v = MousePositionDelta.x; break;
			case StandardAxis.MouseY: v = MousePositionDelta.y; break;
			case StandardAxis.MouseScrollY: v = MouseScrollDelta.y; break;
			}
#else
			switch (axis) {
			case StandardAxis.Horizontal: v = Input.GetAxis("Horizontal");       break;
			case StandardAxis.Vertical:   v = Input.GetAxis("Vertical");         break;
			case StandardAxis.MouseX:     v = Input.GetAxis("Mouse X");          break;
			case StandardAxis.MouseY:     v = Input.GetAxis("Mouse Y");          break;
			case StandardAxis.MouseScroll:v = Input.GetAxis("Mouse ScrollWheel");break;
			}
#endif
			return v;
		}

		// TODO use this to get inputs instead of the awkward polling mechanism
		private void EventHandler(UnityEngine.InputSystem.LowLevel.InputEventPtr eventPtr, UnityEngine.InputSystem.InputDevice device) {
			if (!eventPtr.IsA<UnityEngine.InputSystem.LowLevel.StateEvent>() && !eventPtr.IsA<UnityEngine.InputSystem.LowLevel.DeltaStateEvent>()) {
				return;
			}
			foreach (UnityEngine.InputSystem.InputControl control in eventPtr.EnumerateChangedControls(device)) {
				Debug.Log(control.displayName+" : "+control.ReadValueAsObject());
			}
		}
		public void Start() {
			UnityEngine.InputSystem.InputSystem.onEvent += EventHandler;
		}
		public void Update() { DoUpdate(); }

		bool IsKeyBindAmbiguousWithTextInput(KBind kBind) {
			for (int i = 0; i < kBind.keyCombinations.Length; ++i) {
				bool isSimpleKeyPress = kBind.keyCombinations[i].modifiers == null || kBind.keyCombinations[i].modifiers.Length == 0;
				//bool shiftModified = !isSimpleKeyPress && kBind.keyCombinations[i].modifiers[0].key == KCode.LeftShift;
				bool shiftModified = !isSimpleKeyPress && kBind.keyCombinations[i].modifiers[0] == KModifier.AnyShift;
				bool isFunctionKey = (kBind.keyCombinations[i].key >= KCode.F1 && kBind.keyCombinations[i].key <= KCode.F15);
				bool couldInterfereWithKeyboardInput = (isSimpleKeyPress && !isFunctionKey) || (shiftModified && !isFunctionKey);
				if (couldInterfereWithKeyboardInput) return true;
			}
			return false;
		}

		[Serializable]
		public class KBindGroup {
			public string name;
			public List<KBind> keyBindList;
			public List<KeyTrigger> triggerList = new List<KeyTrigger>();
			/// <summary>
			/// could be <see cref="KBind.GetDown"/>, <see cref="KBind.GetHeld"/>, or <see cref="KBind.GetUp"/>
			/// </summary>
			public Func<KBind, KCombo> trigger;
			/// <summary>
			/// should return true if the action was successful (prevents other key events with the same keycode from triggering)
			/// </summary>
			public Func<KBind, bool> action;
			/// <summary>
			/// a filter that prevents key bindings from entering this list
			/// </summary>
			public Func<KBind, bool> putInList;

			/// <summary>
			/// we want to know what <see cref="KBind"/> was triggered, including which specific <see cref="KCombo"/> did the triggerings
			/// </summary>
			public struct KeyTrigger : IComparable<KeyTrigger> {
				public KBind kb;
				/// <summary>
				/// which (of the possibly many) keypress triggered the key mapping to activate
				/// </summary>
				public KCombo kp;
				/// sort by keypress
				public int CompareTo(KeyTrigger other) { return kp.CompareTo(other.kp); }
			}

			public void Update(Func<KBind, bool> additionalFilter = null) {
				for (int i = 0; i < keyBindList.Count; ++i) {
					KeyCheck(keyBindList[i], triggerList, additionalFilter);
				}
			}

			/// <param name="kBind"></param>
			/// <param name="kind"></param>
			/// <returns></returns>
			public bool UpdateKeyBinding(KBind kBind, KBindChange kind) {
				if (kind == KBindChange.Add && putInList != null && !putInList.Invoke(kBind)) { return false; }
				bool changeHappened = false;
				int index = keyBindList.IndexOf(kBind);
				switch (kind) {
				case KBindChange.Add:
					if (index >= 0) { Log("already added " + name + " " + kBind.name + "?"); }
					if (index < 0) {
						keyBindList.Add(kBind);
						keyBindList.Sort();
						changeHappened = true;
						//Log($"added {name} {kBind.name}");
					} else { if (index >= 0) { Log("will not add duplicate " + name + " " + kBind.name); } }
					break;
				case KBindChange.Remove:
					if (index >= 0) { keyBindList.RemoveAt(index); changeHappened = true; }
					break;
				case KBindChange.Update:
					throw new Exception("Update is composed of a Remove and Add, should never be called directly like this.");
				}
				return changeHappened;
			}

			/// <param name="kb">check if this key bind is being triggered</param>
			/// <param name="list">where to mark if this is indeed triggered</param>
			/// <param name="additionalFilter">an additional gate that might prevent this particluar keybind from triggering. possibly heavy method, so only checked if the key is triggered</param>
			bool KeyCheck(KBind kb, List<KeyTrigger> list, Func<KBind, bool> additionalFilter = null) {
				KCombo kp = trigger.Invoke(kb);
				if (kp != null && (additionalFilter == null || !additionalFilter.Invoke(kb))) {
					//Debug.Log("triggered: " + kp + " -> " + kb.name);
					list.Add(new KeyTrigger { kb = kb, kp = kp });
					return true;
				}
				return false;
			}

			/// <summary>
			/// keeps one event consumed for each key (pressing w and a should trigger both w and a, not just the first one)
			/// </summary>
			private static Dictionary<KCode, KBind> s_eventConsumed = new Dictionary<KCode, KBind>();
			/// <summary>
			/// resolves key conflicts (giving priority to more complex key presses first) before invoking all triggered keys
			/// </summary>
			public void Resolve(bool showConflict, bool logActivatedKeyBinds) {
				if (triggerList.Count <= 0) { return; }
				// sort by KCode in the triggering ComplexKeyPress, with the most complex first
				triggerList.Sort();
				// if there are multiple keybinds with the same kcode
				for (int a = 0; a < triggerList.Count; ++a) {
					int conflictHere = -1;
					for (int b = a + 1; b < triggerList.Count; ++b) {
						if (triggerList[a].kp.key == triggerList[b].kp.key) {
							conflictHere = b;
						}
					}
					// go through all of those keybinds
					if (conflictHere != -1) {
						string debugOutput = showConflict ? "possible " + name + " conflict" : "";
						int complexityToKeep = triggerList[a].kp.GetComplexity();
						// if a keybind's modifiers are all fulfilled by a more complex keybind, ignore this keybind (remove from list) 
						for (int i = a; i <= conflictHere; ++i) {
							KBind kb = triggerList[i].kb;
							if (showConflict) { debugOutput += "\n" + kb + kb.priority; }
							if (triggerList[i].kp.GetComplexity() < complexityToKeep) {
								triggerList.RemoveAt(i);
								if (showConflict) { debugOutput += "[REMOVED]"; }
							}
						}
						if (showConflict) { Log(debugOutput); }
					}
				}

				string debugText = null;
				if (logActivatedKeyBinds) { debugText = name + " activating: "; }

				// trigger everything left in the list
				for (int i = 0; i < triggerList.Count; ++i) {
					KBind kb = triggerList[i].kb;
					KBind eventConsumed;
					s_eventConsumed.TryGetValue(triggerList[i].kp.key, out eventConsumed);
					if (logActivatedKeyBinds) {
						if (i > 0) debugText += ", ";
						debugText += kb.name;
					}
					bool activated = false;
					// invoke non-consumptive events, or consumptive events as long as the event is not consumed
					if (eventConsumed == null || kb.alwaysTriggerable) {
						activated = action.Invoke(kb);
					}
					// high-priority key mappings that consume events should prevent future events that also consume.
					if (!kb.alwaysTriggerable && activated) {
						//Log($"{kb.name} consumed {name}");
						s_eventConsumed[triggerList[i].kp.key] = kb;
					}
				}
				if (logActivatedKeyBinds) {
					Log(debugText);
				}
				s_eventConsumed.Clear();
				triggerList.Clear();
			}
		}
		public void Awake() {
			EnsureInitializedKeyBindGroups();
		}
		void OnApplicationQuit() {
			IsQuitting = true;
			string c = "color", a = "#84f", b = "#48f";
			Log("<" + c + "=" + a + ">AppInput</" + c + ">.IsQuitting = <" + c + "=" + b + ">true</" + c + ">;");
		}

		public void EnsureInitializedKeyBindGroups() {
			if (keyBindGroups != null) return;
			keyBindGroups = new KBindGroup[] {
				new KBindGroup{name="Press",  keyBindList=kBindPresses, trigger=kb=>kb.GetDown(),action=kb=>kb.DoPress(),  putInList=kb=>kb.keyEvent.CountPress>0  },
				new KBindGroup{name="Hold",   keyBindList=kBindHolds,   trigger=kb=>kb.GetHeld(),action=kb=>kb.DoHold(),   putInList=kb=>kb.keyEvent.CountHold>0   },
				new KBindGroup{name="Release",keyBindList=kBindReleases,trigger=kb=>kb.GetUp(),  action=kb=>kb.DoRelease(),putInList=kb=>kb.keyEvent.CountRelease>0},
			};
		}

		public void DoUpdate() {
			if (!textInputHappening) {
				Array.ForEach(keyBindGroups, ks => ks.Update());
			} else {
				Array.ForEach(keyBindGroups, ks => ks.Update(IsKeyBindAmbiguousWithTextInput));
			}
			Array.ForEach(keyBindGroups, ks => ks.Resolve(debugPrintPossibleKeyConflicts, debugPrintActivatedEvents));
			for (int i = 0; i < AxisBinds.Count; ++i) {
				AxisBinds[i].Update();
			}
			++updates;
			keyRepeatRate.Update();
		}
		public void UpdateCurrentKeyBindText() {
			EnsureInitializedKeyBindGroups();
			StringBuilder sb = new StringBuilder();
			for (int s = 0; s < keyBindGroups.Length; ++s) {
				KBindGroup ks = keyBindGroups[s];
				if (ks.keyBindList.Count == 0) continue;
				sb.Append("[" + ks.name + "]\n");
				for (int i = 0; i < ks.keyBindList.Count; ++i) {
					KBind kb = ks.keyBindList[i];
					bool needsPriority = true;
					bool hasKeys = true;
					if (kb.keyCombinations.Length != 0 && (kb.keyCombinations.Length != 1 || kb.keyCombinations[0].key != KCode.None)) {
						KCombo theseKeys = kb.keyCombinations[0];
						bool hasPrev = i > 0;
						bool hasNext = i < ks.keyBindList.Count - 1;
						KBind prev = (hasPrev) ? ks.keyBindList[i - 1] : null;
						KBind next = (hasNext) ? ks.keyBindList[i + 1] : null;
						KCombo prevKeys = hasPrev && prev.keyCombinations.Length > 0 ? prev.keyCombinations[0] : null;
						KCombo nextKeys = hasNext && next.keyCombinations.Length > 0 ? next.keyCombinations[0] : null;
						needsPriority = (prevKeys != null && prevKeys.CompareTo(theseKeys) == 0 ||
							nextKeys != null && nextKeys.CompareTo(theseKeys) == 0);
					} else {
						hasKeys = false;
					}
					if (hasKeys) {
						sb.Append(kb.ShortDescribe(" | "));
					} else {
						sb.Append("(no keys)");
					}
					sb.Append(" :");
					if (needsPriority) { sb.Append(kb.priority.ToString()); }
					sb.Append(": ");
					sb.Append(kb.name);
					sb.Append("\n");
				}
			}
			if (AxisBinds.Count > 0) {
				sb.Append("[Axis]\n");
				for (int i = 0; i < AxisBinds.Count; ++i) {
					AxBind ab = AxisBinds[i];
					sb.Append(ab.ShortDescribe(" | "));
					sb.Append(" :: ");
					sb.Append(ab.name);
					sb.Append("\n");
				}
			}
			CurrentKeyBindings = sb.ToString();
		}
	}
}