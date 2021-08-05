using System;
using System.Collections.Generic;

namespace NonStandard.Inputs {
	public static class KCodeExtension {
		public static KCode Normalized(this KCode k) {
			switch (k) {
			case KCode.AltGr: case KCode.LeftAlt: case KCode.RightAlt: return KCode.AnyAlt;
			case KCode.LeftShift: case KCode.RightShift: return KCode.AnyShift;
			case KCode.LeftApple: case KCode.RightApple: return KCode.LeftApple;
			case KCode.LeftWindows: case KCode.RightWindows: return KCode.LeftWindows;
			case KCode.LeftControl: case KCode.RightControl: return KCode.AnyCtrl;
			}
			return k;
		}
		public static string NormalName(this KCode k) {
			switch (k) {
			case KCode.AnyAlt:
			case KCode.AltGr:
			case KCode.LeftAlt:
			case KCode.RightAlt:
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			return "Option";
#else
				return "Alt";
#endif
			case KCode.AnyShift: case KCode.LeftShift: case KCode.RightShift: return "Shift";
			case KCode.LeftApple: case KCode.RightApple: return "Apple";
			case KCode.LeftWindows: case KCode.RightWindows: return "Windows";
			case KCode.AnyCtrl: case KCode.LeftControl: case KCode.RightControl: return "Ctrl";
			case KCode.Mouse0: return "Left Click";
			case KCode.Mouse1: return "Right Click";
			case KCode.Mouse2: return "Middle Click";
			case KCode.MouseXUp: return "Mouse Right";
			case KCode.MouseXDown: return "Mouse Left";
			case KCode.MouseYUp: return "Mouse Up";
			case KCode.MouseYDown: return "Mouse Down";
			}
			return k.ToString();
		}

		public static KCode FromConsoleKey(System.ConsoleKeyInfo ckey) { return FromConsoleKey(ckey.Key); }

		public static readonly Dictionary<ConsoleKey, KCode> consoleToKCode = new Dictionary<ConsoleKey, KCode>() {
			[ConsoleKey.PageUp] = KCode.PageUp,
			[ConsoleKey.PageDown] = KCode.PageDown,
			[ConsoleKey.End] = KCode.End,
			[ConsoleKey.Home] = KCode.Home,
			[ConsoleKey.LeftArrow] = KCode.LeftArrow,
			[ConsoleKey.UpArrow] = KCode.UpArrow,
			[ConsoleKey.RightArrow] = KCode.RightArrow,
			[ConsoleKey.DownArrow] = KCode.DownArrow,
			[ConsoleKey.Print] = KCode.Print,
			[ConsoleKey.Execute] = KCode.SysReq,
			[ConsoleKey.PrintScreen] = KCode.Print,
			[ConsoleKey.Insert] = KCode.Insert,
			[ConsoleKey.Delete] = KCode.Delete,
			[ConsoleKey.Help] = KCode.Help,
			[ConsoleKey.A] = KCode.A,
			[ConsoleKey.B] = KCode.B,
			[ConsoleKey.C] = KCode.C,
			[ConsoleKey.D] = KCode.D,
			[ConsoleKey.E] = KCode.E,
			[ConsoleKey.F] = KCode.F,
			[ConsoleKey.G] = KCode.G,
			[ConsoleKey.H] = KCode.H,
			[ConsoleKey.I] = KCode.I,
			[ConsoleKey.J] = KCode.J,
			[ConsoleKey.K] = KCode.K,
			[ConsoleKey.L] = KCode.L,
			[ConsoleKey.M] = KCode.M,
			[ConsoleKey.N] = KCode.N,
			[ConsoleKey.O] = KCode.O,
			[ConsoleKey.P] = KCode.P,
			[ConsoleKey.Q] = KCode.Q,
			[ConsoleKey.R] = KCode.R,
			[ConsoleKey.S] = KCode.S,
			[ConsoleKey.T] = KCode.T,
			[ConsoleKey.U] = KCode.U,
			[ConsoleKey.V] = KCode.V,
			[ConsoleKey.W] = KCode.W,
			[ConsoleKey.X] = KCode.X,
			[ConsoleKey.Y] = KCode.Y,
			[ConsoleKey.Z] = KCode.Z,
			[ConsoleKey.LeftWindows] = KCode.LeftWindows,
			[ConsoleKey.RightWindows] = KCode.RightWindows,
			[ConsoleKey.Applications] = KCode.Menu,
			[ConsoleKey.NumPad0] = KCode.Keypad0,
			[ConsoleKey.NumPad1] = KCode.Keypad1,
			[ConsoleKey.NumPad2] = KCode.Keypad2,
			[ConsoleKey.NumPad3] = KCode.Keypad3,
			[ConsoleKey.NumPad4] = KCode.Keypad4,
			[ConsoleKey.NumPad5] = KCode.Keypad5,
			[ConsoleKey.NumPad6] = KCode.Keypad6,
			[ConsoleKey.NumPad7] = KCode.Keypad7,
			[ConsoleKey.NumPad8] = KCode.Keypad8,
			[ConsoleKey.NumPad9] = KCode.Keypad9,
			[ConsoleKey.Multiply] = KCode.KeypadMultiply,
			[ConsoleKey.Add] = KCode.KeypadPlus,
			[ConsoleKey.Separator] = KCode.Minus,
			[ConsoleKey.Subtract] = KCode.KeypadMinus,
			[ConsoleKey.Decimal] = KCode.KeypadPeriod,
			[ConsoleKey.Divide] = KCode.KeypadDivide,
			[ConsoleKey.F1] = KCode.F1,
			[ConsoleKey.F2] = KCode.F2,
			[ConsoleKey.F3] = KCode.F3,
			[ConsoleKey.F4] = KCode.F4,
			[ConsoleKey.F5] = KCode.F5,
			[ConsoleKey.F6] = KCode.F6,
			[ConsoleKey.F7] = KCode.F7,
			[ConsoleKey.F8] = KCode.F8,
			[ConsoleKey.F9] = KCode.F9,
			[ConsoleKey.F10] = KCode.F10,
			[ConsoleKey.F11] = KCode.F11,
			[ConsoleKey.F12] = KCode.F12,
			[ConsoleKey.F13] = KCode.F13,
			[ConsoleKey.F14] = KCode.F14,
			[ConsoleKey.F15] = KCode.F15,
			[ConsoleKey.Oem1] = KCode.Semicolon,
			[ConsoleKey.OemPlus] = KCode.Plus,
			[ConsoleKey.OemComma] = KCode.Comma,
			[ConsoleKey.OemMinus] = KCode.Minus,
			[ConsoleKey.OemPeriod] = KCode.Period,
			[ConsoleKey.Oem2] = KCode.Slash,
			[ConsoleKey.Oem3] = KCode.BackQuote,
			[ConsoleKey.Oem4] = KCode.LeftBracket,
			[ConsoleKey.Oem5] = KCode.Backslash,
			[ConsoleKey.Oem6] = KCode.RightBracket,
			[ConsoleKey.Oem7] = KCode.Quote,

			[ConsoleKey.Select] = KCode.UnsupportedKey,
			[ConsoleKey.Sleep] = KCode.UnsupportedKey,
			[ConsoleKey.F16] = KCode.UnsupportedKey,
			[ConsoleKey.F17] = KCode.UnsupportedKey,
			[ConsoleKey.F18] = KCode.UnsupportedKey,
			[ConsoleKey.F19] = KCode.UnsupportedKey,
			[ConsoleKey.F20] = KCode.UnsupportedKey,
			[ConsoleKey.F21] = KCode.UnsupportedKey,
			[ConsoleKey.F22] = KCode.UnsupportedKey,
			[ConsoleKey.F23] = KCode.UnsupportedKey,
			[ConsoleKey.F24] = KCode.UnsupportedKey,
			[ConsoleKey.BrowserBack] = KCode.UnsupportedKey,
			[ConsoleKey.BrowserForward] = KCode.UnsupportedKey,
			[ConsoleKey.BrowserRefresh] = KCode.UnsupportedKey,
			[ConsoleKey.BrowserStop] = KCode.UnsupportedKey,
			[ConsoleKey.BrowserSearch] = KCode.UnsupportedKey,
			[ConsoleKey.BrowserFavorites] = KCode.UnsupportedKey,
			[ConsoleKey.BrowserHome] = KCode.UnsupportedKey,
			[ConsoleKey.VolumeMute] = KCode.UnsupportedKey,
			[ConsoleKey.VolumeDown] = KCode.UnsupportedKey,
			[ConsoleKey.VolumeUp] = KCode.UnsupportedKey,
			[ConsoleKey.MediaNext] = KCode.UnsupportedKey,
			[ConsoleKey.MediaPrevious] = KCode.UnsupportedKey,
			[ConsoleKey.MediaStop] = KCode.UnsupportedKey,
			[ConsoleKey.MediaPlay] = KCode.UnsupportedKey,
			[ConsoleKey.LaunchMail] = KCode.UnsupportedKey,
			[ConsoleKey.LaunchMediaSelect] = KCode.UnsupportedKey,
			[ConsoleKey.LaunchApp1] = KCode.UnsupportedKey,
			[ConsoleKey.LaunchApp2] = KCode.UnsupportedKey,
			[ConsoleKey.Oem8] = KCode.UnsupportedKey,
			[ConsoleKey.Oem102] = KCode.UnsupportedKey,
			[ConsoleKey.Process] = KCode.UnsupportedKey,
			[ConsoleKey.Packet] = KCode.UnsupportedKey,
			[ConsoleKey.Attention] = KCode.UnsupportedKey,
			[ConsoleKey.CrSel] = KCode.UnsupportedKey,
			[ConsoleKey.ExSel] = KCode.UnsupportedKey,
			[ConsoleKey.EraseEndOfFile] = KCode.UnsupportedKey,
			[ConsoleKey.Play] = KCode.UnsupportedKey,
			[ConsoleKey.Zoom] = KCode.UnsupportedKey,
			[ConsoleKey.NoName] = KCode.UnsupportedKey,
			[ConsoleKey.Pa1] = KCode.UnsupportedKey,
			[ConsoleKey.OemClear] = KCode.UnsupportedKey,
		};
		public static KCode FromConsoleKey(System.ConsoleKey cKey) {
			if (consoleToKCode.TryGetValue(cKey, out KCode kCode)) { return kCode; }
			return (KCode)(int)(cKey);
		}
	}
	/// <summary>
	/// named after the Unity Input.Get___ methods (except KeyReleased)
	/// </summary>
	public enum KState { KeyReleased, KeyDown, KeyHeld, KeyUp }

