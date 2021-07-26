using NonStandard.Data;
using NonStandard.Extension;
using NonStandard.Inputs;
using NonStandard.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Cli {
	[RequireComponent(typeof(UnityConsole))]
	public class UnityConsoleInput : UserInput {
		protected StringBuilder currentLine = new StringBuilder();
		protected List<KCode> keysDown = new List<KCode>();
		internal string _pastedText;
		/// <summary>
		/// this is an 'any key' listener, added to by the <see cref="Read(Action{KCode})"/> function
		/// </summary>
		protected List<Action<KCode>> tempKeyCodeListeners = new List<Action<KCode>>();
		/// <summary>
		/// added to by the <see cref="ReadLine(Show.PrintFunc)"/> function
		/// </summary>
		protected List<Show.PrintFunc> tempLineInputListeners = new List<Show.PrintFunc>();
		/// <summary>
		/// if false, typing (and pasting) will not add characters to the current input, or to the console
		/// </summary>
		public bool textInput = true;
		public bool clipboardPaste = true;

		public Color inputColor = new Color(1, 1, 0);
		int inputColorCode = -1;
		private UnityConsole console;

		public UnityEvent_string inputListener;

		// TODO remove the ctrl option, since it should be in the KeyBinds dictionary
		private struct KMap {
			public object normal, shift, ctrl;
			public KMap(object n, object s=null, object c=null) { normal = n;shift = s;ctrl = c; }
		}
		Dictionary<KCode, KMap> _keyMap = null;
		Dictionary<KCode, KMap> KepMap() => _keyMap != null
			? _keyMap : _keyMap = new Dictionary<KCode, KMap>() {
			[KCode.BackQuote] = new KMap('`', '~'),
			[KCode.Alpha0] = new KMap('0', ')'),
			[KCode.Alpha1] = new KMap('1', '!'),
			[KCode.Alpha2] = new KMap('2', '@'),
			[KCode.Alpha3] = new KMap('3', '#'),
			[KCode.Alpha4] = new KMap('4', '$'),
			[KCode.Alpha5] = new KMap('5', '%'),
			[KCode.Alpha6] = new KMap('6', '^'),
			[KCode.Alpha7] = new KMap('7', '&'),
			[KCode.Alpha8] = new KMap('8', '*'),
			[KCode.Alpha9] = new KMap('9', '('),
			[KCode.Minus] = new KMap('-', '_'),
			[KCode.Equals] = new KMap('=', '+'),
			[KCode.Tab] = new KMap('\t'),
			[KCode.LeftBracket] = new KMap('[', '{'),
			[KCode.RightBracket] = new KMap(']', '}'),
			[KCode.Backslash] = new KMap('\\', '|'),
			[KCode.Semicolon] = new KMap(';', ':'),
			[KCode.Quote] = new KMap('\'', '\"'),
			[KCode.Comma] = new KMap(',', '<'),
			[KCode.Period] = new KMap('.', '>'),
			[KCode.Slash] = new KMap('/', '?'),
			[KCode.Space] = new KMap(' ', ' '),
			[KCode.Backspace] = new KMap('\b', '\b'),
			[KCode.Return] = new KMap(null, '\n'),
			[KCode.A] = new KMap('a', 'A'),
			[KCode.B] = new KMap('b', 'B'),
			// TODO add CopyToClipboard to KeyBinds in the Reset function
			[KCode.C] = new KMap('c', 'C', new Action(CopyToClipboard)),
			[KCode.D] = new KMap('d', 'D'),
			[KCode.E] = new KMap('e', 'E'),
			[KCode.F] = new KMap('f', 'F'),
			[KCode.G] = new KMap('g', 'G'),
			[KCode.H] = new KMap('h', 'H'),
			[KCode.I] = new KMap('i', 'I'),
			[KCode.J] = new KMap('j', 'J'),
			[KCode.K] = new KMap('k', 'K'),
			[KCode.L] = new KMap('l', 'L'),
			[KCode.M] = new KMap('m', 'M'),
			[KCode.N] = new KMap('n', 'N'),
			[KCode.O] = new KMap('o', 'O'),
			[KCode.P] = new KMap('p', 'P'),
			[KCode.Q] = new KMap('q', 'Q'),
			[KCode.R] = new KMap('r', 'R'),
			[KCode.S] = new KMap('s', 'S'),
			[KCode.T] = new KMap('t', 'T'),
			[KCode.U] = new KMap('u', 'U'),
			// TODO add PasteFromClipboard to KeyBinds in the Reset function
			[KCode.V] = new KMap('v', 'V', new Action(PasteFromClipboard)),
			[KCode.W] = new KMap('w', 'W'),
			[KCode.X] = new KMap('x', 'X'),
			[KCode.Y] = new KMap('y', 'Y'),
			[KCode.Z] = new KMap('z', 'Z'),
			// TODO add this to KeyBinds in the Reset function
			[KCode.UpArrow] = new KMap(new Action(() => MovCur(Coord.Up)), new Action(() => MovWin(Coord.Up))),
			// TODO add this to KeyBinds in the Reset function
			[KCode.LeftArrow] = new KMap(new Action(() => MovCur(Coord.Left)), new Action(() => MovWin(Coord.Left))),
			// TODO add this to KeyBinds in the Reset function
			[KCode.DownArrow] = new KMap(new Action(() => MovCur(Coord.Down)), new Action(() => MovWin(Coord.Down))),
			// TODO add this to KeyBinds in the Reset function
			[KCode.RightArrow] = new KMap(new Action(() => MovCur(Coord.Right)), new Action(() => MovWin(Coord.Right))),
		};
		private void MovCur(Coord dir) { console.Cursor += dir; }
		private void MovWin(Coord dir) { console.ScrollRenderWindow(dir); ; }
		public void PasteFromClipboard() {
			if (!clipboardPaste) return;
			_pastedText = GUIUtility.systemCopyBuffer.Replace("\r","");
		}
		public void CopyToClipboard() {
			Show.Log("copy mechanism from Input should be working: " + GUIUtility.systemCopyBuffer.StringifySmall());
		}
		public bool KeyAvailable {
			get {
				foreach (KeyValuePair<KCode, KMap> kvp in KepMap()) { if (kvp.Key.IsHeld()) return true; }
				return false;
			}
		}
		public List<KCode> GetKeyAvailabe(List<KCode> list) {
			foreach (KeyValuePair<KCode, KMap> kvp in KepMap()) { if (kvp.Key.IsHeld()) { list.Add(kvp.Key); } }
			return list;
		}
		public List<KCode> GetKeyDown(List<KCode> list) {
			foreach (KeyValuePair<KCode, KMap> kvp in KepMap()) { if (kvp.Key.IsDown()) { list.Add(kvp.Key); } }
			return list;
		}
		public void Read(Action<KCode> kCodeListener) { tempKeyCodeListeners.Add(kCodeListener); }
		public void ReadLine(Show.PrintFunc lineInputListener) { tempLineInputListeners.Add(lineInputListener); }
		public string ResolveInput(bool alsoResolveNonText) {
			StringBuilder sb = textInput ? new StringBuilder() : null;
			AddPastedTextToInput(sb);
			AddKeysToInput(sb, alsoResolveNonText);
			return sb?.ToString() ?? null;
		}
		void AddPastedTextToInput(StringBuilder sb) {
			if (_pastedText == null) { return; }
			sb.Append(_pastedText);
			_pastedText = null;
		}
		void AddKeysToInput(StringBuilder sb, bool alsoResolveNonText = true) {
			keysDown.Clear();
			GetKeyDown(keysDown);
			if (tempKeyCodeListeners != null && keysDown.Count > 0) {
				keysDown.ForEach(keyDown => tempKeyCodeListeners.ForEach(action => action.Invoke(keyDown)));
				tempKeyCodeListeners.Clear();
			}
			bool isCtrl = KCode.AnyCtrl.IsHeld(), isShift = KCode.AnyShift.IsHeld();
			for (int i = 0; i < keysDown.Count; ++i) {
				if (_keyMap.TryGetValue(keysDown[i], out KMap kmap)) {
					if (isCtrl) { DoTheThing(kmap.ctrl); } else if (isShift) { DoTheThing(kmap.shift); } else { DoTheThing(kmap.normal); }
				}
			}
			void DoTheThing(object context) {
				switch (context) {
				case char c: sb?.Append(c); break;
				case Action a: if (alsoResolveNonText) a.Invoke(); break;
				}
			}
		}
		public static string ProcessInput(string currentLine) {
			StringBuilder finalString = new StringBuilder();
			for (int i = 0; i < currentLine.Length; ++i) {
				char c = currentLine[i];
				switch (c) {
				case '\b':
					if (finalString.Length > 0) {
						finalString.Remove(finalString.Length - 1, 1);
					}
					break;
				//case '\n': return finalString.ToString();
				case '\r': break;
				case '\\':
					if (++i < currentLine.Length) { c = currentLine[i]; }
					finalString.Append(c); break;
				default: finalString.Append(c); break;
				}
			}
			return finalString.ToString();
		}
		bool IsListeningToLine() { return tempLineInputListeners != null && tempLineInputListeners.Count > 0; }
		public void CommandLineUpdate(string txt) {
			currentLine.Append(txt);
		}
		public void FinishCurrentInput() {
			string processedInput = ProcessInput(currentLine.ToString());
			Show.Log(currentLine.ToString().StringifySmall()+" -> "+processedInput.StringifySmall());
			currentLine.Clear();
			console.Write("\n");
			if (string.IsNullOrEmpty(processedInput)) { return; }
			inputListener.Invoke(processedInput);
			if (IsListeningToLine()) {
				tempLineInputListeners.ForEach(action => action.Invoke(processedInput));
				tempLineInputListeners.Clear();
			}
		}
		void IncreaseFontSize() { console.AddToFontSize(1); }
		private void Reset() {
			UnityConsole console = GetComponent<UnityConsole>();
			UnityConsoleCommander conCom = GetComponent<UnityConsoleCommander>();
			if (conCom != null) {
				EventBind invokeSet = new EventBind(conCom, nameof(conCom.DoCommand), "blarg");
				invokeSet.Bind(inputListener);
			}
			KeyBinds.Add(new KBind(new KCombo(KCode.Equals, KModifier.AnyCtrl), "+ console font", 
				pressFunc: new EventBind(console, nameof(console.AddToFontSize), 1f)));
			KeyBinds.Add(new KBind(new KCombo(KCode.Minus, KModifier.AnyCtrl), "- console font", 
				pressFunc: new EventBind(console, nameof(console.AddToFontSize), -1f)));
			KeyBinds.Add(new KBind(new KCombo(KCode.Return, KModifier.NoShift), "submit console input",
				pressFunc: new EventBind(this, nameof(FinishCurrentInput), null)));
		}
		private void Awake() { console = GetComponent<UnityConsole>(); }
		private void Start() {
			inputColorCode = console.AddConsoleColor(inputColor);
		}
		public void WriteInputText(string txt) {
			if (inputColorCode > 0) { console.PushForeColor((byte)inputColorCode); }
			console.Write(txt);
			if (inputColorCode > 0) { console.PopForeColor(); }
		}
		void Update() {
			string txt = ResolveInput(true);
			if (string.IsNullOrEmpty(txt)) { return; }
			WriteInputText(txt);
			CommandLineUpdate(txt);
		}
	}
}