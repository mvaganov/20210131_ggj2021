using NonStandard.Commands;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Cli {
	[System.Serializable] public class UnityEvent_CommandExec : UnityEvent<Command.Exec> { }
	public class ConsoleCommands : MonoBehaviour {
		public Active whenToUse;
		public List<UnityConsoleCommander.CommandEntry> newCommands = new List<UnityConsoleCommander.CommandEntry>();
		NonStandard.Commands.Commander _commander;
		public enum Active {
			AlwaysUseCommands, DoNotUseCommands, UseOnlyIfComponentIsActive
		}
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
		public int GetCommandIndex(string name) {
			for (int i = 0; i < newCommands.Count; ++i) {
				if (newCommands[i].name == name) return i;
			}
			return -1;
		}
		public UnityConsoleCommander.CommandEntry GetCommand(string name) {
			int i = GetCommandIndex(name);
			if (i >= 0) return newCommands[i];
			return null;
		}
		public void SetCommand(UnityConsoleCommander.CommandEntry command) {
			int i = GetCommandIndex(command.name);
			if (i >= 0) { newCommands[i] = command; return; }
			newCommands.Add(command);
		}
		public NonStandard.Commands.Commander GetCommander() {
			return _commander != null ? _commander : _commander = GetComponent<UnityConsoleCommander>().CommanderInstance;
		}
		private void OnEnable() { if (whenToUse != Active.DoNotUseCommands) { UseTheseCommands = whenToUse; } }
		private void OnDisable() { if (whenToUse == Active.UseOnlyIfComponentIsActive) { UseTheseCommands = whenToUse; } }
	}
}