using NonStandard.Data.Parse;
using System;
using System.Collections.Generic;

namespace NonStandard.Commands {
	public class Commander {
		Dictionary<string, Action<object, Tokenizer>> commandListing = null;
		public Action<List<ParseError>> errorListeners;
		/// <summary>every command can be executed by a different user, and might work differently based on user</summary>
		[System.Serializable]
		public class Instruction {
			public string text; public object source;
			public bool IsSource(object a_source) { return source == a_source; }
			public override string ToString() { return "(Instruction){text:\"" + text + "\"}"; }
		}
		/// <summary>queue of instructions that this command line needs to execute.</summary>
		private List<Instruction> instructionList = new List<Instruction>();
		/// <summary>useful for callbacks, for finding out what is going on right now</summary>
		public Instruction RecentInstruction;


		private static Commander _instance;
		public static Commander Instance { get { return (_instance != null) ? _instance : _instance = new Commander(); } }

		public object _scope;
		public void SetScope(object scope) { _scope = scope; }
		public object GetScope() { return _scope; }
		private Commander() { InitializeCommands(); }
		public void ParseCommand(Instruction instruction) { ParseCommand(instruction.source, instruction.text); }
		public void ParseCommand(object source, string command) {
			Tokenizer cmdTok = new Tokenizer();
			cmdTok.Tokenize(command);
			int iter = 0;
			do {
				string cmd = cmdTok.GetResolvedToken(0, GetScope()).ToString();
				ExecuteCommand(source, cmd, cmdTok);
				++iter;
			} while (RemoveFirstCommand(cmdTok));
		}
		public bool RemoveFirstCommand(Tokenizer cmdTok) {
			for (int i = 0; i < cmdTok.TokenCount; ++i) {
				Token t = cmdTok.GetToken(i);
				Delim d = t.GetAsDelimiter();
				if (d != null && d.text == ";") {
					cmdTok.PopTokens(i + 1);
					return true;
				}
			}
			return false;
		}
		public void ExecuteCommand(object source, string command, Tokenizer tok) {
			//Show.Log(command+": "+tok.DebugPrint());
			Action<object,Tokenizer> commandToExecute;
			if (commandListing.TryGetValue(command, out commandToExecute)) {
				commandToExecute.Invoke(source, tok);
			} else {
				tok.AddError("unknown command \'" + command + "\'");
			}
			if (tok.errors.Count > 0 && errorListeners != null) {
				errorListeners.Invoke(tok.errors);
				tok.errors.Clear();
			}
		}
		public void AddCommands(Dictionary<string, Action<object, Tokenizer>> commands) {
			foreach(KeyValuePair<string, Action<object, Tokenizer>> kvp in commands) {
				AddCommand(kvp.Key, kvp.Value);
			}
		}
		public void AddCommand(string command, Action<object, Tokenizer> commandAction) {
			commandListing[command] = commandAction;
		}
		private void InitializeCommands() {
			if (commandListing != null) return; 
			commandListing = new Dictionary<string, Action<object,Tokenizer>>() {
				["exit"] = (s,t) => PlatformAdjust.Exit(),
			};
		}

		public Instruction PopInstruction() {
			if (instructionList.Count > 0) {
				RecentInstruction = instructionList[0];
				instructionList.RemoveAt(0);
				return RecentInstruction;
			}
			return null;
		}
		/// <summary>Enqueues a command to run, which will be run during the next Update</summary>
		/// <param name="instruction">Command string, with arguments.</param>
		public void EnqueueRun(Instruction instruction) { instructionList.Add(instruction); }
	}
}