	/// <summary>
	///   built to be an extension for the UnityEngine.KeyCode enumerator
	/// Key codes returned by Event.keyCode. These map directly to a physical key on the keyboard
	/// </summary>
	public enum KCode {
		/// Not assigned (never returned as the result of a keystroke)
		None = 0,

		// UNUSED = 1, // 0x00000001
		// UNUSED = 2, // 0x00000001
		// UNUSED = 3, // 0x00000001
		// UNUSED = 4, // 0x00000001
		// UNUSED = 5, // 0x00000001
		// UNUSED = 6, // 0x00000001
		// UNUSED = 7, // 0x00000001

		/// <summary>
		/// The backspace key
		/// </summary>
		Backspace = 8,
		/// <summary>
		/// The tab key
		/// </summary>
		Tab = 9,

		// UNUSED = 10, // 0x0000000A
		// UNUSED = 11, // 0x0000000B

		/// <summary>
		/// The Clear key
		/// </summary>
		Clear = 12, // 0x0000000C
		/// <summary>
		/// Return key
		/// </summary>
		Return = 13, // 0x0000000D

		// UNUSED = 14, // 0x0000000E
		// UNUSED = 15 // 0x0000000F
		// UNUSED = 16, // 0x00000010
		// UNUSED = 17, // 0x00000011
		// UNUSED = 18, // 0x00000012

		/// <summary>
		/// Pause on PC machines
		/// </summary>
		Pause = 19, // 0x00000013

		// UNUSED = 20, // 0x00000014
		// UNUSED = 21, // 0x00000015
		// UNUSED = 22, // 0x00000016
		// UNUSED = 23, // 0x00000017
		// UNUSED = 24, // 0x00000018
		// UNUSED = 25, // 0x00000019
		// UNUSED = 26, // 0x0000001A

		/// <summary>
		/// Escape key
		/// </summary>
		Escape = 27, // 0x0000001B

		MouseXUp = 28, // 0x0000001C
		MouseXDown = 29, // 0x0000001D
		MouseYUp = 30, // 0x0000001E
		MouseYDown = 31, // 0x0000001F

