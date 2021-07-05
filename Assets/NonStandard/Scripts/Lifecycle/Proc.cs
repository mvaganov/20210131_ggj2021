using System;

namespace NonStandard.Procedure {
	/// <summary>
	/// short for "Procedure" or "Process", this class is an interface to sequential process convenience functions
	/// </summary>
	public class Proc {
		public enum Result { Success, Failure, Halt }
		public delegate Result edure(Incident incident);
		public delegate void edureSimple(Incident incident);
		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static edure ConvertR(Proc.edureSimple r, bool cacheIfNotFound) { return Get().ConvertR(r, cacheIfNotFound); }
		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static edure ConvertR(Action a, bool cacheIfNotFound) { return Get().ConvertR(a, cacheIfNotFound); }
		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static edure ConvertR(Strategy s, bool cacheIfNotFound) { return Get().ConvertR(s, cacheIfNotFound); }

		public static int Code(string identifier, bool createIfNotFound) { return Get().Code(identifier, createIfNotFound); }
		public static void NotifyIncident(string incidentId, Incident incident) { Get().NotifyIncident(incidentId, incident); }
		public static void NotifyIncident(string incidentId, object source = null, object detail = null) { Get().NotifyIncident(incidentId, source, detail); }
		public static void NotifyIncident(int incidentCode, Incident incident) { Get().NotifyIncident(incidentCode, incident); }
		public static void OnIncident(string incidentId, Proc.edure reaction, int count = -1) { Get().OnIncident(incidentId, reaction, count); }
		public static void OnIncident(int incidentCode, Proc.edure reaction, int count = -1) { Get().OnIncident(incidentCode, reaction, count); }
		public static int GetResponseIndex(int incidentCode, Proc.edure reaction) { return Get().GetResponseIndex(incidentCode, reaction); }
		public static void RemoveIncident(int incidentCode, Proc.edure reaction) { Get().RemoveIncident(incidentCode, reaction); }
		public static void RemoveIncident(int incidentCode, object reaction) { Get().RemoveIncident(incidentCode, reaction); }
		public static void Update() { Get().Update(); }
		public static Incident Delay(long delay, int incidentCode) { return SystemClock.Delay(delay, incidentCode); }
		public static Incident Delay(long delay, string incidentId) { return SystemClock.Delay(delay, incidentId); }
		public static Incident Delay(long delay, Action action) { return SystemClock.Delay(delay, action); }
		public static Incident Delay(long delay, Proc.edure response) { return SystemClock.Delay(delay, response); }
		public static Incident Delay(long delay, Proc.edureSimple response) { return SystemClock.Delay(delay, ConvertR(response, true)); }
		public static Incident Delay(long delay, Strategy strategy) { return SystemClock.Delay(delay, strategy); }
		public static Incident Enqueue(int incidentCode) { return SystemClock.Delay(0, incidentCode); }
		public static Incident Enqueue(string incidentId) { return SystemClock.Delay(0, incidentId); }
		public static Incident Enqueue(Action action) { return SystemClock.Delay(0, action); }
		public static Incident Enqueue(Proc.edure response) { return SystemClock.Delay(0, response); }
		public static Incident Enqueue(Proc.edureSimple response) { return SystemClock.Delay(0, ConvertR(response, true)); }
		public static Incident Enqueue(Strategy strategy) { return SystemClock.Delay(0, strategy); }
		public static TimeKeeper SystemClock => Get().SystemClock;
		public static long Time => SystemClock.GetTime();

		// functions so we don't need to include "return Procedure.Result.Success;" at the end of each lambda
		public static void OnIncident(string incidentId, Proc.edureSimple reaction, int count = -1) { Get().OnIncident(incidentId, ConvertR(reaction, true), count); }
		public static void OnIncident(int incidentCode, Proc.edureSimple reaction, int count = -1) { Get().OnIncident(incidentCode, ConvertR(reaction, true), count); }
		public static int GetResponseIndex(int incidentCode, Proc.edureSimple reaction) { return Get().GetResponseIndex(incidentCode, ConvertR(reaction, false)); }
		// functions so we can pass more straight forward Actions instead of more detailed Reactions
		public static void OnIncident(string incidentId, Action reaction, int count = -1) { Get().OnIncident(incidentId, ConvertR(reaction,true), count); }
		public static void OnIncident(int incidentCode, Action reaction, int count = -1) { Get().OnIncident(incidentCode, ConvertR(reaction,true), count); }
		public static int GetResponseIndex(int incidentCode, Action reaction) { return Get().GetResponseIndex(incidentCode, ConvertR(reaction, false)); }
		// singleton
		private static Processr _instance;
		public static Processr Get() {
			if (_instance != null) return _instance;
			return _instance = new Processr();
		}
		public static TimeKeeper GetTimer() { return Get().SystemClock; }
	}
}