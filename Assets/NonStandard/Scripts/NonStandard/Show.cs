using System;

namespace NonStandard {
	public partial class Show {
		public delegate void PrintFunc(string text);
		public class Routes {
			public PrintFunc onLog;
			public PrintFunc onError;
			public PrintFunc onWarning;
			public PrintFunc onAnyMessage;
			public void Log(string str)     { onLog?.Invoke(str);     onAnyMessage?.Invoke(str); }
			public void Error(string str)   { onError?.Invoke(str);   onAnyMessage?.Invoke(str); }
			public void Warning(string str) { onWarning?.Invoke(str); onAnyMessage?.Invoke(str); }
			public void Assert(bool condition, string format, params object[] args) {
				if (condition) return;
				string message = format;
				if(args != null && args.Length != 0) { message = string.Format(format, args); }
				Error(message);
			}
			public Routes() { }
			public Routes(Routes o) {
				if (o.onLog != null)        { onLog =        (PrintFunc)o.onLog.Clone();        }
				if (o.onError != null)      { onError =      (PrintFunc)o.onError.Clone();      }
				if (o.onWarning != null)    { onWarning =    (PrintFunc)o.onWarning.Clone();    }
				if (o.onAnyMessage != null) { onAnyMessage = (PrintFunc)o.onAnyMessage.Clone(); }
			}
		}
		public static Routes _instance;
		public static Routes Route {
			get => _instance != null ? _instance : _instance = DefaultRoutes();
		}
		public static Routes DefaultRoutes() {
			Routes r = new Routes();
			r.onLog += DefaultLog;
			r.onError += DefaultError;
			r.onWarning += DefaultWarning;
			return r;
		}

		public static void Log(string s)     => Route.Log(s);
		public static void Error(string s)   => Route.Error(s);
		public static void Warning(string s) => Route.Warning(s);
		public static void Log(object o)     => Route.Log(o != null ? o.ToString() : "");
		public static void Error(object o)   => Route.Error(o != null ? o.ToString() : "");
		public static void Warning(object o) => Route.Warning(o != null ? o.ToString() : "");

		public static void Assert(bool condition, string format, params object[] args) =>
			Route.Assert(condition, format, args);
		public static void Assert(bool condition, string message) => Route.Assert(condition, message);
		public static void DefaultLog(string s) => Console.WriteLine(s);
		public static void DefaultWarning(string s) {
			ConsoleColor c = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(s);
			Console.ForegroundColor = c;
		}
		public static void DefaultError(string s) {
			ConsoleColor c = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(s);
			Console.ForegroundColor = c;
		}
	}
}