		/// <summary>
		/// Space key
		/// </summary>
		Space = 32, // 0x00000020
		/// <summary>
		/// Exclamation mark key '!'
		/// </summary>
		Exclaim = 33, // 0x00000021
		/// <summary>
		/// Double quote key '"'
		/// </summary>
		DoubleQuote = 34, // 0x00000022
		/// <summary>
		/// Hash key '#'
		/// </summary>
		Hash = 35, // 0x00000023
		/// <summary>
		/// Dollar sign key '$'
		/// </summary>
		Dollar = 36, // 0x00000024
		/// <summary>
		/// Percent '%' key
		/// </summary>
		Percent = 37, // 0x00000025
		/// <summary>
		/// Ampersand key '&amp;'
		/// </summary>
		Ampersand = 38, // 0x00000026
		/// <summary>
		/// Quote key '
		/// </summary>
		Quote = 39, // 0x00000027
		/// <summary>
		/// Left Parenthesis key '('
		/// </summary>
		LeftParen = 40, // 0x00000028
		/// <summary>
		/// Right Parenthesis key ')'
		/// </summary>
		RightParen = 41, // 0x00000029
		/// <summary>
		/// Asterisk key '*'
		/// </summary>
		Asterisk = 42, // 0x0000002A
		/// <summary>
		/// Plus key '+'
		/// </summary>
		Plus = 43, // 0x0000002B
		/// <summary>
		/// Comma ',' key
		/// </summary>
		Comma = 44, // 0x0000002C
		/// <summary>
		/// Minus '-' key
		/// </summary>
		Minus = 45, // 0x0000002D
		/// <summary>
		/// Period '.' key
		/// </summary>
		Period = 46, // 0x0000002E
		/// <summary>
		/// Slash '/' key
		/// </summary>
		Slash = 47, // 0x0000002F
		/// <summary>
		/// The '0' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha0 = 48, // 0x00000030
		/// <summary>
		/// The '1' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha1 = 49, // 0x00000031
		/// <summary>
		/// The '2' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha2 = 50, // 0x00000032
		/// <summary>
		/// The '3' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha3 = 51, // 0x00000033
		/// <summary>
		/// The '4' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha4 = 52, // 0x00000034
		/// <summary>
		/// The '5' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha5 = 53, // 0x00000035
		/// <summary>
		/// The '6' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha6 = 54, // 0x00000036
		/// <summary>
		/// The '7' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha7 = 55, // 0x00000037
		/// <summary>
		/// The '8' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha8 = 56, // 0x00000038
		/// <summary>
		/// The '9' key on the top of the alphanumeric keyboard
		/// </summary>
		Alpha9 = 57, // 0x00000039
		/// <summary>
		/// Colon ':' key
		/// </summary>
		Colon = 58, // 0x0000003A
		/// <summary>
		/// Semicolon ';' key
		/// </summary>
		Semicolon = 59, // 0x0000003B
		/// <summary>
		/// Less than '&lt;' key
		/// </summary>
		Less = 60, // 0x0000003C
		/// <summary>
		/// Equals '=' key
		/// </summary>
		Equals = 61, // 0x0000003D
		/// <summary>
		/// Greater than '&gt;' key
		/// </summary>
		Greater = 62, // 0x0000003E
		/// <summary>
		/// Question mark '?' key
		/// </summary>
		Question = 63, // 0x0000003F
		/// <summary>
		/// At key '@'
		/// </summary>
		At = 64, // 0x00000040

		// UNUSED = 65, // a 0x00000041
		// UNUSED = 66, // b 0x00000042
		// UNUSED = 67, // c 0x00000043
		// UNUSED = 68, // d 0x00000044
		// UNUSED = 69, // e 0x00000045
		// UNUSED = 70, // f 0x00000046
		// UNUSED = 71, // g 0x00000047
		// UNUSED = 72, // h 0x00000048
		// UNUSED = 73, // i 0x00000049
		// UNUSED = 74, // j 0x0000004A
		// UNUSED = 75, // k 0x0000004B
		// UNUSED = 76, // l 0x0000004C
		// UNUSED = 77, // m 0x0000004D
		// UNUSED = 78, // n 0x0000004E
		// UNUSED = 79, // o 0x0000004F
		// UNUSED = 80, // p 0x00000050
		// UNUSED = 81, // q 0x00000051
		// UNUSED = 82, // r 0x00000052
		// UNUSED = 83, // s 0x00000053
		// UNUSED = 84, // t 0x00000054
		// UNUSED = 85, // u 0x00000055
		// UNUSED = 86, // v 0x00000056
		// UNUSED = 87, // w 0x00000057
		// UNUSED = 88, // x 0x00000058
		// UNUSED = 89, // y 0x00000059
		// UNUSED = 90, // z 0x0000005A

		/// <summary>
		/// Left square bracket key '['
		/// </summary>
		LeftBracket = 91, // 0x0000005B
		/// <summary>
		/// Backslash key '\'
		/// </summary>
		Backslash = 92, // 0x0000005C
		/// <summary>
		/// Right square bracket key ']'
		/// </summary>
		RightBracket = 93, // 0x0000005D
		/// <summary>
		/// Caret key '^'
		/// </summary>
		Caret = 94, // 0x0000005E
		/// <summary>
		/// Underscore '_' key
		/// </summary>
		Underscore = 95, // 0x0000005F
		/// <summary>
		/// Back quote key '`'
		/// </summary>
		BackQuote = 96, // 0x00000060
		/// <summary>
		/// 'a' key
		/// </summary>
		A = 97, // 0x00000061
		/// <summary>
		/// 'b' key
		/// </summary>
		B = 98, // 0x00000062
		/// <summary>
		/// 'c' key
		/// </summary>
		C = 99, // 0x00000063
		/// <summary>
		/// 'd' key
		/// </summary>
		D = 100, // 0x00000064
		/// <summary>
		/// 'e' key
		/// </summary>
		E = 101, // 0x00000065
		/// <summary>
		/// 'f' key
		/// </summary>
		F = 102, // 0x00000066
		/// <summary>
		/// 'g' key
		/// </summary>
		G = 103, // 0x00000067
		/// <summary>
		/// 'h' key
		/// </summary>
		H = 104, // 0x00000068
		/// <summary>
		/// 'i' key
		/// </summary>
		I = 105, // 0x00000069
		/// <summary>
		/// 'j' key
		/// </summary>
		J = 106, // 0x0000006A
		/// <summary>
		/// 'k' key
		/// </summary>
		K = 107, // 0x0000006B
		/// <summary>
		/// 'l' key
		/// </summary>
		L = 108, // 0x0000006C
		/// <summary>
		/// 'm' key
		/// </summary>
		M = 109, // 0x0000006D
		/// <summary>
		/// 'n' key
		/// </summary>
		N = 110, // 0x0000006E
		/// <summary>
		/// 'o' key
		/// </summary>
		O = 111, // 0x0000006F
		/// <summary>
		/// 'p' key
		/// </summary>
		P = 112, // 0x00000070
		/// <summary>
		/// 'q' key
		/// </summary>
		Q = 113, // 0x00000071
		/// <summary>
		/// 'r' key
		/// </summary>
		R = 114, // 0x00000072
		/// <summary>
		/// 's' key
		/// </summary>
		S = 115, // 0x00000073
		/// <summary>
		/// 't' key
		/// </summary>
		T = 116, // 0x00000074
		/// <summary>
		/// 'u' key
		/// </summary>
		U = 117, // 0x00000075
		/// <summary>
		/// 'v' key
		/// </summary>
		V = 118, // 0x00000076
		/// <summary>
		/// 'w' key
		/// </summary>
		W = 119, // 0x00000077
		/// <summary>
		/// 'x' key
		/// </summary>
		X = 120, // 0x00000078
		/// <summary>
		/// 'y' key
		/// </summary>
		Y = 121, // 0x00000079
		/// <summary>
		/// 'z' key
		/// </summary>
		Z = 122, // 0x0000007A
		/// <summary>
		/// Left curly bracket key '{'
		/// </summary>
		LeftCurlyBracket = 123, // 0x0000007B
		/// <summary>
		/// Pipe '|' key
		/// </summary>
		Pipe = 124, // 0x0000007C
		/// <summary>
		/// Right curly bracket key '}'
		/// </summary>
		RightCurlyBracket = 125, // 0x0000007D
		/// <summary>
		/// Tilde '~' key
		/// </summary>
		Tilde = 126, // 0x0000007E
		/// <summary>
		/// The forward delete key
		/// </summary>
		Delete = 127, // 0x0000007F

