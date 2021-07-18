using NonStandard.Data.Parse;

namespace NonStandard.Commands {
	public class Command {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens">parsed tokens from a command string. use <see cref="Tokenizer.Tokenize"/></param>
		/// <param name="whosAsking"></param>
		/// <param name="printFunction"></param>
		public delegate void Handler(Tokenizer tokens, object whosAsking, Show.PrintFunc printFunction);

		public readonly string Name;
		public Handler handler;
		public Argument[] arguments;
		public string help;
		public bool deprecated = false;
		public bool preview = false;
		public Command(string command, Handler handler, Argument[] arguments = null, string help = null) {
			this.Name = command; this.handler = handler; this.arguments = arguments; this.help = help;
		}
	}
}