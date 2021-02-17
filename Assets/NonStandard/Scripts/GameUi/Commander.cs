using NonStandard.Data;
using NonStandard.Data.Parse;
using System;
using System.Collections.Generic;

namespace NonStandard.GameUi {
	public class Commander {
		Dictionary<string, Action<object, Tokenizer>> commandListing = null;
		public Action<List<ParseError>> onErrors;
		private static Commander _instance;
		public static Commander Instance { get { return (_instance != null) ? _instance : _instance = new Commander(); } }

		public object _scope;
		public void SetScope(object scope) { _scope = scope; }
		public object GetScope() { return _scope; }
		private Commander() { InitializeCommands(); }
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
			if (tok.errors.Count > 0 && onErrors != null) {
				onErrors.Invoke(tok.errors);
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
	}
}
