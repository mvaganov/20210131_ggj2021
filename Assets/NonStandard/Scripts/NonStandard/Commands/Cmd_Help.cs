using NonStandard.Cli;
using System;
using System.Collections.Generic;

namespace NonStandard.Commands {
	public partial class Commander {
		public static void Cmd_Help_Handler(Command.Exec e, Dictionary<string,Command> commandTable) {
			byte command = (byte)ConsoleColor.Magenta;
			byte argument = (byte)ConsoleColor.Yellow;
			byte preview = (byte)ConsoleColor.DarkMagenta;
			byte deprecated = (byte)ConsoleColor.DarkYellow;
			byte description = (byte)ConsoleColor.DarkGray;
			List<Command> commands = new List<Command>();
			foreach (KeyValuePair<string, Command> kvp in commandTable) {
				commands.Add(kvp.Value);
			}
			commands.Sort((a, b) => a.Name.CompareTo(b.Name));
			bool useColor = true;
			string colorStd = (useColor ? Col.r() : "");
			string colorCommand = (useColor ? Col.r(command) : "");
			string colorPreview = (useColor ? Col.r(preview) : "");
			string colorArgument = (useColor ? Col.r(argument) : "");
			string colorDeprecated = (useColor ? Col.r(deprecated) : "");
			string colorDescription = (useColor ? Col.r(description) : "");
			for (int i = 0; i < commands.Count; ++i) {
				Command cmd = commands[i];
				string colorTxt = cmd.deprecated ? colorDeprecated : cmd.preview ? colorPreview : colorCommand;
				string line = $"{colorTxt}{cmd.Name}{colorStd} : {colorDescription}{cmd.help}{colorStd}\n";
				e.print.Invoke(line);
			}
			string extraInstructions = $"for more information, type {colorCommand}help{colorArgument} nameOfCommand{colorStd}\n";
			e.print.Invoke(extraInstructions);
		}
		protected void Cmd_Help_Handler(Command.Exec e) {
			Cmd_Help_Handler(e, commandLookup);
		}
		[CommandMaker] protected Command GenerateHelpCommand() {
			return new Command("help", Cmd_Help_Handler, new Argument[] {
				new Argument("-c", "command", "which specific command to get help info for", type:typeof(string), order:1),
			}, "prints this help text");
		}
	}
}
