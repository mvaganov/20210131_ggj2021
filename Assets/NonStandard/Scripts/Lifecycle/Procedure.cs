using System;
using System.Collections.Generic;
using UnityEngine;

public class Procedure {
	private static Instance _instance;
	public static Instance GetInstance() {
		if (_instance != null) return _instance;
		return _instance = new Instance();
	}
	public enum Result { Success, Failure, Halt }
	public delegate Result Reaction(Incident incident);
	public delegate void ReactionNoReturn(Incident incident);

	public static Reaction ToReaction(ReactionNoReturn reaction) {
		return incident => {
			reaction.Invoke(incident);
			return Result.Success;
		};
	}

	public const string ScheduledId = "crono";
	public delegate long GetTimeFunction();
	public static long GetFileSystemTimeNow() { return System.Environment.TickCount; }
	public static long UtcMilliseconds { get { return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond; } }

	public static int Code(string identifier, bool createIfNotFound = false) { return GetInstance().Code(identifier, createIfNotFound); }
	public static void NotifyIncident(string incidentId, Incident incident) { GetInstance().NotifyIncident(incidentId, incident); }
	public static void NotifyIncident(string incidentId, object source = null, object detail = null) { GetInstance().NotifyIncident(incidentId, source, detail); }
	public static void NotifyIncident(int incidentCode, Incident incident) { GetInstance().NotifyIncident(incidentCode, incident); }
	public static void OnIncident(string incidentId, Reaction reaction, int count = -1) { GetInstance().OnIncident(incidentId, reaction, count); }
	public static void OnIncident(int incidentCode, Reaction reaction, int count = -1) { GetInstance().OnIncident(incidentCode, reaction, count); }
	public static int GetResponseIndex(int incidentCode, Reaction reaction) { return GetInstance().GetResponseIndex(incidentCode, reaction); }
	public static void RemoveIncident(int incidentCode, Reaction reaction) { GetInstance().RemoveIncident(incidentCode, reaction); }
	public static void Update() { GetInstance().Update(); }
	public static void Delay(long delay, int incidentCode) { GetInstance().Delay(delay, incidentCode); }
	public static void Delay(long delay, string incidentId) {  GetInstance().Delay(delay, incidentId); }
	public static void Delay(long delay, Action action) {  GetInstance().Delay(delay, action); }
	public static void Delay(long delay, Reaction response) {  GetInstance().Delay(delay, response); }


	public static void OnIncident(string incidentId, ReactionNoReturn reaction, int count = -1) { GetInstance().OnIncident(incidentId, ToReaction(reaction), count); }
	public static void OnIncident(int incidentCode, ReactionNoReturn reaction, int count = -1) { GetInstance().OnIncident(incidentCode, ToReaction(reaction), count); }
	public static int GetResponseIndex(int incidentCode, ReactionNoReturn reaction) { return GetInstance().GetResponseIndex(incidentCode, ToReaction(reaction)); }
	public static void RemoveIncident(int incidentCode, ReactionNoReturn reaction) { GetInstance().RemoveIncident(incidentCode, ToReaction(reaction)); }
	public static void Delay(long delay, ReactionNoReturn response) { GetInstance().Delay(delay, ToReaction(response)); }
	public static long GetTime() { return GetInstance().HowToGetTime(); }
	public class Instance {
		public GetTimeFunction HowToGetTime = GetFileSystemTimeNow;
		public Instance() { }
		public Instance(GetTimeFunction howToGetTime) { HowToGetTime = howToGetTime; }

		List<List<Reaction>> responseTable = new List<List<Reaction>>();
		Dictionary<string, int> codes = new Dictionary<string, int>();
		Dictionary<Reaction, Reaction> responseAilias = new Dictionary<Reaction, Reaction>();
		List<Incident> schedule = new List<Incident>();

