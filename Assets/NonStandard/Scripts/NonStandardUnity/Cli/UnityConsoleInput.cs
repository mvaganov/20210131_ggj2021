using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
using NonStandard.Inputs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Cli {
	[RequireComponent(typeof(UnityConsole))]
	public class UnityConsoleInput : MonoBehaviour {
		public Color inputColor = new Color(1, 1, 0, 0.5f);
		int inputColorCode = -1;
		private UnityConsole console;

		public Commands.Commander commander = new Commands.Commander();

		[System.Serializable] public class Command {
			public string command;
			public UnityEvent action;
		}
		public List<Command> commands = new List<Command>();
		public Command ExecuteCommand(string command) {
			Tokenizer t = Tokenizer.Tokenize(command);
			Show.Log(t.GetStr(0)+" : "+t.ToString());
			commander.ParseCommand(t, this, console.Write);
			if(t.errors.Count > 0) {
				console.PushForeColor(ConsoleColor.Red);
				console.Write(t.ErrorString());
				console.PopForeColor();
			}
			////t.ToString()
			////commander.AddCommands;
			//for (int i = 0; i < commands.Count; ++i) {
			//	Command c = commands[i];
			//	if (command.StartsWith(c.command)) { c.action.Invoke(); return c; }
			//}
			return null;
		}

		Dictionary<KCode, (char, char)> qwertyKeyMap = new Dictionary<KCode, (char, char)>() {
			[KCode.BackQuote] = ('`', '~'),
			[KCode.Alpha0] = ('0', ')'),
			[KCode.Alpha1] = ('1', '!'),
			[KCode.Alpha2] = ('2', '@'),
			[KCode.Alpha3] = ('3', '#'),
			[KCode.Alpha4] = ('4', '$'),
			[KCode.Alpha5] = ('5', '%'),
			[KCode.Alpha6] = ('6', '^'),
			[KCode.Alpha7] = ('7', '&'),
			[KCode.Alpha8] = ('8', '*'),
			[KCode.Alpha9] = ('9', '('),
			[KCode.Minus] = ('-', '_'),
			[KCode.Equals] = ('=', '+'),
			[KCode.Tab] = ('\t', '\t'),
			[KCode.LeftBracket] = ('[', '{'),
			[KCode.RightBracket] = (']', '}'),
			[KCode.Backslash] = ('\\', '|'),
			[KCode.Semicolon] = (';', ':'),
			[KCode.Quote] = ('\'', '\"'),
			[KCode.Comma] = (',', '<'),
			[KCode.Period] = ('.', '>'),
			[KCode.Slash] = ('/', '?'),
			[KCode.Space] = (' ', ' '),
			[KCode.Backspace] = ('\b', '\b'),
			[KCode.Return] = ('\n', '\n'),
			[KCode.A] = ('a', 'A'),
			[KCode.B] = ('b', 'B'),
			[KCode.C] = ('c', 'C'),
			[KCode.D] = ('d', 'D'),
			[KCode.E] = ('e', 'E'),
			[KCode.F] = ('f', 'F'),
			[KCode.G] = ('g', 'G'),
			[KCode.H] = ('h', 'H'),
			[KCode.I] = ('i', 'I'),
			[KCode.J] = ('j', 'J'),
			[KCode.K] = ('k', 'K'),
			[KCode.L] = ('l', 'L'),
			[KCode.M] = ('m', 'M'),
			[KCode.N] = ('n', 'N'),
			[KCode.O] = ('o', 'O'),
			[KCode.P] = ('p', 'P'),
			[KCode.Q] = ('q', 'Q'),
			[KCode.R] = ('r', 'R'),
			[KCode.S] = ('s', 'S'),
			[KCode.T] = ('t', 'T'),
			[KCode.U] = ('u', 'U'),
			[KCode.V] = ('v', 'V'),
			[KCode.W] = ('w', 'W'),
			[KCode.X] = ('x', 'X'),
			[KCode.Y] = ('y', 'Y'),
			[KCode.Z] = ('z', 'Z'),
		};
		public bool KeyAvailable {
			get {
				foreach (KeyValuePair<KCode, (char, char)> kvp in qwertyKeyMap) { if (kvp.Key.IsHeld()) return true; }
				return false;
			}
		}
		public List<KCode> GetKeyAvailabe(List<KCode> list) {
			foreach (KeyValuePair<KCode, (char, char)> kvp in qwertyKeyMap) { if (kvp.Key.IsHeld()) { list.Add(kvp.Key); } }
			return list;
		}
		public List<KCode> GetKeyDown(List<KCode> list) {
			foreach (KeyValuePair<KCode, (char, char)> kvp in qwertyKeyMap) { if (kvp.Key.IsDown()) { list.Add(kvp.Key); } }
			return list;
		}
		List<Action<KCode>> kCodeListeners = new List<Action<KCode>>();
		public void Read(Action<KCode> kCodeListener) { kCodeListeners.Add(kCodeListener); }
		List<Show.PrintFunc> lineInputListeners = new List<Show.PrintFunc>();
		public void ReadLine(Show.PrintFunc lineInputListener) { lineInputListeners.Add(lineInputListener); }
		StringBuilder currentLine = new StringBuilder();

		List<KCode> keysDown = new List<KCode>();
		public string GetKeyInput() {
			keysDown.Clear();
			GetKeyDown(keysDown);
			if (kCodeListeners != null && keysDown.Count > 0) {
				kCodeListeners.ForEach(action => action.Invoke(keysDown[0]));
				kCodeListeners.Clear();
			}
			StringBuilder sb = new StringBuilder();
			bool isShift = KCode.AnyShift.IsHeld();
			for (int i = 0; i < keysDown.Count; ++i) {
				if (qwertyKeyMap.TryGetValue(keysDown[i], out (char, char) kodes)) {
					sb.Append(!isShift ? kodes.Item1 : kodes.Item2);
				}
			}
			return sb.ToString();
		}

		public string GetInputSoFar() {
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
				default: finalString.Append(c); break;
				}
			}
			return finalString.ToString();
		}
		public bool IsListeningToLine() { return lineInputListeners != null && lineInputListeners.Count > 0; }
		public bool CurrentLineUpdate(string txt) {
			currentLine.Append(txt);
			if (txt.Contains("\n")) {
				string completeInput = GetInputSoFar();
				Show.Log(completeInput);
				if (IsListeningToLine()) {
					lineInputListeners.ForEach(action => action.Invoke(completeInput));
					lineInputListeners.Clear();
				}
				currentLine.Clear();
				ExecuteCommand(completeInput);
				return true;
			}
			return false;
		}
		public bool UpdateKey() {
			string txt = GetKeyInput();
			if (!string.IsNullOrEmpty(txt)) {
				if (IsListeningToLine() && currentLine.Length <= 0 && txt.Contains("\b")) {
					txt = txt.Replace("\b", "");
				}
				if (inputColorCode > 0) { console.PushForeColor((byte)inputColorCode); }
				console.Write(txt);
				if (inputColorCode > 0) { console.PopForeColor(); }
				CurrentLineUpdate(txt);
				return true;
			}
			Coord move = Coord.Zero;
			if (Input.GetKeyDown(KeyCode.LeftArrow)) { move = Coord.Left; }
			if (Input.GetKeyDown(KeyCode.RightArrow)) { move = Coord.Right; }
			if (Input.GetKeyDown(KeyCode.UpArrow)) { move = Coord.Up; }
			if (Input.GetKeyDown(KeyCode.DownArrow)) { move = Coord.Down; }
			if (move != Coord.Zero) {
				if (KCode.AnyShift.IsHeld()) {
					console.ScrollRenderWindow(move);
				} else {
					console.MoveCursor(move);
				}
				return true;
			}
			return false;
		}

		private void Awake() { console = GetComponent<UnityConsole>(); }
		private void Start() {
			inputColorCode = console.AddConsoleColor(inputColor);
		}
		void Update() {
			if (UpdateKey()) { console.RefreshText(); }
		}
	}
}