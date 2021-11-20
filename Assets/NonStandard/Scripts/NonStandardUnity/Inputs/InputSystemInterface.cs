using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace NonStandard.Inputs {
	public class InputSystemInterface : MonoBehaviour {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		InputSystemInterfaceLogic isInterface;

		public Dictionary<KCode, InputSystemInterfaceLogic.SpecificControlHandler> OnPressed => isInterface.OnPressed;
		public Dictionary<KCode, InputSystemInterfaceLogic.SpecificControlHandler> OnRelease => isInterface.OnRelease;
		public Dictionary<KCode, InputSystemInterfaceLogic.SpecificControlHandler> OnPressing => isInterface.OnPressing;
		public InputSystemInterfaceLogic.SpecificControlHandler OnPressedAny => isInterface.OnPressedAny;
		public InputSystemInterfaceLogic.SpecificControlHandler OnReleaseAny => isInterface.OnReleaseAny;
		public InputSystemInterfaceLogic.SpecificControlHandler OnPressingAny => isInterface.OnPressingAny;
		void Awake() {
			isInterface = new InputSystemInterfaceLogic();
			isInterface.Initialize();
		}
		void OnDestroy() { isInterface.Release(); }
		void Update() { isInterface.Update(); }
#endif
	}
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
	public class InputSystemInterfaceLogic {
		/// <summary>
		/// Delegate for controller input handling
		/// </summary>
		/// <param name="control"></param>
		/// <returns>true if the input was handled and consumed, false if not (which means the event should be passed to other handlers)</returns>
		public delegate void DeviceControlHandler(object control);
		public delegate void SpecificControlHandler(KCode kCode, object control);

		/// <summary>
		/// list of input handlers, where the integer device ID is the index in the list
		/// </summary>
		private List<Dictionary<Type, DeviceControlHandler>> deviceInputHandler = new List<Dictionary<Type, DeviceControlHandler>>();
		public Dictionary<KCode, SpecificControlHandler> OnPressed = new Dictionary<KCode, SpecificControlHandler>();
		public Dictionary<KCode, SpecificControlHandler> OnRelease = new Dictionary<KCode, SpecificControlHandler>();
		public Dictionary<KCode, SpecificControlHandler> OnPressing = new Dictionary<KCode, SpecificControlHandler>();

		public SpecificControlHandler OnPressedAny;
		public SpecificControlHandler OnReleaseAny;
		public SpecificControlHandler OnPressingAny;

		/// <summary>
		/// which controls are active right now, by their keycode
		/// </summary>
		public Dictionary<KCode, object> activeControl = new Dictionary<KCode, object>();

		private void RootEventHandler(InputEventPtr eventPtr, InputDevice device) {
			// only spend time on input events
			if (!eventPtr.IsA<StateEvent>()
			&& !eventPtr.IsA<DeltaStateEvent>()) {
				return;
			}
			// fail if the input device is unknown to the input handler
			if (eventPtr.deviceId >= deviceInputHandler.Count) {
				throw new Exception("unknown device id " + eventPtr.deviceId + ", not expecting " + device);
			}
			Dictionary<Type, DeviceControlHandler> deviceHandler = deviceInputHandler[eventPtr.deviceId];
			// go through all of the inputs
			foreach (InputControl control in eventPtr.EnumerateChangedControls(device)) {
				// execute the input handler for the device
				if (!deviceHandler.TryGetValue(control.GetType(), out DeviceControlHandler handler)) {
					throw new Exception("unable to handle " + control.GetType());
				}
				handler.Invoke(control);
			}
		}
		public void Update() {
			foreach (KeyValuePair<KCode, object> kvp in activeControl) {
				if (OnPressing.TryGetValue(kvp.Key, out SpecificControlHandler handler)) {
					handler.Invoke(kvp.Key, kvp.Value);
				}
				OnPressingAny?.Invoke(kvp.Key, kvp.Value);
			}
		}

		/// <summary>
		/// an abstraction layer to catch inconsistencies with presses and releases. ignores bad presses and bad releases
		/// </summary>
		/// <param name="control"></param>
		/// <param name="kCode"></param>
		/// <returns>true if there are no errors</returns>
		public bool PressAndReleaseUpdate(ButtonControl control, KCode kCode) {
			if (AppInput.IsQuitting) return true;
			bool isRelease = control.IsPressed();
			int k = (int)kCode;
			if (isRelease) {
				if (!activeControl.ContainsKey(kCode)) { Debug.Log("double release " + control + "? " + kCode); } else {
					//Debug.Log("good release");
					if (OnRelease.TryGetValue(kCode, out SpecificControlHandler handler)) { handler.Invoke(kCode, control); }
					activeControl.Remove(kCode);
					OnReleaseAny?.Invoke(kCode, control);
				}
			} else {
				if (activeControl.ContainsKey(kCode)) { Debug.Log("double press " + control + "? " + kCode); } else {
					//Debug.Log("good press");
					if (OnPressed.TryGetValue(kCode, out SpecificControlHandler handler)) { handler.Invoke(kCode, control); }
					activeControl[kCode] = control;
					OnPressedAny?.Invoke(kCode, control);
				}
			}
			return true;
		}
		public bool AxisUpdate(AxisControl control, KCode kCode) {
			if (AppInput.IsQuitting) return true;
			bool isRelease = control.IsPressed();
			int k = (int)kCode;
			if (isRelease) {
				if (OnRelease.TryGetValue(kCode, out SpecificControlHandler handler)) { handler.Invoke(kCode, control); }
				activeControl.Remove(kCode);
			} else {
				if (OnPressed.TryGetValue(kCode, out SpecificControlHandler handler)) { handler.Invoke(kCode, control); }
				activeControl[kCode] = control;
			}
			return true;
		}

		private void HandleKeyControl(object keyControl) {
			KeyControl kc = (KeyControl)keyControl;
			//Debug.Log(kc.keyCode);
			PressAndReleaseUpdate(kc, KCodeExtensionUnity.GetInputCode(kc));
		}
		private void HandleMouseButtonControl(object mouseButtonControl) {
			ButtonControl bc = (ButtonControl)mouseButtonControl;
			//Debug.Log(bc.shortDisplayName);
			PressAndReleaseUpdate(bc, KCodeExtensionUnity.GetInputCode(bc));
		}
		private void HandleMouseAxisControl(object mouseAxisControl) {
			AxisControl ac = (AxisControl)mouseAxisControl;
			//Debug.Log(ac.shortDisplayName);
			AxisUpdate(ac, KCodeExtensionUnity.GetInputCode(ac));
		}
		private int keyboardDeviceId, mouseDeviceId;
		private bool _inputSystemInitialized = false;
		public void Initialize() {
			if (_inputSystemInitialized) return;
			_inputSystemInitialized = true;
			for (int i = 0; i < InputSystem.devices.Count; ++i) {
				//Debug.Log(InputSystem.devices[i].deviceId + " " + InputSystem.devices[i].GetType() +" " +InputSystem.devices[i].shortDisplayName+" "+InputSystem.devices[i].Stringify());
				if (InputSystem.devices[i] is Keyboard) { keyboardDeviceId = InputSystem.devices[i].deviceId; }
				if (InputSystem.devices[i] is Mouse) { mouseDeviceId = InputSystem.devices[i].deviceId; }
			}
			int max = Math.Max(keyboardDeviceId, mouseDeviceId);
			deviceInputHandler.Capacity = max + 1;
			for (int i = 0; i <= max; ++i) { deviceInputHandler.Add(null); }
			deviceInputHandler[keyboardDeviceId] = new Dictionary<Type, DeviceControlHandler>() {
				[typeof(KeyControl)] = HandleKeyControl
			};
			deviceInputHandler[mouseDeviceId] = new Dictionary<Type, DeviceControlHandler>() {
				[typeof(ButtonControl)] = HandleMouseButtonControl,
				[typeof(AxisControl)] = HandleMouseAxisControl,
			};
			InputSystem.onEvent += RootEventHandler;
		}
		public void Release() {
			_inputSystemInitialized = false;
			deviceInputHandler.Clear();
			InputSystem.onEvent -= RootEventHandler;
		}
	}
#endif
}