		public int Code(string identifier, bool createIfNotFound = false) {
			if (!codes.TryGetValue(identifier, out int code)) {
				if (createIfNotFound) {
					if (responseTable.Count == 0) {
						responseTable.Add(null);
					}
					codes[identifier] = code = responseTable.Count;
					Debug.Log("creating incident type '" + identifier + "': " + code);
					responseTable.Add(new List<Reaction>());
				} else {
					//Debug.Log("Could not find code for "+identifier+"\n"+NonStandard.Show.Stringify(codes));
				}
			}
			return code;
		}
		public void NotifyIncident(string incidentId, object source = null, object detail = null) {
			NotifyIncident(incidentId, new Incident(HowToGetTime(), incidentId, source, detail));
		}
		public void NotifyIncident(int incidentCode, object source = null, object detail = null) {
			NotifyIncident(incidentCode, new Incident(HowToGetTime(), null, source, detail));
		}
		public void NotifyIncident(string incidentId, Incident incident) {
			NotifyIncident(Code(incidentId, true), incident);
		}
		public void NotifyIncident(int incidentCode, Incident incident) {
			// make an array copy of the list because the list might be modified by the execution of elements in the list.
			Reaction[] responses = responseTable[incidentCode].ToArray();
			responses.ForEach(reaction => reaction.Invoke(incident));
		}
		public void OnIncident(string incidentId, Reaction reaction, int count = -1) {
			OnIncident(Code(incidentId, true), reaction, count);
		}
		public void OnIncident(int incidentCode, Reaction reaction, int count = -1) {
			List<Reaction> responses = responseTable[incidentCode];
			if (count < 0) {
				responses.Add(reaction);
			}
			if (count > 0) {
				Reaction countLimitedReaction = incident => {
					if (count > 0) {
						reaction.Invoke(incident);
						if (--count <= 0) {
							RemoveIncident(incidentCode, reaction);
						}
					} else {
						RemoveIncident(incidentCode, reaction);
					}
					return Result.Success;
				};
				responseAilias[reaction] = countLimitedReaction;
				responses.Add(countLimitedReaction);
			}
		}
		public int GetResponseIndex(int incidentCode, Reaction eaction) {
			return responseTable[incidentCode].IndexOf(eaction);
		}
		public void RemoveIncident(int incidentCode, Reaction reaction) {
			if (!responseTable[incidentCode].Remove(reaction)) {
				if (responseAilias.TryGetValue(reaction, out Reaction alias)) {
					responseTable[incidentCode].Remove(alias);
					responseAilias.Remove(reaction);
				} else {
					// Debug.LogWarning("the given response is not in the response table");
				}
			}
		}
		private List<Incident> _doneRightNow = new List<Incident>();
		public void Update() {
			long now = HowToGetTime();
			_doneRightNow.Clear();
			lock (schedule) {
				for (int i = 0; i < schedule.Count; ++i) {
					if (schedule[i].Timestamp > now) { break; }
					_doneRightNow.Add(schedule[i]);
				}
				schedule.RemoveRange(0, _doneRightNow.Count);
			}
			for (int i = 0; i < _doneRightNow.Count; ++i) {
				Incident incident = _doneRightNow[i];
				switch (incident.Source) {
				case int code: NotifyIncident(code, incident); break;
				case string id: NotifyIncident(id, incident); break;
				case Action act: act.Invoke(); break;
				case Reaction r: r.Invoke(incident); break;
				}
			}
		}
		public void Delay(long delay, int incidentCode) { AddToSchedule(new Incident(HowToGetTime() + delay, ScheduledId, incidentCode, null)); }
		public void Delay(long delay, string incidentId) { AddToSchedule(new Incident(HowToGetTime() + delay, ScheduledId, incidentId, null)); }
		public void Delay(long delay, Action action) { AddToSchedule(new Incident(HowToGetTime() + delay, ScheduledId, action, null)); }
		public void Delay(long delay, Reaction response) { AddToSchedule(new Incident(HowToGetTime() + delay, ScheduledId, response, null)); }
		public class IncidentComparer : IComparer<Incident> {
			public int Compare(Incident a, Incident b) {
				int comp = a.Timestamp.CompareTo(b.Timestamp);
				if (comp != 0) { return comp; }
				if (a.Identifier != null && b.Identifier != null) {
					comp = a.Identifier.CompareTo(b.Identifier);
				} else if (a.Identifier == null && b.Identifier != null) {
					comp = 1;
				} else if (a.Identifier != null && b.Identifier == null) {
					comp = -1;
				}
				return comp;
			}
		}
		public static readonly IncidentComparer Comparer = new IncidentComparer();
		public void AddToSchedule(Incident incident) {
			lock (schedule) {
				int index = schedule.BinarySearch(incident, Comparer);
				if (index < 0) {
					index = ~index;
				}
				schedule.Insert(index, incident);
			}
		}
	}
}
