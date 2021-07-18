using NonStandard.Data.Parse;
using System;

namespace NonStandard.Commands {
	public partial class MoreCommands {
		protected static void Cmd_Exit_Handler(Tokenizer tokenizer, object whosAsking, Show.PrintFunc print) { PlatformAdjust.Exit(); }

		[CommandMaker] protected static Command GenerateExitCommand() {
			return new Command("exit", Cmd_Exit_Handler, help:"ends this program");
		}
	}
}