		_UserDefined0 = 128, // 0x00000080
		_UserDefined1 = _UserDefined0 + 1, // 0x00000081
		_UserDefined2 = _UserDefined0 + 2, // 0x00000082
		_UserDefined3 = _UserDefined0 + 3, // 0x00000083
		_UserDefined4 = _UserDefined0 + 4, // 0x00000084
		_UserDefined5 = _UserDefined0 + 5, // 0x00000085
		_UserDefined6 = _UserDefined0 + 6, // 0x00000086
		_UserDefined7 = _UserDefined0 + 7, // 0x00000087
		_UserDefined8 = _UserDefined0 + 8, // 0x00000088
		_UserDefined9 = _UserDefined0 + 9, // 0x00000089
		_UserDefined10 = _UserDefined0 + 10, // 0x0000008A
		_UserDefined11 = _UserDefined0 + 11, // 0x0000008B
		_UserDefined12 = _UserDefined0 + 12, // 0x0000008C
		_UserDefined13 = _UserDefined0 + 13, // 0x0000008D
		_UserDefined14 = _UserDefined0 + 14, // 0x0000008E
		_UserDefined15 = _UserDefined0 + 15, // 0x0000008F

		// UNUSED = 144, // 0x00000090
		// UNUSED = 145, // 0x00000091
		// UNUSED = 146, // 0x00000092
		// UNUSED = 147, // 0x00000093
		// UNUSED = 148, // 0x00000094
		// UNUSED = 149, // 0x00000095
		// UNUSED = 150, // 0x00000096
		// UNUSED = 151, // 0x00000097
		// UNUSED = 152, // 0x00000098
		// UNUSED = 153, // 0x00000099
		// UNUSED = 154, // 0x0000009A
		// UNUSED = 155, // 0x0000009B
		// UNUSED = 156, // 0x0000009C
		// UNUSED = 157, // 0x0000009D
		/// <summary>
		/// shift keys is explicitly not pressed
		/// </summary>
		NoShift = 158, // 0x0000009E
		/// <summary>
		/// ctrl keys are explicitly not pressed
		/// </summary>
		NoCtrl = 159, // 0x0000009F
		/// <summary>
		/// alt keys are explicitly not pressed
		/// </summary>
		NoAlt = 160, // 0x000000A0
		/// <summary>
		/// alt keys are explicitly not pressed
		/// </summary>
		NoOption = 160, // 0x000000A0

		// UNUSED = 161, // 0x000000A1
		// UNUSED = 162, // 0x000000A2
		// UNUSED = 163, // 0x000000A3
		// UNUSED = 164, // 0x000000A4
		// UNUSED = 165, // 0x000000A5
		// UNUSED = 166, // 0x000000A6
		// UNUSED = 167, // 0x000000A7
		// UNUSED = 168, // 0x000000A8
		// UNUSED = 169, // 0x000000A9
		UnsupportedKey = 170, // 0x000000AA
		// UNUSED = 171, // 0x000000AB
		// UNUSED = 172, // 0x000000AC
		// UNUSED = 173, // 0x000000AD
		// UNUSED = 174, // 0x000000AE
		// UNUSED = 175, // 0x000000AF
		// UNUSED = 176, // 0x000000B0
		// ...
		// UNUSED = 255, // 0x000000ff

		/// <summary>
		/// Numeric keypad 0
		/// </summary>
		Keypad0 = 256, // 0x00000100
		/// <summary>
		/// Numeric keypad 1
		/// </summary>
		Keypad1 = 257, // 0x00000101
		/// <summary>
		/// Numeric keypad 2
		/// </summary>
		Keypad2 = 258, // 0x00000102
		/// <summary>
		/// Numeric keypad 3
		/// </summary>
		Keypad3 = 259, // 0x00000103
		/// <summary>
		/// Numeric keypad 4
		/// </summary>
		Keypad4 = 260, // 0x00000104
		/// <summary>
		/// Numeric keypad 5
		/// </summary>
		Keypad5 = 261, // 0x00000105
		/// <summary>
		/// Numeric keypad 6
		/// </summary>
		Keypad6 = 262, // 0x00000106
		/// <summary>
		/// Numeric keypad 7
		/// </summary>
		Keypad7 = 263, // 0x00000107
		/// <summary>
		/// Numeric keypad 8
		/// </summary>
		Keypad8 = 264, // 0x00000108
		/// <summary>
		/// Numeric keypad 9
		/// </summary>
		Keypad9 = 265, // 0x00000109
		/// <summary>
		/// Numeric keypad '.'
		/// </summary>
		KeypadPeriod = 266, // 0x0000010A
		/// <summary>
		/// Numeric keypad '/'
		/// </summary>
		KeypadDivide = 267, // 0x0000010B
		/// <summary>
		/// Numeric keypad '*'
		/// </summary>
		KeypadMultiply = 268, // 0x0000010C
		/// <summary>
		/// Numeric keypad '-'
		/// </summary>
		KeypadMinus = 269, // 0x0000010D
		/// <summary>
		/// Numeric keypad '+'
		/// </summary>
		KeypadPlus = 270, // 0x0000010E
		/// <summary>
		/// Numeric keypad Enter
		/// </summary>
		KeypadEnter = 271, // 0x0000010F
		/// <summary>
		/// Numeric keypad '='
		/// </summary>
		KeypadEquals = 272, // 0x00000110
		/// <summary>
		/// Up arrow key
		/// </summary>
		UpArrow = 273, // 0x00000111
		/// <summary>
		/// Down arrow key
		/// </summary>
		DownArrow = 274, // 0x00000112
		/// <summary>
		/// Right arrow key
		/// </summary>
		RightArrow = 275, // 0x00000113
		/// <summary>
		/// Left arrow key
		/// </summary>
		LeftArrow = 276, // 0x00000114
		/// <summary>
		/// Insert key key
		/// </summary>
		Insert = 277, // 0x00000115
		/// <summary>
		/// Home key
		/// </summary>
		Home = 278, // 0x00000116
		/// <summary>
		/// End key
		/// </summary>
		End = 279, // 0x00000117
		/// <summary>
		/// Page up
		/// </summary>
		PageUp = 280, // 0x00000118
		/// <summary>
		/// Page down
		/// </summary>
		PageDown = 281, // 0x00000119
		/// <summary>
		/// F1 function key
		/// </summary>
		F1 = 282, // 0x0000011A
		/// <summary>
		/// F2 function key
		/// </summary>
		F2 = 283, // 0x0000011B
		/// <summary>
		/// F3 function key
		/// </summary>
		F3 = 284, // 0x0000011C
		/// <summary>
		/// F4 function key
		/// </summary>
		F4 = 285, // 0x0000011D
		/// <summary>
		/// F5 function key
		/// </summary>
		F5 = 286, // 0x0000011E
		/// <summary>
		/// F6 function key
		/// </summary>
		F6 = 287, // 0x0000011F
		/// <summary>
		/// F7 function key
		/// </summary>
		F7 = 288, // 0x00000120
		/// <summary>
		/// F8 function key
		/// </summary>
		F8 = 289, // 0x00000121
		/// <summary>
		/// F9 function key
		/// </summary>
		F9 = 290, // 0x00000122
		/// <summary>
		/// F10 function key
		/// </summary>
		F10 = 291, // 0x00000123
		/// <summary>
		/// F11 function key
		/// </summary>
		F11 = 292, // 0x00000124
		/// <summary>
		/// F12 function key
		/// </summary>
		F12 = 293, // 0x00000125
		/// <summary>
		/// F13 function key
		/// </summary>
		F13 = 294, // 0x00000126
		/// <summary>
		/// F14 function key
		/// </summary>
		F14 = 295, // 0x00000127
		/// <summary>
		/// F15 function key
		/// </summary>
		F15 = 296, // 0x00000128

