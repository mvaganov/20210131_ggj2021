#define USING_UNITY_INPUT_SYSTEM
#if USING_UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using System.Collections.Generic;

namespace NonStandard.Inputs {
	public static class KCodeExtensionUnity {
		public static bool IsDown(this KCode kCode) {return AppInput.GetKeyDown(kCode);}
		public static bool IsUp(this KCode kCode) {return AppInput.GetKeyUp(kCode);}
		public static bool IsHeld(this KCode kCode) {return AppInput.GetKey(kCode);}

		/// <summary>
		/// checks *every* possible KCode, don't put this in an inner loop. if the KCode is pressed, it is added to the given list.
		/// </summary>
		/// <param name="out_keys"></param>
		public static void GetHeld(List<KCode> out_keys) {
			for(int i = 0; i < (int)KCode.LAST; ++i) { KCode key = (KCode)i; if (IsHeld(key)) { out_keys.Add(key); } }
		}
		public static void GetDown(List<KCode> out_keys) {
			for (int i = 0; i < (int)KCode.LAST; ++i) { KCode key = (KCode)i; if (IsDown(key)) { out_keys.Add(key); } }
		}
		public static void GetUp(List<KCode> out_keys) {
			for (int i = 0; i < (int)KCode.LAST; ++i) { KCode key = (KCode)i; if (IsUp(key)) { out_keys.Add(key); } }
		}
		public static KState GetState(this KCode kCode) {
			// prevent two-finger-right-click on touch screens, it messes with other right-click behaviour
			if(kCode == KCode.Mouse1 && UnityEngine.Input.touches != null && UnityEngine.Input.touches.Length >= 2)
				return KState.KeyReleased;
			return AppInput.GetKeyDown(kCode) ? KState.KeyDown :
			AppInput.GetKeyUp(kCode) ? KState.KeyUp : 
			AppInput.GetKey(kCode) ? KState.KeyHeld :
			KState.KeyReleased;
		}
		
#if USING_UNITY_INPUT_SYSTEM
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
#endif
	}
}
