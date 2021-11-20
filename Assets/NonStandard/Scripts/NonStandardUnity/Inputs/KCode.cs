#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif
using System.Collections.Generic;

namespace NonStandard.Inputs {
	public static class KCodeExtensionUnity {
		public static bool IsDown(this KCode kCode) { return AppInput.GetKeyDown(kCode); }
		public static bool IsUp(this KCode kCode) { return AppInput.GetKeyUp(kCode); }
		public static bool IsHeld(this KCode kCode) { return AppInput.GetKey(kCode); }

		/// <summary>
		/// checks *every* possible KCode, don't put this in an inner loop. if the KCode is pressed, it is added to the given list.
		/// </summary>
		/// <param name="out_keys"></param>
		public static void GetHeld(List<KCode> out_keys) {
			for (int i = 0; i < (int)KCode.LAST; ++i) { KCode key = (KCode)i; if (IsHeld(key)) { out_keys.Add(key); } }
		}
		public static void GetDown(List<KCode> out_keys) {
			for (int i = 0; i < (int)KCode.LAST; ++i) { KCode key = (KCode)i; if (IsDown(key)) { out_keys.Add(key); } }
		}
		public static void GetUp(List<KCode> out_keys) {
			for (int i = 0; i < (int)KCode.LAST; ++i) { KCode key = (KCode)i; if (IsUp(key)) { out_keys.Add(key); } }
		}
		public static KState GetState(this KCode kCode) {
			// prevent two-finger-right-click on touch screens, it messes with other right-click behaviour
			if (kCode == KCode.Mouse1 && UnityEngine.Input.touches != null && UnityEngine.Input.touches.Length >= 2)
				return KState.KeyReleased;
			return AppInput.GetKeyDown(kCode) ? KState.KeyDown :
			AppInput.GetKeyUp(kCode) ? KState.KeyUp :
			AppInput.GetKey(kCode) ? KState.KeyHeld :
			KState.KeyReleased;
		}

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		public static readonly Dictionary<KCode, Key> kCodeToInputSystem = new Dictionary<KCode, Key>() {
			[KCode.None] = Key.None,
			[KCode.Space] = Key.Space,
			[KCode.Return] = Key.Enter,
			[KCode.Tab] = Key.Tab,
			[KCode.BackQuote] = Key.Backquote,
			[KCode.Quote] = Key.Quote,
			[KCode.Semicolon] = Key.Semicolon,
			[KCode.Comma] = Key.Comma,
			[KCode.Period] = Key.Period,
			[KCode.Slash] = Key.Slash,
			[KCode.Backslash] = Key.Backslash,
			[KCode.LeftBracket] = Key.LeftBracket,
			[KCode.RightBracket] = Key.RightBracket,
			[KCode.Minus] = Key.Minus,
			[KCode.Equals] = Key.Equals,
			[KCode.A] = Key.A,
			[KCode.B] = Key.B,
			[KCode.C] = Key.C,
			[KCode.D] = Key.D,
			[KCode.E] = Key.E,
			[KCode.F] = Key.F,
			[KCode.G] = Key.G,
			[KCode.H] = Key.H,
			[KCode.I] = Key.I,
			[KCode.J] = Key.J,
			[KCode.K] = Key.K,
			[KCode.L] = Key.L,
			[KCode.M] = Key.M,
			[KCode.N] = Key.N,
			[KCode.O] = Key.O,
			[KCode.P] = Key.P,
			[KCode.Q] = Key.Q,
			[KCode.R] = Key.R,
			[KCode.S] = Key.S,
			[KCode.T] = Key.T,
			[KCode.U] = Key.U,
			[KCode.V] = Key.V,
			[KCode.W] = Key.W,
			[KCode.X] = Key.X,
			[KCode.Y] = Key.Y,
			[KCode.Z] = Key.Z,
			[KCode.Alpha1] = Key.Digit1,
			[KCode.Alpha2] = Key.Digit2,
			[KCode.Alpha3] = Key.Digit3,
			[KCode.Alpha4] = Key.Digit4,
			[KCode.Alpha5] = Key.Digit5,
			[KCode.Alpha6] = Key.Digit6,
			[KCode.Alpha7] = Key.Digit7,
			[KCode.Alpha8] = Key.Digit8,
			[KCode.Alpha9] = Key.Digit9,
			[KCode.Alpha0] = Key.Digit0,
			[KCode.LeftShift] = Key.LeftShift,
			[KCode.RightShift] = Key.RightShift,
			[KCode.LeftAlt] = Key.LeftAlt,
			[KCode.AltGr] = Key.AltGr,
			[KCode.RightAlt] = Key.RightAlt,
			[KCode.LeftControl] = Key.LeftCtrl,
			[KCode.RightControl] = Key.RightCtrl,
			[KCode.LeftApple] = Key.LeftApple,
			[KCode.LeftCommand] = Key.LeftCommand,
			[KCode.LeftWindows] = Key.LeftWindows,
			[KCode.RightApple] = Key.RightApple,
			[KCode.RightCommand] = Key.RightCommand,
			[KCode.RightWindows] = Key.RightWindows,
			[KCode.Menu] = Key.ContextMenu,
			[KCode.Escape] = Key.Escape,
			[KCode.LeftArrow] = Key.LeftArrow,
			[KCode.RightArrow] = Key.RightArrow,
			[KCode.UpArrow] = Key.UpArrow,
			[KCode.DownArrow] = Key.DownArrow,
			[KCode.Backspace] = Key.Backspace,
			[KCode.PageDown] = Key.PageDown,
			[KCode.PageUp] = Key.PageUp,
			[KCode.Home] = Key.Home,
			[KCode.End] = Key.End,
			[KCode.Insert] = Key.Insert,
			[KCode.Delete] = Key.Delete,
			[KCode.CapsLock] = Key.CapsLock,
			[KCode.Numlock] = Key.NumLock,
			[KCode.Print] = Key.PrintScreen,
			[KCode.ScrollLock] = Key.ScrollLock,
			[KCode.Pause] = Key.Pause,
			[KCode.KeypadEnter] = Key.NumpadEnter,
			[KCode.KeypadDivide] = Key.NumpadDivide,
			[KCode.KeypadMultiply] = Key.NumpadMultiply,
			[KCode.KeypadPlus] = Key.NumpadPlus,
			[KCode.KeypadMinus] = Key.NumpadMinus,
			[KCode.KeypadPeriod] = Key.NumpadPeriod,
			[KCode.KeypadEquals] = Key.NumpadEquals,
			[KCode.Keypad0] = Key.Numpad0,
			[KCode.Keypad1] = Key.Numpad1,
			[KCode.Keypad2] = Key.Numpad2,
			[KCode.Keypad3] = Key.Numpad3,
			[KCode.Keypad4] = Key.Numpad4,
			[KCode.Keypad5] = Key.Numpad5,
			[KCode.Keypad6] = Key.Numpad6,
			[KCode.Keypad7] = Key.Numpad7,
			[KCode.Keypad8] = Key.Numpad8,
			[KCode.Keypad9] = Key.Numpad9,
			[KCode.F1] = Key.F1,
			[KCode.F2] = Key.F2,
			[KCode.F3] = Key.F3,
			[KCode.F4] = Key.F4,
			[KCode.F5] = Key.F5,
			[KCode.F6] = Key.F6,
			[KCode.F7] = Key.F7,
			[KCode.F8] = Key.F8,
			[KCode.F9] = Key.F9,
			[KCode.F10] = Key.F10,
			[KCode.F11] = Key.F11,
			[KCode.F12] = Key.F12,
			//[KCode.OEM1] = Key.OEM1,
			//[KCode.OEM2] = Key.OEM2,
			//[KCode.OEM3] = Key.OEM3,
			//[KCode.OEM4] = Key.OEM4,
			//[KCode.OEM5] = Key.OEM5,
			//[KCode.IMESelected] = Key.IMESelected,
		};
		public static Dictionary<Key, KCode> inputSystemKeyboardToKCode = new Dictionary<Key, KCode>();
		public static Dictionary<AxisControl, KCode> inputSystemMouseAndAxisToKCode = null;

