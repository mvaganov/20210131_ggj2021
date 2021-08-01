using System;
using System.Collections.Generic;

namespace NonStandard.Process {
	/// <summary>
	/// short for "Procedure" or "Process", this class is an interface to sequential process convenience functions
	/// </summary>
	public static partial class Proc {
		public enum Result {
			/// <summary>
			/// execution ended successfully
			/// </summary>
			Success,
			/// <summary>
			/// execution did not end successfully (probably cut short by failure)
			/// </summary>
			Failure,
			/// <summary>
			/// execution halted, do not continue current strategy.
			/// </summary>
			Halt
		}
		public delegate Result edure(Incident incident);
		public delegate void edureSimple(Incident incident);

		public struct Id {
			public static bool CreateFromStringIfNotFoundByDefault = true;
			public int Value;
			public Id(int value) { Value = value; }
			public static bool IsValidValue(int code) { return Main.IsValidCode(code); }
			public bool IsValid() { return IsValidValue(Value); }
			public static Id CreateIfNotFound(string id) { return new Id(Code(id, true)); }
			public static implicit operator Id(int code) { return new Id(code); }
			public static implicit operator Id(string id) {
				Id result = new Id(Code(id, CreateFromStringIfNotFoundByDefault));
				if (!CreateFromStringIfNotFoundByDefault && !result.IsValid()) { throw new Exception("Invalid "+typeof(Id)+" "+id); }
				return result;
			}
			public static implicit operator string(Id id) { return id.ToString(); }
			public static implicit operator int(Id id) { return id.Value; }
			public override string ToString() { return Main.codeToIncident[Value].Id; }
		}
		
		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static Proc.edure ConvertR(Proc.edureSimple r, bool cacheIfNotFound) { return Main.ConvertR(r, cacheIfNotFound); }
		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static Proc.edure ConvertR(Action a, bool cacheIfNotFound) { return Main.ConvertR(a, cacheIfNotFound); }

		public static int Code(string identifier, bool createIfNotFound) { return Main.Code(identifier, createIfNotFound); }
		public static void NotifyIncident(Proc.Id id, Incident incident) { Main.NotifyIncident(id, incident); }
		public static void NotifyIncident(Id id, object source = null, object detail = null) { Main.NotifyIncident(id, source, detail); }
		public static void WhileIncident(Proc.Id id, Proc.edure procedure, Proc.edure onFinish) { Main.WhileIncident(id, procedure, onFinish); }
		public static int GetResponseIndex(int incidentCode, Proc.edure procedure) { return Main.GetResponseIndex(incidentCode, procedure); }
		public static bool RemoveIncident(Proc.Id id, Proc.edure procedure) { return Main.RemoveIncident(id, procedure); }
		public static bool RemoveIncident(Proc.Id id, Action procedure) { return Main.RemoveIncident(id, procedure); }
		public static bool RemoveIncident(Proc.Id id, object procedure) { return Main.RemoveIncident(id, procedure); }
		public static Incident RemoveScheduled(Action action) => Main.RemoveScheduled(action);
		public static Incident RemoveScheduled(object procedure) => Main.RemoveScheduled(procedure);
		public static Incident Reschedule(object procedure, long when) => SystemClock.Reschedule(procedure, when);
		public static Incident Reschedule(Delegate procedure, long when) => SystemClock.Reschedule(procedure, when);
		public static Incident Reschedule(Incident incident, long when) => SystemClock.Reschedule(incident, when);
		public static ulong UpdateCounter { get; set; }
		public static void Update() { Main.Update(); ++UpdateCounter; }
		public static Incident Delay(long delay, int incidentCode) { return SystemClock.Delay(delay, incidentCode); }
		public static Incident Delay(long delay, string incidentId) { return SystemClock.Delay(delay, incidentId); }
		public static Incident Delay(long delay, Action action) { return SystemClock.Delay(delay, action); }
		public static Incident Delay(long delay, Proc.edure response) { return SystemClock.Delay(delay, response); }
		public static Incident Delay(long delay, Proc.edureSimple response) { return SystemClock.Delay(delay, ConvertR(response, true)); }
		public static Incident Enqueue(int incidentCode) { return SystemClock.Delay(0, incidentCode); }
		public static Incident Enqueue(string incidentId) { return SystemClock.Delay(0, incidentId); }
		public static Incident Enqueue(Proc.Id incidentId) { return SystemClock.Delay(0, incidentId.Value); }
		public static Incident Enqueue(Action action) { return SystemClock.Delay(0, action); }
		public static Incident Enqueue(Proc.edure response) { return SystemClock.Delay(0, response); }
		public static Incident Enqueue(Proc.edureSimple response) { return SystemClock.Delay(0, ConvertR(response, true)); }
		public static TimeKeeper SystemClock => Main.SystemClock;
		
