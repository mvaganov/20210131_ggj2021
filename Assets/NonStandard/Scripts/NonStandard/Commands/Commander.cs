using NonStandard.Data.Parse;
using NonStandard.Extension;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NonStandard.Commands {
	public partial class Commander {
		public Dictionary<string, Command> commandLookup = new Dictionary<string, Command>();

		public Action<List<ParseError>> errorListeners;
		/// <summary>every command can be executed by a different user/command-source, and might work differently based on the source</summary>
		[System.Serializable] public class Instruction {
			public string text; public object source;
			public bool IsSource(object a_source) { return source == a_source; }
			public override string ToString() { return "{=CommanderInstruction text:\"" + text + "\"}"; }
			public Instruction() {}
			public Instruction(string text, object source) { this.text = text; this.source = source; }
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
		public Commander() {
			if (_instance != null) { Show.Warning("multiple commanders exist"); }
			InitializeCommands();
		}
		public Command GetCommand(string commandName) {
			commandLookup.TryGetValue(commandName, out Command command);
			return command;
		}
		public void ParseCommand(Instruction instruction, Show.PrintFunc print, out Tokenizer tokenizer) {
			tokenizer = null;
			string trimmed = instruction.text?.Trim();
			if (string.IsNullOrEmpty(trimmed)) return;
			string firstWord = Tokenizer.FirstWord(trimmed);
			if (firstWord == null) { firstWord = trimmed; }
			//Show.Log("1stword " + firstWord.StringifySmall());
			Command command = GetCommand(firstWord);
			if (command != null) {
				tokenizer = command.Tokenize(trimmed);
				//Show.Log(tokenizer);
				if (tokenizer.errors.Count > 0) { return; }
				command.handler.Invoke(new Command.Exec(command, tokenizer, instruction.source, print));
			} else {
				print.Invoke("unknown command \'" + firstWord + "\'\n");
				tokenizer = new Tokenizer();
				tokenizer.Tokenize(trimmed, Command.BaseArgumentParseRules);
			}
			if (tokenizer.errors.Count > 0) { return; }
			Instruction next = NextInstruction(tokenizer, instruction.source);
			if (next == null) { return; }
			ParseCommand(next, print, out tokenizer); // make this a do-while loop instead of tail recursion?
		}
		private Instruction NextInstruction(Tokenizer tokenizer, object source) {
			// check if there is an end-command semicolon delimiter. if so, run this function again starting from that position in the text
			Delim semicolonDelim = CodeRules._instruction_finished_delimiter_ignore_rest[0];
			int semicolonIndex = tokenizer.IndexOf(semicolonDelim);
			if (semicolonIndex >= 0) {
				int startIndex = tokenizer.tokens[semicolonIndex].index + semicolonDelim.text.Length;
				//Show.Log("found next instruction at " + startIndex);
				string nextText = tokenizer.str.Substring(startIndex);
				//Show.Log(nextText);
				return new Instruction(nextText, source);
			}
			return null;
		}
		public void AddCommands(IList<Command> commands) {
			foreach (Command cmd in commands) { AddCommand(cmd); }
		}
		public void AddCommand(Command command) {
			commandLookup[command.Name] = command;
		}
		private void InitializeCommands() {
			//Show.Log("initializing...");
			AddCommandsFrom(this);
			AddCommandsFrom(type:typeof(MoreCommands));
		}
		public void AddCommandsFrom(object obj) { AddCommandsFrom(obj, null); }
		public void AddCommandsFrom(Type type) { AddCommandsFrom(null, type); }
		private void AddCommandsFrom(object obj, Type type) {
			if(obj != null && type == null) type = obj.GetType();
			IEnumerable<MethodInfo> methods = type.FindMethodsWithAttribute<CommandMakerAttribute>();
			//Show.Log(type.Name+" has candidates");
			object[] noparams = Array.Empty<object>();
			foreach (MethodInfo m in methods) {
				//Show.Log(m.Name);
				try {
					Command cmd = m.Invoke(obj, noparams) as Command;
					//Show.Log("adding \'"+cmd.Name+"\' from "+m.Name);
					AddCommand(cmd);
				} catch (Exception) { }
			}
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

	public partial class MoreCommands { }

	public class CommandMakerAttribute : Attribute { }
}