		public static KCode GetInputCode(AxisControl controller) {
			KeyControl kc = controller as KeyControl;
			if (kc != null) {
				if(inputSystemKeyboardToKCode.Count == 0) {
					foreach(KeyValuePair<KCode,Key> kvp in kCodeToInputSystem) {
						inputSystemKeyboardToKCode[kvp.Value] = kvp.Key;
					}
				}
				if (inputSystemKeyboardToKCode.TryGetValue(kc.keyCode, out KCode v)) { return v; }
				return KCode.NoOption;
			}
			if (inputSystemMouseAndAxisToKCode == null) {
				Mouse mouse = Mouse.current;
				inputSystemMouseAndAxisToKCode = new Dictionary<AxisControl, KCode>() {
					[mouse.leftButton] = KCode.Mouse0,
					[mouse.rightButton] = KCode.Mouse1,
					[mouse.middleButton] = KCode.Mouse2,
					[mouse.forwardButton] = KCode.Mouse3,
					[mouse.backButton] = KCode.Mouse4,
					[mouse.press] = KCode.Mouse5,
					[mouse.pressure] = KCode.Mouse6,
					[mouse.scroll.y] = mouse.scroll.y.ReadValue() <= 0 ? KCode.MouseWheelDown : KCode.MouseWheelUp,
					[mouse.scroll.x] = mouse.scroll.x.ReadValue() <= 0 ? KCode.MouseWheelLeft : KCode.MouseWheelRight,
					[mouse.delta.y] = mouse.delta.y.ReadValue() <= 0 ? KCode.MouseYDown : KCode.MouseYUp,
					[mouse.delta.x] = mouse.delta.x.ReadValue() <= 0 ? KCode.MouseXDown : KCode.MouseXUp,
				};
				if (inputSystemMouseAndAxisToKCode.TryGetValue(controller, out KCode v)) { return v; }
				return KCode.NoCtrl;
			}
			return KCode.None;
		}
		public static AxisControl GetInputController(KCode kcode) {
			switch (kcode) {
			case KCode.Space: return Keyboard.current.spaceKey;
			case KCode.Return: return Keyboard.current.enterKey;
			case KCode.Tab: return Keyboard.current.tabKey;
			case KCode.BackQuote: return Keyboard.current.backquoteKey;
			case KCode.Quote: return Keyboard.current.quoteKey;
			case KCode.Semicolon: return Keyboard.current.semicolonKey;
			case KCode.Comma: return Keyboard.current.commaKey;
			case KCode.Period: return Keyboard.current.periodKey;
			case KCode.Slash: return Keyboard.current.slashKey;
			case KCode.Backslash: return Keyboard.current.backslashKey;
			case KCode.LeftBracket: return Keyboard.current.leftBracketKey;
			case KCode.RightBracket: return Keyboard.current.rightBracketKey;
			case KCode.Minus: return Keyboard.current.minusKey;
			case KCode.Equals: return Keyboard.current.equalsKey;
			case KCode.A: return Keyboard.current.aKey;
			case KCode.B: return Keyboard.current.bKey;
			case KCode.C: return Keyboard.current.cKey;
			case KCode.D: return Keyboard.current.dKey;
			case KCode.E: return Keyboard.current.eKey;
			case KCode.F: return Keyboard.current.fKey;
			case KCode.G: return Keyboard.current.gKey;
			case KCode.H: return Keyboard.current.hKey;
			case KCode.I: return Keyboard.current.iKey;
			case KCode.J: return Keyboard.current.jKey;
			case KCode.K: return Keyboard.current.mKey;
			case KCode.L: return Keyboard.current.lKey;
			case KCode.M: return Keyboard.current.mKey;
			case KCode.N: return Keyboard.current.nKey;
			case KCode.O: return Keyboard.current.oKey;
			case KCode.P: return Keyboard.current.pKey;
			case KCode.Q: return Keyboard.current.qKey;
			case KCode.R: return Keyboard.current.rKey;
			case KCode.S: return Keyboard.current.sKey;
			case KCode.T: return Keyboard.current.tKey;
			case KCode.U: return Keyboard.current.uKey;
			case KCode.V: return Keyboard.current.aKey;
			case KCode.W: return Keyboard.current.wKey;
			case KCode.X: return Keyboard.current.xKey;
			case KCode.Y: return Keyboard.current.yKey;
			case KCode.Z: return Keyboard.current.zKey;
			case KCode.Alpha1: return Keyboard.current.digit1Key;
			case KCode.Alpha2: return Keyboard.current.digit2Key;
			case KCode.Alpha3: return Keyboard.current.digit3Key;
			case KCode.Alpha4: return Keyboard.current.digit4Key;
			case KCode.Alpha5: return Keyboard.current.digit5Key;
			case KCode.Alpha6: return Keyboard.current.digit6Key;
			case KCode.Alpha7: return Keyboard.current.digit7Key;
			case KCode.Alpha8: return Keyboard.current.digit8Key;
			case KCode.Alpha9: return Keyboard.current.digit9Key;
			case KCode.Alpha0: return Keyboard.current.digit0Key;
			case KCode.LeftShift: return Keyboard.current.leftShiftKey;
			case KCode.RightShift: return Keyboard.current.rightShiftKey;
			case KCode.LeftAlt: return Keyboard.current.leftAltKey;
			case KCode.AltGr: return Keyboard.current.altKey;
			case KCode.RightAlt: return Keyboard.current.rightAltKey;
			case KCode.LeftControl: return Keyboard.current.leftCtrlKey;
			case KCode.RightControl: return Keyboard.current.rightCtrlKey;
			//case KCode.LeftApple: return Keyboard.current.leftAppleKey;
			case KCode.LeftCommand: return Keyboard.current.leftCommandKey;
			case KCode.LeftWindows: return Keyboard.current.leftWindowsKey;
			//case KCode.RightApple: return Keyboard.current.rightAppleKey;
			case KCode.RightCommand: return Keyboard.current.rightCommandKey;
			case KCode.RightWindows: return Keyboard.current.rightWindowsKey;
			case KCode.Menu: return Keyboard.current.contextMenuKey;
			case KCode.Escape: return Keyboard.current.escapeKey;
			case KCode.LeftArrow: return Keyboard.current.leftArrowKey;
			case KCode.RightArrow: return Keyboard.current.rightArrowKey;
			case KCode.UpArrow: return Keyboard.current.upArrowKey;
			case KCode.DownArrow: return Keyboard.current.downArrowKey;
			case KCode.Backspace: return Keyboard.current.backspaceKey;
			case KCode.PageDown: return Keyboard.current.pageDownKey;
			case KCode.PageUp: return Keyboard.current.pageUpKey;
			case KCode.Home: return Keyboard.current.homeKey;
			case KCode.End: return Keyboard.current.endKey;
			case KCode.Insert: return Keyboard.current.insertKey;
			case KCode.Delete: return Keyboard.current.deleteKey;
			case KCode.CapsLock: return Keyboard.current.capsLockKey;
			case KCode.Numlock: return Keyboard.current.numLockKey;
			case KCode.Print: return Keyboard.current.printScreenKey;
			case KCode.ScrollLock: return Keyboard.current.scrollLockKey;
			case KCode.Pause: return Keyboard.current.pauseKey;
			case KCode.KeypadEnter: return Keyboard.current.numpadEnterKey;
			case KCode.KeypadDivide: return Keyboard.current.numpadDivideKey;
			case KCode.KeypadMultiply: return Keyboard.current.numpadMultiplyKey;
			case KCode.KeypadPlus: return Keyboard.current.numpadPlusKey;
			case KCode.KeypadMinus: return Keyboard.current.numpadMinusKey;
			case KCode.KeypadPeriod: return Keyboard.current.numpadPeriodKey;
			case KCode.KeypadEquals: return Keyboard.current.numpadEqualsKey;
			case KCode.Keypad0: return Keyboard.current.numpad0Key;
			case KCode.Keypad1: return Keyboard.current.numpad1Key;
			case KCode.Keypad2: return Keyboard.current.numpad2Key;
			case KCode.Keypad3: return Keyboard.current.numpad3Key;
			case KCode.Keypad4: return Keyboard.current.numpad4Key;
			case KCode.Keypad5: return Keyboard.current.numpad5Key;
			case KCode.Keypad6: return Keyboard.current.numpad6Key;
			case KCode.Keypad7: return Keyboard.current.numpad7Key;
			case KCode.Keypad8: return Keyboard.current.numpad8Key;
			case KCode.Keypad9: return Keyboard.current.numpad9Key;
			case KCode.F1: return Keyboard.current.f1Key;
			case KCode.F2: return Keyboard.current.f2Key;
			case KCode.F3: return Keyboard.current.f3Key;
			case KCode.F4: return Keyboard.current.f4Key;
			case KCode.F5: return Keyboard.current.f5Key;
			case KCode.F6: return Keyboard.current.f6Key;
			case KCode.F7: return Keyboard.current.f7Key;
			case KCode.F8: return Keyboard.current.f8Key;
			case KCode.F9: return Keyboard.current.f9Key;
			case KCode.F10: return Keyboard.current.f10Key;
			case KCode.F11: return Keyboard.current.f11Key;
			case KCode.F12: return Keyboard.current.f12Key;
			case KCode.Mouse0: return Mouse.current.leftButton;
			case KCode.Mouse1: return Mouse.current.rightButton;
			case KCode.Mouse2: return Mouse.current.middleButton;
			case KCode.Mouse3: return Mouse.current.forwardButton;
			case KCode.Mouse4: return Mouse.current.backButton;
			case KCode.Mouse5: return Mouse.current.press;
			case KCode.Mouse6: return Mouse.current.pressure;
			case KCode.MouseWheelDown: return Mouse.current.scroll.y;
			case KCode.MouseXDown: return Mouse.current.delta.x;
			case KCode.MouseXUp: return Mouse.current.delta.x;
			case KCode.MouseYDown: return Mouse.current.delta.y;
			case KCode.MouseYUp: return Mouse.current.delta.y;
			}
			return null;
		}
#endif
	}
}
