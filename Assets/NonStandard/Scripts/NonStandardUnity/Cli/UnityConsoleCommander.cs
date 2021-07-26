using NonStandard;
using NonStandard.Cli;
using NonStandard.Data.Parse;
using NonStandard.Utility;
using System;
using UnityEngine;
using Commander = NonStandard.Commands.Commander;

public class UnityConsoleCommander : MonoBehaviour
{
	[System.Serializable] public class CommandEvents {
		[TextArea(1, 10)] public string firstCommands;
		public UnityEvent_string WhenCommandRuns;
	}
	public CommandEvents commandEvents = new CommandEvents();

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
		commandEvents?.WhenCommandRuns?.Invoke(text);
	}
	private void Start() {
		if (!string.IsNullOrEmpty(commandEvents.firstCommands)) {
			UnityConsoleInput consoleInput = GetComponent<UnityConsoleInput>();
			consoleInput._pastedText = commandEvents.firstCommands;
		}
	}
}
