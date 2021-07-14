using System;
using System.Collections.Generic;

namespace NonStandard.Procedure {
	/// <summary>
	/// short for "Procedure" or "Process", this class is an interface to sequential process convenience functions
	/// </summary>
	public static partial class Proc {
		public enum Result { Success, Failure, Halt }
		public delegate Result edure(Incident incident);
		public delegate void edureSimple(Incident incident);

		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static Proc.edure ConvertR(Proc.edureSimple r, bool cacheIfNotFound) { return Main.ConvertR(r, cacheIfNotFound); }
		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static Proc.edure ConvertR(Action a, bool cacheIfNotFound) { return Main.ConvertR(a, cacheIfNotFound); }

		public static int Code(string identifier, bool createIfNotFound) { return Main.Code(identifier, createIfNotFound); }
		public static void NotifyIncident(string incidentId, Incident incident) { Main.NotifyIncident(incidentId, incident); }
		public static void NotifyIncident(string incidentId, object source = null, object detail = null) { Main.NotifyIncident(incidentId, source, detail); }
		public static void NotifyIncident(int incidentCode, object source = null, object detail = null) { Main.NotifyIncident(incidentCode, source, detail); }
		public static void NotifyIncident(int incidentCode, Incident incident) { Main.NotifyIncident(incidentCode, incident); }
		public static void OnIncident(string incidentId, Proc.edure procedure, int count = -1, Proc.edure onLast=null) { Main.OnIncident(incidentId, procedure, count, onLast); }
		public static void OnIncident(int incidentCode, Proc.edure procedure, int count = -1, Proc.edure onLast=null) { Main.OnIncident(incidentCode, procedure, count, onLast); }
		public static int GetResponseIndex(int incidentCode, Proc.edure procedure) { return Main.GetResponseIndex(incidentCode, procedure); }
		public static bool RemoveIncident(string incidentId, Proc.edure procedure) { return Main.RemoveIncident(Code(incidentId, false), procedure); }
		public static bool RemoveIncident(string incidentId, Action procedure) { return Main.RemoveIncident(Code(incidentId, false), procedure); }
		public static bool RemoveIncident(int incidentCode, Proc.edure procedure) { return Main.RemoveIncident(incidentCode, procedure); }
		public static bool RemoveIncident(int incidentCode, Action procedure) { return Main.RemoveIncident(incidentCode, procedure); }
		public static bool RemoveIncident(int incidentCode, object procedure) { return Main.RemoveIncident(incidentCode, procedure); }
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
		public static void OnIncident(string incidentId, Proc.edureSimple procedure, int count = -1, Proc.edureSimple onLast = null) { Main.OnIncident(incidentId, ConvertR(procedure, true), count, ConvertR(onLast,false)); }
		public static void OnIncident(int incidentCode, Proc.edureSimple procedure, int count = -1, Proc.edureSimple onLast = null) { Main.OnIncident(incidentCode, ConvertR(procedure, true), count, ConvertR(onLast, false)); }
		public static int GetResponseIndex(int incidentCode, Proc.edureSimple procedure) { return Main.GetResponseIndex(incidentCode, ConvertR(procedure, false)); }
		// functions so we can pass more straight forward Actions instead of more detailed Reactions
		public static void OnIncident(string incidentId, Action procedure, int count = -1, Action onLast = null) { Main.OnIncident(incidentId, ConvertR(procedure, true), count, ConvertR(onLast, false)); }
		public static void OnIncident(int incidentCode, Action procedure, int count = -1, Action onLast = null) { Main.OnIncident(incidentCode, ConvertR(procedure, true), count, ConvertR(onLast, false)); }
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