		/// <summary>
		/// any Shift key
		/// </summary>
		AnyShift = 297, // 0x00000129
		/// <summary>
		/// any Alt key
		/// </summary>
		AnyAlt = 298, // 0x0000012A
		/// <summary>
		/// any Option key
		/// </summary>
		AnyOption = 298, // 0x0000012A
		/// <summary>
		/// any Control key
		/// </summary>
		AnyCtrl = 299, // 0x0000012B

		/// <summary>
		/// Numlock key
		/// </summary>
		Numlock = 300, // 0x0000012C
		/// <summary>
		/// Capslock key
		/// </summary>
		CapsLock = 301, // 0x0000012D
		/// <summary>
		/// Scroll lock key
		/// </summary>
		ScrollLock = 302, // 0x0000012E
		/// <summary>
		/// Right shift key
		/// </summary>
		RightShift = 303, // 0x0000012F
		/// <summary>
		/// Left shift key
		/// </summary>
		LeftShift = 304, // 0x00000130
		/// <summary>
		/// Right Control key
		/// </summary>
		RightControl = 305, // 0x00000131
		/// <summary>
		/// Left Control key
		/// </summary>
		LeftControl = 306, // 0x00000132
		/// <summary>
		/// Right Alt key
		/// </summary>
		RightAlt = 307, // 0x00000133
		RightOption = 307, // 0x00000133
		/// <summary>
		/// Left Alt key
		/// </summary>
		LeftAlt = 308, // 0x00000134
		LeftOption = 308, // 0x00000134
		/// <summary>
		/// Right Command key
		/// </summary>
		RightApple = 309, // 0x00000135
		/// <summary>
		/// Right Command key
		/// </summary>
		RightCommand = 309, // 0x00000135
		/// <summary>
		/// Left Command key
		/// </summary>
		LeftApple = 310, // 0x00000136
		/// <summary>
		/// Left Command key
		/// </summary>
		LeftCommand = 310, // 0x00000136
		/// <summary>
		/// Left Windows key
		/// </summary>
		LeftWindows = 311, // 0x00000137
		/// <summary>
		/// Right Windows key
		/// </summary>
		RightWindows = 312, // 0x00000138
		/// <summary>
		/// Alt Gr key
		/// </summary>
		AltGr = 313, // 0x00000139

		// UNUSED = 314, // 0x0000013A

		/// <summary>
		/// Help key
		/// </summary>
		Help = 315, // 0x0000013B
		/// <summary>
		/// Print key
		/// </summary>
		Print = 316, // 0x0000013C
		/// <summary>
		/// Sys Req key
		/// </summary>
		SysReq = 317, // 0x0000013D
		/// <summary>
		/// Break key
		/// </summary>
		Break = 318, // 0x0000013E
		/// <summary>
		/// Menu key
		/// </summary>
		Menu = 319, // 0x0000013F

		// UNUSED320 = 320, // 0x00000140

		/// <summary>
		/// Pushing the scroll wheel forward
		/// NEW
		/// </summary>
		MouseWheelUp = 321, // 0x00000141
		/// <summary>
		/// Pulling the scroll wheel backward
		/// NEW
		/// </summary>
		MouseWheelDown = 322, // 0x00000142

