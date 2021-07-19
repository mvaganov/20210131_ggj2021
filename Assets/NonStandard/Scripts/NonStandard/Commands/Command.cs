using NonStandard.Data.Parse;

namespace NonStandard.Commands {
	public class Command {
		/// <summary>
		/// an execution context: what is being executed, with what arguments, from what source, and where is realtime output going
		/// </summary>
		public class Exec {
			/// <summary>
			/// what command is executing this handler
			/// </summary>
			public Command cmd;
			/// <summary>
			/// parsed tokens from a command string. use <see cref="Tokenizer.Tokenize"/>
			/// </summary>
			public Tokenizer tok;
			public object src;
			public Show.PrintFunc print;
			/// <param name="cmd">what command is executing this handler</param>
			/// <param name="tokenizer">parsed tokens from a command string. use <see cref="Tokenizer.Tokenize"/></param>
			/// <param name="source"></param>
			/// <param name="printFunction"></param>
			public Exec(Command cmd, Tokenizer tokenizer, object source, Show.PrintFunc printFunction) {
				this.cmd = cmd;
				this.tok = tokenizer;
				this.src = source;
				this.print = printFunction;
			}
		}

		public delegate void Handler(Exec executionContext);

		public readonly string Name;
		public Handler handler;
		public Argument[] arguments;
		public string help;
		public bool deprecated = false;
		public bool preview = false;
		public Command(string command, Handler handler, Argument[] arguments = null, string help = null, bool deprecated = false, bool preview = false) {
			this.Name = command;
			this.handler = handler;
			this.arguments = arguments;
			this.help = help;
			this.deprecated = deprecated;
			this.preview = preview;
		}
	}
}