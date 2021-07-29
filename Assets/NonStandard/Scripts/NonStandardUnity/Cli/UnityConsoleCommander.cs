using NonStandard;
using NonStandard.Cli;
using NonStandard.Commands;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
using NonStandard.Utility;
using System;
using UnityEngine;
using Commander = NonStandard.Commands.Commander;

public partial class UnityConsoleCommander : MonoBehaviour
{
	[TextArea(1, 10)] public string firstCommands;
	public UnityEvent_string WhenCommandRuns;

	public Commander commander = new Commander();
	public void DoCommand(string text) {
		UnityConsole console = GetComponent<UnityConsole>();
		commander.ParseCommand(new Commander.Instruction(text, this), console.Write, out Tokenizer t);
		if (t?.errors?.Count > 0) {
			console.PushForeColor(ConsoleColor.Red);
			console.Write(t.ErrorString());
			Show.Log(t.ErrorString());
			console.PopForeColor();
		}
		WhenCommandRuns?.Invoke(text);
	}
	private void Start() {
		if (!string.IsNullOrEmpty(firstCommands)) {
			UnityConsoleInput consoleInput = GetComponent<UnityConsoleInput>();
			consoleInput._pastedText = firstCommands;
		}
	}
	public void Cmd_Exit(Command.Exec e) { PlatformAdjust.Exit(); }
	public void Cmd_Pause(Command.Exec e) {
		Arguments args = e.GetArgs();
		Show.Log(args);
		if (args.TryGet("0", out bool unpause)) {
			GameClock.Instance().Unpause();
		} else {
			GameClock.Instance().Pause();
		}
	}
	public void Cmd_Help(Command.Exec e) { commander.Cmd_Help_Handler(e); }
#if UNITY_EDITOR
	public void Reset() {
		AddDefaultCommands();
	}
	public void AddDefaultCommands() {
		ConsoleCommands[] cc = GetComponents<ConsoleCommands>();
		if(cc == null || cc.Length == 0) {
			cc = new ConsoleCommands[1];
			cc[0] = gameObject.AddComponent<ConsoleCommands>();
		}
		Command helpCmd = Commander.Cmd_GenerateHelpCommand_static();
		CommandEntry[] DefaultCommandEntries = new CommandEntry[] {
			new CommandEntry("exit", "ends this program", nameof(Cmd_Exit), this),
			new CommandEntry(helpCmd.Name, helpCmd.help, nameof(Cmd_Help), this, ArgumentEntry.GetEntriesFromRealArgs(helpCmd.arguments)),
			new CommandEntry("pause", "pauses the game clock", nameof(Cmd_Pause), this, new ArgumentEntry[] { 
				new ArgumentEntry("unpause","0","unpauses the game clock",valueType:ArgumentEntry.ValueType.Flag),
			}),
		};
		for(int i = 0; i < DefaultCommandEntries.Length; ++i) {
			CommandEntry e = DefaultCommandEntries[i];
			CommandEntry found = GetEntry(e.name, cc);
			if(found == null) { cc[0].newCommands.Add(e); }
		}
	}
#endif
	public enum DevelopmentState { Normal, Deprecated, Preview }
	[System.Serializable] public class CommandEntry {
		public string name;
		public string description;
		public DevelopmentState devState = DevelopmentState.Normal;
		public ArgumentEntry[] arguments;
		public UnityEvent_CommandExec commandExecution = new UnityEvent_CommandExec();
		public void AddToCommander(NonStandard.Commands.Commander cmdr) {
			Command cmd = cmdr.GetCommand(name);
			if (cmd != null && cmd.help == description) { return; }
			Argument[] args = new Argument[arguments.Length];
			int orderedArgumentSlotsFilled = 0;
			for(int i = 0; i < args.Length; ++i) {
				args[i] = arguments[i].GenerateProperArgument();
				if (arguments[i].orderedArgument) {
					args[i].order = ++orderedArgumentSlotsFilled;
				}
			}
			cmdr.AddCommand(new Command(name, commandExecution.Invoke, args, description, 
				devState==DevelopmentState.Deprecated, devState == DevelopmentState.Preview));
		}
		public void RemoveFromCommander(NonStandard.Commands.Commander cmdr) {
			Command cmd = cmdr.GetCommand(name);
			if (cmd == null) { return; }
			cmdr.RemoveCommand(cmd);
		}
		public CommandEntry(string name, string description, string functionName, object functionTarget, 
			ArgumentEntry[] arguments = null, DevelopmentState devState = DevelopmentState.Normal) {
			this.name = name; this.description = description;
			EventBind.On(commandExecution, functionTarget, functionName);
			this.arguments = arguments; this.devState = devState;
		}
	}
	[System.Serializable] public class ArgumentEntry {
		public string name, id, description;
		public DevelopmentState devState = DevelopmentState.Normal;
		public bool required, orderedArgument;
		public ValueType valueType = ValueType.Flag;
		public string defaultValue;
		public enum ValueType { Flag, String, Int, Float, IntArray, FloatArray, StringArray, ByDefaultValue }
		public ArgumentEntry(string name, string id, string description, bool required = false, DevelopmentState devState = DevelopmentState.Normal, bool orderOfValueImpliesArgument = false, ValueType valueType = ValueType.Flag, string defaultValue = null) {
			this.name = name; this.id = id; this.description = description;
			this.required = required; this.devState = devState;
			this.orderedArgument = orderOfValueImpliesArgument; this.valueType = valueType; this.defaultValue = defaultValue;
		}
		public ArgumentEntry(Argument arg) {
			name = arg.Name; id = arg.id; description = arg.description;
			required = arg.required; 
			if (arg.deprecated) { devState = DevelopmentState.Deprecated; }
			else if (arg.preview) { devState = DevelopmentState.Preview; }
			else { devState = DevelopmentState.Normal; }
			if (arg.order > 0) { this.orderedArgument = true; }
			if (arg.flag) { valueType = ValueType.Flag; return; }
			if (arg.defaultValue != null) {
				defaultValue = arg.defaultValue.StringifySmall();
			} else {
				if(arg.valueType == typeof(string)) { valueType = GetValueType(arg.valueType); }
			}
		}
		public static ArgumentEntry[] GetEntriesFromRealArgs(Argument[] args) {
			ArgumentEntry[] arge = new ArgumentEntry[args.Length];
			for (int i = 0; i < arge.Length; ++i) {
				arge[i] = new ArgumentEntry(args[i]);
			}
			return arge;
		}
		public object GetDefaultValue() {
			if (string.IsNullOrEmpty(defaultValue)) { return null; }
			Tokenizer tokenizer = new Tokenizer();
			if(!CodeConvert.TryParse(default, out object result, null, tokenizer)) {
				Debug.LogError(tokenizer.ErrorString());
			}
			return result;
		}
		public ValueType GetValueType(Type t) {
			if (t == typeof(string)) return ValueType.String;
			if (t == typeof(int)) return ValueType.Int;
			if (t == typeof(float)) return ValueType.Float;
			if (t == typeof(int[])) return ValueType.IntArray;
			if (t == typeof(float[])) return ValueType.FloatArray;
			if (t == typeof(string[])) return ValueType.StringArray;
			if (t == typeof(bool[])) return ValueType.Flag;
			return ValueType.ByDefaultValue;
		}
		public Type GetValueType() {
			switch (valueType) {
			case ValueType.String: return typeof(string);
			case ValueType.Int: return typeof(int);
			case ValueType.Float: return typeof(float);
			case ValueType.IntArray: return typeof(int[]);
			case ValueType.FloatArray: return typeof(float[]);
			case ValueType.StringArray: return typeof(string[]);
			}
			return null;
		}
		public bool IsFlag() { return valueType == ValueType.Flag; }
		public Argument GenerateProperArgument() {
			return new Argument(id, name, description, GetDefaultValue(), GetValueType(), -1, required, 
				devState==DevelopmentState.Deprecated, devState == DevelopmentState.Preview, IsFlag());
		}
	}
	public CommandEntry GetEntry(string name, ConsoleCommands[] cc) {
		for (int i = 0; i < cc.Length; ++i) {
			CommandEntry entry = cc[i].GetCommand(name);
			if (entry != null) { return entry; }
		}
		return null;
	}
}
