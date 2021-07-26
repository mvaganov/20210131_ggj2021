using NonStandard.Cli;
using NonStandard.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConsoleCommands : MonoBehaviour
{
	[System.Serializable] public class CommandEntry {
		public string name;
		public string description;
		public UnityEvent action;
		public void Invoke(Command.Exec e) { action.Invoke(); }
		public void AddToCommander(NonStandard.Commands.Commander cmdr) {
			Command cmd = cmdr.GetCommand(name);
			if (cmd != null && cmd.help == description) { return; }
			cmdr.AddCommand(new Command(name, Invoke, help: description));
		}
		public void RemoveFromCommander(NonStandard.Commands.Commander cmdr) {
			Command cmd = cmdr.GetCommand(name);
			if (cmd != null && cmd.help == description) { return; }
			cmdr.RemoveCommand(cmd);
		}
	}
	public List<CommandEntry> newCommands = new List<CommandEntry>();
	public enum Active {
		AlwaysUseCommands, DoNotUseCommands, UseOnlyIfComponentIsActive
	}
	public Active whenToUse;
	public Active UseTheseCommands {
		get => whenToUse;
		set {
			switch (whenToUse) {
			case Active.AlwaysUseCommands: newCommands.ForEach(c => c.AddToCommander(GetCommander())); break;
			case Active.DoNotUseCommands: newCommands.ForEach(c => c.RemoveFromCommander(GetCommander())); break;
			case Active.UseOnlyIfComponentIsActive:
				if (isActiveAndEnabled) {
					newCommands.ForEach(c => c.AddToCommander(GetCommander()));
				} else {
					newCommands.ForEach(c => c.RemoveFromCommander(GetCommander()));
				}
				break;
			}
			whenToUse = value;
		}
	}

	NonStandard.Commands.Commander _commander;
	public NonStandard.Commands.Commander GetCommander() {
		return _commander != null ? _commander : _commander = GetComponent<UnityConsoleCommander>().commander;
	}
	private void OnEnable() { if (whenToUse == Active.UseOnlyIfComponentIsActive) { UseTheseCommands = whenToUse; } }
	private void OnDisable() { if (whenToUse == Active.UseOnlyIfComponentIsActive) { UseTheseCommands = whenToUse; } }
}
