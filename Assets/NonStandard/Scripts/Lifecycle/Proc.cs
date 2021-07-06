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
		internal static Proc.edure ConvertR(Proc.edureSimple r, bool cacheIfNotFound) { return Get().ConvertR(r, cacheIfNotFound); }
		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static Proc.edure ConvertR(Action a, bool cacheIfNotFound) { return Get().ConvertR(a, cacheIfNotFound); }

		public static int Code(string identifier, bool createIfNotFound) { return Get().Code(identifier, createIfNotFound); }
		public static void NotifyIncident(string incidentId, Incident incident) { Get().NotifyIncident(incidentId, incident); }
		public static void NotifyIncident(string incidentId, object source = null, object detail = null) { Get().NotifyIncident(incidentId, source, detail); }
		public static void NotifyIncident(int incidentCode, Incident incident) { Get().NotifyIncident(incidentCode, incident); }
		public static void OnIncident(string incidentId, Proc.edure procedure, int count = -1) { Get().OnIncident(incidentId, procedure, count); }
		public static void OnIncident(int incidentCode, Proc.edure procedure, int count = -1) { Get().OnIncident(incidentCode, procedure, count); }
		public static int GetResponseIndex(int incidentCode, Proc.edure procedure) { return Get().GetResponseIndex(incidentCode, procedure); }
		public static void RemoveIncident(int incidentCode, Proc.edure procedure) { Get().RemoveIncident(incidentCode, procedure); }
		public static void RemoveIncident(int incidentCode, object procedure) { Get().RemoveIncident(incidentCode, procedure); }
		public static void Update() { Get().Update(); }
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
		public static TimeKeeper SystemClock => Get().SystemClock;
		public static long Time => SystemClock.GetTime();

		// functions so we don't need to include "return Procedure.Result.Success;" at the end of each lambda
		public static void OnIncident(string incidentId, Proc.edureSimple procedure, int count = -1) { Get().OnIncident(incidentId, ConvertR(procedure, true), count); }
		public static void OnIncident(int incidentCode, Proc.edureSimple procedure, int count = -1) { Get().OnIncident(incidentCode, ConvertR(procedure, true), count); }
		public static int GetResponseIndex(int incidentCode, Proc.edureSimple procedure) { return Get().GetResponseIndex(incidentCode, ConvertR(procedure, false)); }
		// functions so we can pass more straight forward Actions instead of more detailed Reactions
		public static void OnIncident(string incidentId, Action procedure, int count = -1) { Get().OnIncident(incidentId, ConvertR(procedure, true), count); }
		public static void OnIncident(int incidentCode, Action procedure, int count = -1) { Get().OnIncident(incidentCode, ConvertR(procedure, true), count); }
		public static int GetResponseIndex(int incidentCode, Action procedure) { return Get().GetResponseIndex(incidentCode, ConvertR(procedure, false)); }
		// singleton
		private static Processr _instance;
		public static Processr Get() {
			if (_instance != null) return _instance;
			return _instance = new Processr();
		}
		public static TimeKeeper GetTimer() { return Get().SystemClock; }


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