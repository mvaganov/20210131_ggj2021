#define CONNECT_TO_REAL_COMMAND_LINE_TERMINAL
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Cli {
	public class Commander
	{
		public CmdLine_base cmd;

#if !CONNECT_TO_REAL_COMMAND_LINE_TERMINAL
			public void DoSystemCommand(string command, object whosAsking = null) {
				Debug.LogWarning(whosAsking+" can't do system command '"+command+
					"', #define CONNECT_TO_REAL_COMMAND_LINE_TERMINAL");
			}
#else
		public bool AllowSystemAccess = true;
		public SystemBridge bash;
		public void DoSystemCommand(string command, object whosAsking = null) {
			bash.DoCommand(command, (whosAsking == null) ? bash : whosAsking, null, cmd);
		}
#endif

		public void PopulateBasicCommands()
		{
#if CONNECT_TO_REAL_COMMAND_LINE_TERMINAL
			addCommand("cmd", (args, user) => {
				if (AllowSystemAccess)
				{
					bash.DoCommand(string.Join(" ", args, 1, args.Length - 1), this, null, cmd);
				} else
				{
					cmd.HandleLog("Access Denied", "", LogType.Warning);
				}
			}, "access the true system's command-line terminal");
#endif
		}

		/// <summary>if delegates are here, calls this code instead of executing a known a command</summary>
		public event CmdLine_base.DoAfterStringIsRead waitingToReadLine;
		/// <summary>If this is set, ignore the native command line functionality, and just do this</summary>
		public CmdLine_base.DoAfterStringIsRead onInput;

		/// <summary>watching for commands *about to execute*.</summary>
		public event CommandHandler OnCommand;
		/// <summary>known commands</summary>
		public Dictionary<string, Command> commands = new Dictionary<string, Command>();

		public bool IsDefaultBehavior()
		{
			return waitingToReadLine == null || waitingToReadLine.GetInvocationList().Length == 0;
		}

		/// <summary>every command can be executed by a different user</summary>
		[System.Serializable]
		public class Instruction
		{
			public string text; public object user;
			public bool IsUser(object a_user) { return user == a_user; }
			public override string ToString() { return "(Instruction){text:\""+text+"\"}"; }
		}
		/// <summary>queue of instructions that this command line needs to execute.</summary>
		private List<Instruction> instructionList = new List<Instruction>();
		public Instruction PopInstruction()
		{
			if (instructionList.Count > 0)
			{
				RecentInstruction = instructionList[0];
				instructionList.RemoveAt(0);
				return RecentInstruction;
			}
			return null;
		}
		[Tooltip("Easily accessible way of finding out what instruction was executed last")]
		/// <summary>useful for callbacks, for finding out what is going on right now</summary>
		public Instruction RecentInstruction;

		/// <param name="command">name of the command to add (case insensitive)</param>
		/// <param name="handler">code to execute with this command, think standard main</param>
		/// <param name="help">reference information, think concise man-pages. Make help <c>"\b"</c> for hidden commands</param>
		public void addCommand(string command, CommandHandler handler, string help)
		{
			commands.Add(command.ToLower(), new Command(command, handler, help));
		}
		/// <param name="commands">dictionary of commands to begin using, replacing old commands</param>
		public void SetCommands(Dictionary<string, Command> commands) { this.commands = commands; }
		/// <summary>replace current commands with no commands</summary>
		public void ClearCommands() { commands.Clear(); }

		/// <summary>command-line handler. think "standard main" from Java or C/C++.
		/// args[0] is the command, args[1] and beyond are the arguments.</summary>
		public delegate void CommandHandler(string[] args, object whosAsking);

		public class Command
		{
			public string command { get; private set; }
			public CommandHandler handler { get; private set; }
			public string help { get; private set; }
			public Command(string command, CommandHandler handler, string helpReferenceText)
			{
				this.command = command; this.handler = handler; this.help = helpReferenceText;
			}
		}

		public string CommandPromptArtifact()
		{
			string promptText = cmd.PromptArtifact;
#if CONNECT_TO_REAL_COMMAND_LINE_TERMINAL
			if (cmd.commander.bash.IsInitialized())
			{
				promptText = bash.MachineName + cmd.PromptArtifact;
			}
#endif
			return promptText;
		}

		/// <returns>a list of usable commands</returns>
		public string CommandHelpString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (Command cmd in commands.Values)
			{
				if (cmd.help == "\b") // BECAUSE commands with a single backspace as help text are hidden commands 
					continue;
				sb.Append(((sb.Length > 0) ? "\n" : "") + cmd.command + ": " + cmd.help);
			}
			return sb.ToString();
		}
		/// <summary>Enqueues a command to run, which will be run during the next Update</summary>
		/// <param name="instruction">Command string, with arguments.</param>
		public void EnqueueRun(Instruction instruction)
		{
			instructionList.Add(instruction);
			if (instruction.IsUser(cmd.UserRawInput))
			{
				cmd.indexWherePromptWasPrintedRecently = new Vector2Int(-1,-1); // make sure this command stays visible
			}
		}
		public void Dispatch(Instruction instruction)
		{
			if (waitingToReadLine != null) {
				waitingToReadLine(instruction.text);
				waitingToReadLine = null;
			} else if (onInput != null) {
				onInput(instruction.text);
			} else
			{
				if (string.IsNullOrEmpty(instruction.text)) { return; }
				//Debug.Log("A "+ instruction.text);
				string s = instruction.text.Trim(Util.WHITESPACE); // cut leading & trailing whitespace
				//Debug.Log("B " + instruction.text);
				string[] args = Util.ParseArguments(s).ToArray();
				//Debug.Log("C " + string.Join(":", args));
				if (args.Length < 1) { return; }
				if (OnCommand != null) { OnCommand(args, instruction.user); }
				RunDispatcher(args[0].ToLower(), args, instruction.user);
			}
		}

		/// <summary>if the given text is a tag, returns the tag with noparse around it.</summary>
		/// TODO should this be in another class?
		public static string NoparseFilterAroundTag(string text)
		{
			if (text.IndexOf('<') < 0) return text;
			return "<noparse>" + text + "</noparse>";
		}

		/// <param name="command">Command.</param>
		/// <param name="args">Arguments. [0] is the name of the command, with [1] and beyond being the arguments</param>
		private void RunDispatcher(string command, string[] args, object user) {
			Command cmd = null;
			// try to find the given command. or the default command. if we can't find either...
			if (!commands.TryGetValue(command, out cmd) && !commands.TryGetValue("", out cmd)) {
				// error!
				string error = "Unknown command '" + NoparseFilterAroundTag(command) + "'";
				if (args.Length > 1) { error += " with arguments "; }
				for (int i = 1; i < args.Length; ++i) {
					if (i > 1) { error += ", "; }
					error += "'" + NoparseFilterAroundTag(args[i]) + "'";
				}
				this.cmd.Log(error);
			}
			// if we have a command
			if (cmd != null) {
				// execute it if it has valid code
				if (cmd.handler != null) {
					cmd.handler(args, user);
				} else {
					this.cmd.Log("Null command '" + command + "'");
				}
			}
		}

		public bool IsIdle()
		{
			return bash.IsProbablyIdle();
		}

		public bool ExecuteInstructions()
		{
			Instruction instruction = PopInstruction();
#if CONNECT_TO_REAL_COMMAND_LINE_TERMINAL
			if (bash == null) {
				string cmdExe = cmd.alternateCommandExecutable;
				if(cmdExe == "") { cmdExe = null; }
				bash = new SystemBridge(cmdExe);
			}
			if (bash.IsInitialized() && AllowSystemAccess && (instruction == null || instruction.IsUser(cmd.UserRawInput) || instruction.IsUser(bash)))
			{
				//if(instruction != null) Debug.Log("dispatching [" + instruction.text + "]");
				bash.Update(instruction, cmd); // always update, since this also pushes the pipeline
				return true;
			} else {
#endif
				// run any queued-up commands
				if (instruction != null) {
					//Debug.Log("dispatching " + instruction);
					Dispatch(instruction);
					cmd.NeedToRefreshUserPrompt = true;
					if (!cmd.callbacks.ignoreCallbacks && cmd.callbacks.whenCommandRuns != null) {
						cmd.callbacks.whenCommandRuns.Invoke();
						return true;
					}
				}
#if CONNECT_TO_REAL_COMMAND_LINE_TERMINAL
			}
#endif
			return false;
		}

		public void UpdateActivationCallbacks()
		{
			// if this is the active command line and it has not yet disabled user controls. done in update to stop many onStart and onStop calls from being invoked in series
			if (CmdLine_base.currentlyActiveCmdLine == cmd && CmdLine_base.disabledUserControls != cmd)
			{
				// if another command line disabled user controls
				if (CmdLine_base.disabledUserControls != null)
				{
					// tell it to re-enable controls
					if (!cmd.callbacks.ignoreCallbacks && cmd.callbacks.whenThisDeactivates != null) cmd.callbacks.whenThisDeactivates.Invoke();
				}
				CmdLine_base.disabledUserControls = cmd;
				if (!cmd.callbacks.ignoreCallbacks && cmd.callbacks.whenThisActivates != null) cmd.callbacks.whenThisActivates.Invoke();
			}
		}
	}
}