		/// <summary>
		/// The Left (or primary) mouse button
		/// </summary>
		Mouse0 = 323, // 0x00000143
		/// <summary>
		/// Right mouse button (or secondary mouse button)
		/// </summary>
		Mouse1 = 324, // 0x00000144
		/// <summary>
		/// Middle mouse button (or third button)
		/// </summary>
		Mouse2 = 325, // 0x00000145
		/// <summary>
		/// Additional (fourth) mouse button
		/// </summary>
		Mouse3 = 326, // 0x00000146
		/// <summary>
		/// Additional (fifth) mouse button
		/// </summary>
		Mouse4 = 327, // 0x00000147
		/// <summary>
		/// Additional (or sixth) mouse button
		/// </summary>
		Mouse5 = 328, // 0x00000148
		/// <summary>
		/// Additional (or seventh) mouse button
		/// </summary>
		Mouse6 = 329, // 0x00000149
		/// <summary>
		/// Button 0 on any joystick
		/// </summary>
		JoystickButton0 = 330, // 0x0000014A
		/// <summary>
		/// Button 1 on any joystick
		/// </summary>
		JoystickButton1 = 331, // 0x0000014B
		/// <summary>
		/// Button 2 on any joystick
		/// </summary>
		JoystickButton2 = 332, // 0x0000014C
		/// <summary>
		/// Button 3 on any joystick
		/// </summary>
		JoystickButton3 = 333, // 0x0000014D
		/// <summary>
		/// Button 4 on any joystick
		/// </summary>
		JoystickButton4 = 334, // 0x0000014E
		/// <summary>
		/// Button 5 on any joystick
		/// </summary>
		JoystickButton5 = 335, // 0x0000014F
		/// <summary>
		/// Button 6 on any joystick
		/// </summary>
		JoystickButton6 = 336, // 0x00000150
		/// <summary>
		/// Button 7 on any joystick
		/// </summary>
		JoystickButton7 = 337, // 0x00000151
		/// <summary>
		/// Button 8 on any joystick
		/// </summary>
		JoystickButton8 = 338, // 0x00000152
		/// <summary>
		/// Button 9 on any joystick
		/// </summary>
		JoystickButton9 = 339, // 0x00000153
		/// <summary>
		/// Button 10 on any joystick
		/// </summary>
		JoystickButton10 = 340, // 0x00000154
		/// <summary>
		/// Button 11 on any joystick
		/// </summary>
		JoystickButton11 = 341, // 0x00000155
		/// <summary>
		/// Button 12 on any joystick
		/// </summary>
		JoystickButton12 = 342, // 0x00000156
		/// <summary>
		/// Button 13 on any joystick
		/// </summary>
		JoystickButton13 = 343, // 0x00000157
		/// <summary>
		/// Button 14 on any joystick
		/// </summary>
		JoystickButton14 = 344, // 0x00000158
		/// <summary>
		/// Button 15 on any joystick
		/// </summary>
		JoystickButton15 = 345, // 0x00000159
		/// <summary>
		/// Button 16 on any joystick
		/// </summary>
		JoystickButton16 = 346, // 0x0000015A
		/// <summary>
		/// Button 17 on any joystick
		/// </summary>
		JoystickButton17 = 347, // 0x0000015B
		/// <summary>
		/// Button 18 on any joystick
		/// </summary>
		JoystickButton18 = 348, // 0x0000015C
		/// <summary>
		/// Button 19 on any joystick
		/// </summary>
		JoystickButton19 = 349, // 0x0000015D
		/// <summary>
		/// Button 0 on first joystick
		/// </summary>
		Joystick1Button0 = 350, // 0x0000015E
		/// <summary>
		/// Button 1 on first joystick
		/// </summary>
		Joystick1Button1 = 351, // 0x0000015F
		/// <summary>
		/// Button 2 on first joystick
		/// </summary>
		Joystick1Button2 = 352, // 0x00000160
		/// <summary>
		/// Button 3 on first joystick
		/// </summary>
		Joystick1Button3 = 353, // 0x00000161
		/// <summary>
		/// Button 4 on first joystick
		/// </summary>
		Joystick1Button4 = 354, // 0x00000162
		/// <summary>
		/// Button 5 on first joystick
		/// </summary>
		Joystick1Button5 = 355, // 0x00000163
		/// <summary>
		/// Button 6 on first joystick
		/// </summary>
		Joystick1Button6 = 356, // 0x00000164
		/// <summary>
		/// Button 7 on first joystick
		/// </summary>
		Joystick1Button7 = 357, // 0x00000165
		/// <summary>
		/// Button 8 on first joystick
		/// </summary>
		Joystick1Button8 = 358, // 0x00000166
		/// <summary>
		/// Button 9 on first joystick
		/// </summary>
		Joystick1Button9 = 359, // 0x00000167
		/// <summary>
		/// Button 10 on first joystick
		/// </summary>
		Joystick1Button10 = 360, // 0x00000168
		/// <summary>
		/// Button 11 on first joystick
		/// </summary>
		Joystick1Button11 = 361, // 0x00000169
		/// <summary>
		/// Button 12 on first joystick
		/// </summary>
		Joystick1Button12 = 362, // 0x0000016A
		/// <summary>
		/// Button 13 on first joystick
		/// </summary>
		Joystick1Button13 = 363, // 0x0000016B
		/// <summary>
		/// Button 14 on first joystick
		/// </summary>
		Joystick1Button14 = 364, // 0x0000016C
		/// <summary>
		/// Button 15 on first joystick
		/// </summary>
		Joystick1Button15 = 365, // 0x0000016D
		/// <summary>
		/// Button 16 on first joystick
		/// </summary>
		Joystick1Button16 = 366, // 0x0000016E
		/// <summary>
		/// Button 17 on first joystick
		/// </summary>
		Joystick1Button17 = 367, // 0x0000016F
		/// <summary>
		/// Button 18 on first joystick
		/// </summary>
		Joystick1Button18 = 368, // 0x00000170
		/// <summary>
		/// Button 19 on first joystick
		/// </summary>
		Joystick1Button19 = 369, // 0x00000171
		/// <summary>
		/// Button 0 on second joystick
		/// </summary>
		Joystick2Button0 = 370, // 0x00000172
		/// <summary>
		/// Button 1 on second joystick
		/// </summary>
		Joystick2Button1 = 371, // 0x00000173
		/// <summary>
		/// Button 2 on second joystick
		/// </summary>
		Joystick2Button2 = 372, // 0x00000174
		/// <summary>
		/// Button 3 on second joystick
		/// </summary>
		Joystick2Button3 = 373, // 0x00000175
		/// <summary>
		/// Button 4 on second joystick
		/// </summary>
		Joystick2Button4 = 374, // 0x00000176
		/// <summary>
		/// Button 5 on second joystick
		/// </summary>
		Joystick2Button5 = 375, // 0x00000177
		/// <summary>
		/// Button 6 on second joystick
		/// </summary>
		Joystick2Button6 = 376, // 0x00000178
		/// <summary>
		/// Button 7 on second joystick
		/// </summary>
		Joystick2Button7 = 377, // 0x00000179
		/// <summary>
		/// Button 8 on second joystick
		/// </summary>
		Joystick2Button8 = 378, // 0x0000017A
		/// <summary>
		/// Button 9 on second joystick
		/// </summary>
		Joystick2Button9 = 379, // 0x0000017B
		/// <summary>
		/// Button 10 on second joystick
		/// </summary>
		Joystick2Button10 = 380, // 0x0000017C
		/// <summary>
		/// Button 11 on second joystick
		/// </summary>
		Joystick2Button11 = 381, // 0x0000017D
		/// <summary>
		/// Button 12 on second joystick
		/// </summary>
		Joystick2Button12 = 382, // 0x0000017E
		/// <summary>
		/// Button 13 on second joystick
		/// </summary>
		Joystick2Button13 = 383, // 0x0000017F
		/// <summary>
		/// Button 14 on second joystick
		/// </summary>
		Joystick2Button14 = 384, // 0x00000180
		/// <summary>
		/// Button 15 on second joystick
		/// </summary>
		Joystick2Button15 = 385, // 0x00000181
		/// <summary>
		/// Button 16 on second joystick
		/// </summary>
		Joystick2Button16 = 386, // 0x00000182
		/// <summary>
		/// Button 17 on second joystick
		/// </summary>
		Joystick2Button17 = 387, // 0x00000183
		/// <summary>
		/// Button 18 on second joystick
		/// </summary>
		Joystick2Button18 = 388, // 0x00000184
		/// <summary>
		/// Button 19 on second joystick
		/// </summary>
		Joystick2Button19 = 389, // 0x00000185
		/// <summary>
		/// Button 0 on third joystick
		/// </summary>
		Joystick3Button0 = 390, // 0x00000186
		/// <summary>
		/// Button 1 on third joystick
		/// </summary>
		Joystick3Button1 = 391, // 0x00000187
		/// <summary>
		/// Button 2 on third joystick
		/// </summary>
		Joystick3Button2 = 392, // 0x00000188
		/// <summary>
		/// Button 3 on third joystick
		/// </summary>
		Joystick3Button3 = 393, // 0x00000189
		/// <summary>
		/// Button 4 on third joystick
		/// </summary>
		Joystick3Button4 = 394, // 0x0000018A
		/// <summary>
		/// Button 5 on third joystick
		/// </summary>
		Joystick3Button5 = 395, // 0x0000018B
		/// <summary>
		/// Button 6 on third joystick
		/// </summary>
		Joystick3Button6 = 396, // 0x0000018C
		/// <summary>
		/// Button 7 on third joystick
		/// </summary>
		Joystick3Button7 = 397, // 0x0000018D
		/// <summary>
		/// Button 8 on third joystick
		/// </summary>
		Joystick3Button8 = 398, // 0x0000018E
		/// <summary>
		/// Button 9 on third joystick
		/// </summary>
		Joystick3Button9 = 399, // 0x0000018F
		/// <summary>
		/// Button 10 on third joystick
		/// </summary>
		Joystick3Button10 = 400, // 0x00000190
		/// <summary>
		/// Button 11 on third joystick
		/// </summary>
		Joystick3Button11 = 401, // 0x00000191
		/// <summary>
		/// Button 12 on third joystick
		/// </summary>
		Joystick3Button12 = 402, // 0x00000192
		/// <summary>
		/// Button 13 on third joystick
		/// </summary>
		Joystick3Button13 = 403, // 0x00000193
		/// <summary>
		/// Button 14 on third joystick
		/// </summary>
		Joystick3Button14 = 404, // 0x00000194
		/// <summary>
		/// Button 15 on third joystick
		/// </summary>
		Joystick3Button15 = 405, // 0x00000195
		/// <summary>
		/// Button 16 on third joystick
		/// </summary>
		Joystick3Button16 = 406, // 0x00000196
		/// <summary>
		/// Button 17 on third joystick
		/// </summary>
		Joystick3Button17 = 407, // 0x00000197
		/// <summary>
		/// Button 18 on third joystick
		/// </summary>
		Joystick3Button18 = 408, // 0x00000198
		/// <summary>
		/// Button 19 on third joystick
		/// </summary>
		Joystick3Button19 = 409, // 0x00000199
		/// <summary>
		/// Button 0 on forth joystick
		/// </summary>
		Joystick4Button0 = 410, // 0x0000019A
		/// <summary>
		/// Button 1 on forth joystick
		/// </summary>
		Joystick4Button1 = 411, // 0x0000019B
		/// <summary>
		/// Button 2 on forth joystick
		/// </summary>
		Joystick4Button2 = 412, // 0x0000019C
		/// <summary>
		/// Button 3 on forth joystick
		/// </summary>
		Joystick4Button3 = 413, // 0x0000019D
		/// <summary>
		/// Button 4 on forth joystick
		/// </summary>
		Joystick4Button4 = 414, // 0x0000019E
		/// <summary>
		/// Button 5 on forth joystick
		/// </summary>
		Joystick4Button5 = 415, // 0x0000019F
		/// <summary>
		/// Button 6 on forth joystick
		/// </summary>
		Joystick4Button6 = 416, // 0x000001A0
		/// <summary>
		/// Button 7 on forth joystick
		/// </summary>
		Joystick4Button7 = 417, // 0x000001A1
		/// <summary>
		/// Button 8 on forth joystick
		/// </summary>
		Joystick4Button8 = 418, // 0x000001A2
		/// <summary>
		/// Button 9 on forth joystick
		/// </summary>
		Joystick4Button9 = 419, // 0x000001A3
		/// <summary>
		/// Button 10 on forth joystick
		/// </summary>
		Joystick4Button10 = 420, // 0x000001A4
		/// <summary>
		/// Button 11 on forth joystick
		/// </summary>
		Joystick4Button11 = 421, // 0x000001A5
		/// <summary>
		/// Button 12 on forth joystick
		/// </summary>
		Joystick4Button12 = 422, // 0x000001A6
		/// <summary>
		/// Button 13 on forth joystick
		/// </summary>
		Joystick4Button13 = 423, // 0x000001A7
		/// <summary>
		/// Button 14 on forth joystick
		/// </summary>
		Joystick4Button14 = 424, // 0x000001A8
		/// <summary>
		/// Button 15 on forth joystick
		/// </summary>
		Joystick4Button15 = 425, // 0x000001A9
		/// <summary>
		/// Button 16 on forth joystick
		/// </summary>
		Joystick4Button16 = 426, // 0x000001AA
		/// <summary>
		/// Button 17 on forth joystick
		/// </summary>
		Joystick4Button17 = 427, // 0x000001AB
		/// <summary>
		/// Button 18 on forth joystick
		/// </summary>
		Joystick4Button18 = 428, // 0x000001AC
		/// <summary>
		/// Button 19 on forth joystick
		/// </summary>
		Joystick4Button19 = 429, // 0x000001AD
		/// <summary>
		/// Button 0 on fifth joystick
		/// </summary>
		Joystick5Button0 = 430, // 0x000001AE
		/// <summary>
		/// Button 1 on fifth joystick
		/// </summary>
		Joystick5Button1 = 431, // 0x000001AF
		/// <summary>
		/// Button 2 on fifth joystick
		/// </summary>
		Joystick5Button2 = 432, // 0x000001B0
		/// <summary>
		/// Button 3 on fifth joystick
		/// </summary>
		Joystick5Button3 = 433, // 0x000001B1
		/// <summary>
		/// Button 4 on fifth joystick
		/// </summary>
		Joystick5Button4 = 434, // 0x000001B2
		/// <summary>
		/// Button 5 on fifth joystick
		/// </summary>
		Joystick5Button5 = 435, // 0x000001B3
		/// <summary>
		/// Button 6 on fifth joystick
		/// </summary>
		Joystick5Button6 = 436, // 0x000001B4
		/// <summary>
		/// Button 7 on fifth joystick
		/// </summary>
		Joystick5Button7 = 437, // 0x000001B5
		/// <summary>
		/// Button 8 on fifth joystick
		/// </summary>
		Joystick5Button8 = 438, // 0x000001B6
		/// <summary>
		/// Button 9 on fifth joystick
		/// </summary>
		Joystick5Button9 = 439, // 0x000001B7
		/// <summary>
		/// Button 10 on fifth joystick
		/// </summary>
		Joystick5Button10 = 440, // 0x000001B8
		/// <summary>
		/// Button 11 on fifth joystick
		/// </summary>
		Joystick5Button11 = 441, // 0x000001B9
		/// <summary>
		/// Button 12 on fifth joystick
		/// </summary>
		Joystick5Button12 = 442, // 0x000001BA
		/// <summary>
		/// Button 13 on fifth joystick
		/// </summary>
		Joystick5Button13 = 443, // 0x000001BB
		/// <summary>
		/// Button 14 on fifth joystick
		/// </summary>
		Joystick5Button14 = 444, // 0x000001BC
		/// <summary>
		/// Button 15 on fifth joystick
		/// </summary>
		Joystick5Button15 = 445, // 0x000001BD
		/// <summary>
		/// Button 16 on fifth joystick
		/// </summary>
		Joystick5Button16 = 446, // 0x000001BE
		/// <summary>
		/// Button 17 on fifth joystick
		/// </summary>
		Joystick5Button17 = 447, // 0x000001BF
		/// <summary>
		/// Button 18 on fifth joystick
		/// </summary>
		Joystick5Button18 = 448, // 0x000001C0
		/// <summary>
		/// Button 19 on fifth joystick
		/// </summary>
		Joystick5Button19 = 449, // 0x000001C1
		/// <summary>
		/// Button 0 on sixth joystick
		/// </summary>
		Joystick6Button0 = 450, // 0x000001C2
		/// <summary>
		/// Button 1 on sixth joystick
		/// </summary>
		Joystick6Button1 = 451, // 0x000001C3
		/// <summary>
		/// Button 2 on sixth joystick
		/// </summary>
		Joystick6Button2 = 452, // 0x000001C4
		/// <summary>
		/// Button 3 on sixth joystick
		/// </summary>
		Joystick6Button3 = 453, // 0x000001C5
		/// <summary>
		/// Button 4 on sixth joystick
		/// </summary>
		Joystick6Button4 = 454, // 0x000001C6
		/// <summary>
		/// Button 5 on sixth joystick
		/// </summary>
		Joystick6Button5 = 455, // 0x000001C7
		/// <summary>
		/// Button 6 on sixth joystick
		/// </summary>
		Joystick6Button6 = 456, // 0x000001C8
		/// <summary>
		/// Button 7 on sixth joystick
		/// </summary>
		Joystick6Button7 = 457, // 0x000001C9
		/// <summary>
		/// Button 8 on sixth joystick
		/// </summary>
		Joystick6Button8 = 458, // 0x000001CA
		/// <summary>
		/// Button 9 on sixth joystick
		/// </summary>
		Joystick6Button9 = 459, // 0x000001CB
		/// <summary>
		/// Button 10 on sixth joystick
		/// </summary>
		Joystick6Button10 = 460, // 0x000001CC
		/// <summary>
		/// Button 11 on sixth joystick
		/// </summary>
		Joystick6Button11 = 461, // 0x000001CD
		/// <summary>
		/// Button 12 on sixth joystick
		/// </summary>
		Joystick6Button12 = 462, // 0x000001CE
		/// <summary>
		/// Button 13 on sixth joystick
		/// </summary>
		Joystick6Button13 = 463, // 0x000001CF
		/// <summary>
		/// Button 14 on sixth joystick
		/// </summary>
		Joystick6Button14 = 464, // 0x000001D0
		/// <summary>
		/// Button 15 on sixth joystick
		/// </summary>
		Joystick6Button15 = 465, // 0x000001D1
		/// <summary>
		/// Button 16 on sixth joystick
		/// </summary>
		Joystick6Button16 = 466, // 0x000001D2
		/// <summary>
		/// Button 17 on sixth joystick
		/// </summary>
		Joystick6Button17 = 467, // 0x000001D3
		/// <summary>
		/// Button 18 on sixth joystick
		/// </summary>
		Joystick6Button18 = 468, // 0x000001D4
		/// <summary>
		/// Button 19 on sixth joystick
		/// </summary>
		Joystick6Button19 = 469, // 0x000001D5
		/// <summary>
		/// Button 0 on seventh joystick
		/// </summary>
		Joystick7Button0 = 470, // 0x000001D6
		/// <summary>
		/// Button 1 on seventh joystick
		/// </summary>
		Joystick7Button1 = 471, // 0x000001D7
		/// <summary>
		/// Button 2 on seventh joystick
		/// </summary>
		Joystick7Button2 = 472, // 0x000001D8
		/// <summary>
		/// Button 3 on seventh joystick
		/// </summary>
		Joystick7Button3 = 473, // 0x000001D9
		/// <summary>
		/// Button 4 on seventh joystick
		/// </summary>
		Joystick7Button4 = 474, // 0x000001DA
		/// <summary>
		/// Button 5 on seventh joystick
		/// </summary>
		Joystick7Button5 = 475, // 0x000001DB
		/// <summary>
		/// Button 6 on seventh joystick
		/// </summary>
		Joystick7Button6 = 476, // 0x000001DC
		/// <summary>
		/// Button 7 on seventh joystick
		/// </summary>
		Joystick7Button7 = 477, // 0x000001DD
		/// <summary>
		/// Button 8 on seventh joystick
		/// </summary>
		Joystick7Button8 = 478, // 0x000001DE
		/// <summary>
		/// Button 9 on seventh joystick
		/// </summary>
		Joystick7Button9 = 479, // 0x000001DF
		/// <summary>
		/// Button 10 on seventh joystick
		/// </summary>
		Joystick7Button10 = 480, // 0x000001E0
		/// <summary>
		/// Button 11 on seventh joystick
		/// </summary>
		Joystick7Button11 = 481, // 0x000001E1
		/// <summary>
		/// Button 12 on seventh joystick
		/// </summary>
		Joystick7Button12 = 482, // 0x000001E2
		/// <summary>
		/// Button 13 on seventh joystick
		/// </summary>
		Joystick7Button13 = 483, // 0x000001E3
		/// <summary>
		/// Button 14 on seventh joystick
		/// </summary>
		Joystick7Button14 = 484, // 0x000001E4
		/// <summary>
		/// Button 15 on seventh joystick
		/// </summary>
		Joystick7Button15 = 485, // 0x000001E5
		/// <summary>
		/// Button 16 on seventh joystick
		/// </summary>
		Joystick7Button16 = 486, // 0x000001E6
		/// <summary>
		/// Button 17 on seventh joystick
		/// </summary>
		Joystick7Button17 = 487, // 0x000001E7
		/// <summary>
		/// Button 18 on seventh joystick
		/// </summary>
		Joystick7Button18 = 488, // 0x000001E8
		/// <summary>
		/// Button 19 on seventh joystick
		/// </summary>
		Joystick7Button19 = 489, // 0x000001E9
		/// <summary>
		/// Button 0 on eighth joystick
		/// </summary>
		Joystick8Button0 = 490, // 0x000001EA
		/// <summary>
		/// Button 1 on eighth joystick
		/// </summary>
		Joystick8Button1 = 491, // 0x000001EB
		/// <summary>
		/// Button 2 on eighth joystick
		/// </summary>
		Joystick8Button2 = 492, // 0x000001EC
		/// <summary>
		/// Button 3 on eighth joystick
		/// </summary>
		Joystick8Button3 = 493, // 0x000001ED
		/// <summary>
		/// Button 4 on eighth joystick
		/// </summary>
		Joystick8Button4 = 494, // 0x000001EE
		/// <summary>
		/// Button 5 on eighth joystick
		/// </summary>
		Joystick8Button5 = 495, // 0x000001EF
		/// <summary>
		/// Button 6 on eighth joystick
		/// </summary>
		Joystick8Button6 = 496, // 0x000001F0
		/// <summary>
		/// Button 7 on eighth joystick
		/// </summary>
		Joystick8Button7 = 497, // 0x000001F1
		/// <summary>
		/// Button 8 on eighth joystick
		/// </summary>
		Joystick8Button8 = 498, // 0x000001F2
		/// <summary>
		/// Button 9 on eighth joystick
		/// </summary>
		Joystick8Button9 = 499, // 0x000001F3
		/// <summary>
		/// Button 10 on eighth joystick
		/// </summary>
		Joystick8Button10 = 500, // 0x000001F4
		/// <summary>
		/// Button 11 on eighth joystick
		/// </summary>
		Joystick8Button11 = 501, // 0x000001F5
		/// <summary>
		/// Button 12 on eighth joystick
		/// </summary>
		Joystick8Button12 = 502, // 0x000001F6
		/// <summary>
		/// Button 13 on eighth joystick
		/// </summary>
		Joystick8Button13 = 503, // 0x000001F7
		/// <summary>
		/// Button 14 on eighth joystick
		/// </summary>
		Joystick8Button14 = 504, // 0x000001F8
		/// <summary>
		/// Button 15 on eighth joystick
		/// </summary>
		Joystick8Button15 = 505, // 0x000001F9
		/// <summary>
		/// Button 16 on eighth joystick
		/// </summary>
		Joystick8Button16 = 506, // 0x000001FA
		/// <summary>
		/// Button 17 on eighth joystick
		/// </summary>
		Joystick8Button17 = 507, // 0x000001FB
		/// <summary>
		/// Button 18 on eighth joystick
		/// </summary>
		Joystick8Button18 = 508, // 0x000001FC
		/// <summary>
		/// Button 19 on eighth joystick
		/// </summary>
		Joystick8Button19 = 509, // 0x000001FD
		LAST = 510,
	}
}
