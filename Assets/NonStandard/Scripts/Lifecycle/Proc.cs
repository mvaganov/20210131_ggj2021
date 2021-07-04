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
		internal static edure ConvertR(edureSimple r, bool createIfNotFound = true) { return Get().Convert(r, createIfNotFound); }
		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static edure ConvertR(Action a, bool createIfNotFound = true) { return Get().Convert(a, createIfNotFound); }

		public static int Code(string identifier, bool createIfNotFound = false) { return Get().Code(identifier, createIfNotFound); }
		public static void NotifyIncident(string incidentId, Incident incident) { Get().NotifyIncident(incidentId, incident); }
		public static void NotifyIncident(string incidentId, object source = null, object detail = null) { Get().NotifyIncident(incidentId, source, detail); }
		public static void NotifyIncident(int incidentCode, Incident incident) { Get().NotifyIncident(incidentCode, incident); }
		public static void OnIncident(string incidentId, edure reaction, int count = -1) { Get().OnIncident(incidentId, reaction, count); }
		public static void OnIncident(int incidentCode, edure reaction, int count = -1) { Get().OnIncident(incidentCode, reaction, count); }
		public static int GetResponseIndex(int incidentCode, edure reaction) { return Get().GetResponseIndex(incidentCode, reaction); }
		public static void RemoveIncident(int incidentCode, edure reaction) { Get().RemoveIncident(incidentCode, reaction); }
		public static void RemoveIncident(int incidentCode, object reaction) { Get().RemoveIncident(incidentCode, reaction); }
		public static void Update() { Get().Update(); }
		public static void Delay(long delay, int incidentCode) { Get().Delay(delay, incidentCode); }
		public static void Delay(long delay, string incidentId) { Get().Delay(delay, incidentId); }
		public static void Delay(long delay, Action action) { Get().Delay(delay, action); }
		public static void Delay(long delay, edure response) { Get().Delay(delay, response); }
		public static void Delay(long delay, edureSimple response) { Get().Delay(delay, ConvertR(response)); }
		public static long GetTime() { return Get().GetTime(); }

		// functions so we don't need to include "return Procedure.Result.Success;" at the end of each lambda
		public static void OnIncident(string incidentId, edureSimple reaction, int count = -1) { Get().OnIncident(incidentId, ConvertR(reaction), count); }
		public static void OnIncident(int incidentCode, edureSimple reaction, int count = -1) { Get().OnIncident(incidentCode, ConvertR(reaction), count); }
		public static int GetResponseIndex(int incidentCode, edureSimple reaction) { return Get().GetResponseIndex(incidentCode, ConvertR(reaction, false)); }
		// functions so we can pass more straight forward Actions instead of more detailed Reactions
		public static void OnIncident(string incidentId, Action reaction, int count = -1) { Get().OnIncident(incidentId, ConvertR(reaction), count); }
		public static void OnIncident(int incidentCode, Action reaction, int count = -1) { Get().OnIncident(incidentCode, ConvertR(reaction), count); }
		public static int GetResponseIndex(int incidentCode, Action reaction) { return Get().GetResponseIndex(incidentCode, ConvertR(reaction, false)); }
		// singleton
		private static Processr _instance;
		public static Processr Get() {
			if (_instance != null) return _instance;
			return _instance = new Processr();
		}
	}
}