		/// <summary>
		/// this value gives a numeric code to this moment as defined by milliseconds, unique to this runtime
		/// use <see cref="Now"/> for unsigned time (Now will never be before 0)
		/// </summary>
		public static long Time => SystemClock.GetTime();

		/// <summary>
		/// this value gives a numeric code to this moment as defined by milliseconds, unique to this runtime
		/// use <see cref="Time"/> for signed time (the concept of a moment before 0 is valid)
		/// </summary>
		public static ulong Now => (ulong)SystemClock.GetTime();

		// functions so we don't need to include "return Procedure.Result.Success;" at the end of each lambda
		public static void OnIncident(Proc.Id id, Proc.edure procedure, int count = -1, Proc.edure onLast = null) { Main.OnIncident(id, procedure, count, onLast); }
		public static void OnIncident(Proc.Id id, Proc.edureSimple procedure, int count = -1, Proc.edureSimple onLast = null) { Main.OnIncident(id, ConvertR(procedure, true), count, ConvertR(onLast,false)); }
		public static int GetResponseIndex(int incidentCode, Proc.edureSimple procedure) { return Main.GetResponseIndex(incidentCode, ConvertR(procedure, false)); }
		// functions so we can pass more straight forward Actions instead of more detailed Reactions
		public static void OnIncident(Proc.Id id, Action procedure, int count = -1, Action onLast = null) { Main.OnIncident(id, ConvertR(procedure, true), count, ConvertR(onLast, false)); }

		public static int GetResponseIndex(int incidentCode, Action procedure) { return Main.GetResponseIndex(incidentCode, ConvertR(procedure, false)); }
		// singleton
		private static Processr _instance;
		public static bool _isQuitting;
		public static bool IsQuitting { get { return _isQuitting; } internal set { _isQuitting = value; } }
		public static Processr Main {
			get => _instance != null ? _instance :  _instance = new Processr();
			set => _instance = value;
		}
		public static TimeKeeper GetTimer() { return Main.SystemClock; }

		public delegate Result ExecuteObjectFunction(object procedureLikeObjectToExecute, Incident incident);
		public static Dictionary<Type, ExecuteObjectFunction> InvokeMap = new Dictionary<Type, ExecuteObjectFunction>();
		public static void RegisterInvoke<T>(ExecuteObjectFunction invokeFunction) { InvokeMap[typeof(T)] = invokeFunction; }

		static Proc() {
			RegisterInvoke<Proc.edure>(InvokeProcedure);
			RegisterInvoke<Proc.edureSimple>(InvokeProcedureSimple);
			RegisterInvoke<Action>(InvokeAction);
			RegisterInvoke<int>(InvokeNotifyIncidentCode);
			RegisterInvoke<string>(InvokeNotifyIncidentId);
		}

		public static Result Invoke(object obj, Incident incident) {
			ExecuteObjectFunction func = InvokeMap[obj.GetType()];
			return func.Invoke(obj, incident);
		}

		public static Result InvokeProcedure(object obj, Incident incident) {
			return ((Proc.edure)obj).Invoke(incident);
		}
		public static Result InvokeProcedureSimple(object obj, Incident incident) {
			((Proc.edureSimple)obj).Invoke(incident);
			return Result.Success;
		}
		public static Result InvokeAction(object obj, Incident incident) {
			((Action)obj).Invoke();
			return Result.Success;
		}
		public static Result InvokeNotifyIncidentCode(object obj, Incident incident) {
			Proc.NotifyIncident((int)obj, incident);
			return Result.Success;
		}
		public static Result InvokeNotifyIncidentId(object obj, Incident incident) {
			Proc.NotifyIncident((string)obj, incident);
			return Result.Success;
		}
	}
}