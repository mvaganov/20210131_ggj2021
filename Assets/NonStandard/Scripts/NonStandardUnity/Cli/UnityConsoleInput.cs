using NonStandard.Commands;
using NonStandard.Data;
using NonStandard.Data.Parse;
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
	public class UnityConsoleInput : MonoBehaviour {
		protected StringBuilder currentLine = new StringBuilder();
		protected List<KCode> keysDown = new List<KCode>();
		protected string _pastedText;
		/// <summary>
		/// this is an 'any key' listener, added to by the <see cref="Read(Action{KCode})"/> function
		/// </summary>
		protected List<Action<KCode>> tempKeyCodeListeners = new List<Action<KCode>>();
		/// <summary>
		/// added to by the <see cref="ReadLine(Show.PrintFunc)"/> function
		/// </summary>
		protected List<Show.PrintFunc> tempLineInputListeners = new List<Show.PrintFunc>();

		public string promptArtifact = "$"; // TODO implement
		public Color inputColor = new Color(1, 1, 0, 0.5f);
		int inputColorCode = -1;
		private UnityConsole console;

		public Commands.Commander commander = new Commands.Commander();
		public bool acceptCommand = true;
		public bool clipboardPaste = true;
		public bool interceptDebugLog = false; // TODO

		[System.Serializable] public class CommandEvents {
			[TextArea(1, 10)] public string firstCommands;
			public UnityEvent_string WhenCommandRuns;
		}
		public CommandEvents commandEvents = new CommandEvents();

		[System.Serializable] public class CommandEntry {
			public string name;
			public string description;
			public UnityEvent action;
			public void Invoke(Command.Exec e) { action.Invoke(); }
			public void AddToCommander(Commands.Commander cmdr) { cmdr.AddCommand(new Command(name, Invoke, help: description)); }
		}
		public List<CommandEntry> newCommands = new List<CommandEntry>();
		public void DoCommand(string text) {
			commander.ParseCommand(new Commands.Commander.Instruction(text,this), console.Write, out Tokenizer t);
			if (t?.errors?.Count > 0) {
				console.PushForeColor(ConsoleColor.Red);
				console.Write(t.ErrorString());
				Show.Log(t.ErrorString());
				console.PopForeColor();
			}
		}
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
			[KCode.Return] = new KMap('\n', '\n'),
			[KCode.A] = new KMap('a', 'A'),
			[KCode.B] = new KMap('b', 'B'),
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
			[KCode.V] = new KMap('v', 'V', new Action(PasteFromClipboard)),
			[KCode.W] = new KMap('w', 'W'),
			[KCode.X] = new KMap('x', 'X'),
			[KCode.Y] = new KMap('y', 'Y'),
			[KCode.Z] = new KMap('z', 'Z'),
			[KCode.UpArrow] = new KMap(new Action(() => MovCur(Coord.Up)), new Action(() => MovWin(Coord.Up))),
			[KCode.LeftArrow] = new KMap(new Action(() => MovCur(Coord.Left)), new Action(() => MovWin(Coord.Left))),
			[KCode.DownArrow] = new KMap(new Action(() => MovCur(Coord.Down)), new Action(() => MovWin(Coord.Down))),
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
			StringBuilder sb = new StringBuilder();
			AddPastedTextToInput(sb);
			AddKeysToInput(sb, alsoResolveNonText);
			return sb.ToString();
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
			bool isCtrl = KCode.AnyControl.IsHeld(), isShift = KCode.AnyShift.IsHeld();
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
				case '\n': return finalString.ToString();
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
		public bool CommandLineUpdate(string txt) {
			currentLine.Append(txt);
			if (!txt.Contains("\n")) { return false; }
			string processedInput = ProcessInput(currentLine.ToString());
			currentLine.Clear();
			//Show.Log(completeInput);
			if (IsListeningToLine()) {
				tempLineInputListeners.ForEach(action => action.Invoke(processedInput));
				tempLineInputListeners.Clear();
			}
			if (acceptCommand) {
				DoCommand(processedInput);
				commandEvents?.WhenCommandRuns?.Invoke(processedInput);
			}
			return true;
		}
		private void Awake() { console = GetComponent<UnityConsole>(); }
		private void Start() {
			inputColorCode = console.AddConsoleColor(inputColor);
			newCommands.ForEach(c => c.AddToCommander(commander));
			if (!string.IsNullOrEmpty(commandEvents.firstCommands)) {
				_pastedText = commandEvents.firstCommands;
			}
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