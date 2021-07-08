namespace NonStandard.Commands {
	public class Command {
		/// <summary>command-line handler. think "standard main" from Java or C/C++.
		/// args[0] is the command, args[1] and beyond are the arguments.</summary>
		public delegate void Handler(string[] args, object whosAsking);

		public readonly string command;
		public Handler handler;
		public Argument[] arguments;
		public string help;
		public bool deprecated = false;
		public bool preview = false;
		public Command(string command, Handler handler, Argument[] arguments, string help) {
			this.command = command; this.handler = handler; this.arguments = arguments; this.help = help;
		}
	}
}