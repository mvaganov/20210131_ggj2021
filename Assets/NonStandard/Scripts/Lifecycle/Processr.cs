using System;
using System.Collections.Generic;

namespace NonStandard.Procedure {
	public class Processr {
		public Processr() { }

		/// <summary>
		/// each index maps to an Incident's "Code", which can be determined by <see cref="incidentIdTocode"/>. used for event-based processing "kairos"
		/// </summary>
		List<List<Proc.edure>> incidentResponseTable = new List<List<Proc.edure>>();
		/// <summary>
		/// maps the names of incidents to their sequential code
		/// </summary>
		Dictionary<string, int> incidentIdTocode = new Dictionary<string, int>();
		/// <summary>
		/// maps the sequential codes of incidents to string identifier
		/// </summary>
		List<string> codeToIncidentId = new List<string>();
		/// <summary>
		/// when an Action of some kind is wrapped around another Reaction, this keeps track of the alias
		/// </summary>
		Dictionary<object, Proc.edure> responseAilias = new Dictionary<object, Proc.edure>();
		/// <summary>
		/// used for time-based processing "chronos"
		/// </summary>
		TimeKeeper systemClock = new TimeKeeper();

		public TimeKeeper SystemClock => systemClock;

		public Proc.edure ConvertR(Proc.edureSimple reaction, bool cacheIfNotFound) {
			if (!responseAilias.TryGetValue(reaction, out Proc.edure r)) {
				r = incident => { reaction.Invoke(incident); return Proc.Result.Success; };
				if (cacheIfNotFound) { responseAilias[reaction] = r; }
			}
			return r;
		}
		public Proc.edure ConvertR(Action action, bool cacheIfNotFound) {
			if (!responseAilias.TryGetValue(action, out Proc.edure r)) {
				r = unusedIncident => { action.Invoke(); return Proc.Result.Success; };
				if (cacheIfNotFound) { responseAilias[action] = r; }
			}
			return r;
		}
		public Proc.edure ConvertR(Strategy strategy, bool cacheIfNotFound) {
			if (!responseAilias.TryGetValue(strategy, out Proc.edure r)) {
				r = incident => { return strategy.Invoke(incident); };
				if (cacheIfNotFound) { responseAilias[strategy] = r; }
			}
			return r;
		}

		public int Code(string identifier, bool createIfNotFound = false) {
			if (!incidentIdTocode.TryGetValue(identifier, out int code)) {
				if (createIfNotFound) {
					if (incidentResponseTable.Count == 0) {
						incidentResponseTable.Add(null);
						codeToIncidentId.Add(null);
					}
					incidentIdTocode[identifier] = code = incidentResponseTable.Count;
					//Debug.Log("creating incident type '" + identifier + "': " + code);
					incidentResponseTable.Add(new List<Proc.edure>());
					codeToIncidentId.Add(identifier);
				} else {
					//Debug.Log("Could not find code for "+identifier+"\n"+NonStandard.Show.Stringify(codes));
				}
			}
			return code;
		}
		public void Update() { systemClock.Update(); }
		public void NotifyIncident(string incidentId, object source = null, object detail = null) {
			NotifyIncident(incidentId, new Incident(SystemClock.GetTime(), incidentId, source, detail));
		}
		public void NotifyIncident(int incidentCode, object source = null, object detail = null, string identifier = null) {
			NotifyIncident(incidentCode, new Incident(SystemClock.GetTime(), identifier, source, detail));
		}
		public void NotifyIncident(string incidentId, Incident incident) {
			NotifyIncident(Code(incidentId, true), incident);
		}
		public void NotifyIncident(int incidentCode, Incident incident) {
			// make an array copy of the list because the list might be modified by the execution of elements in the list.
			Proc.edure[] responses = incidentResponseTable[incidentCode].ToArray();
			responses.ForEach(reaction => reaction.Invoke(incident));
		}
		public void OnIncident(string incidentId, Proc.edure reaction, int count = -1) {
			OnIncident(Code(incidentId, true), reaction, count);
		}
		public void OnIncident(int incidentCode, Proc.edure reaction, int count = -1) {
			List<Proc.edure> responses = incidentResponseTable[incidentCode];
			if (count < 0) {
				responses.Add(reaction);
			}
			if (count > 0) {
				Proc.edure countLimitedReaction = incident => {
					if (count > 0) {
						reaction.Invoke(incident);
						if (--count <= 0) {
							RemoveIncident(incidentCode, reaction);
						}
					} else {
						RemoveIncident(incidentCode, reaction);
					}
					return Proc.Result.Success;
				};
				responseAilias[reaction] = countLimitedReaction;
				responses.Add(countLimitedReaction);
			}
		}
		public int GetResponseIndex(int incidentCode, Proc.edure reaction) {
			if (incidentCode <= 0 || incidentCode >= incidentResponseTable.Count) { throw new Exception("bad incident code"); }
			return incidentResponseTable[incidentCode].IndexOf(reaction);
		}
		/// <summary>
		/// if a non-<see cref="Reaction"/> is used as a response to the incident, this should clear it
		/// </summary>
		public void RemoveIncident(int incidentCode, object reaction) {
			Proc.edure r = reaction as Proc.edure;
			if (r == null) {
				if (responseAilias.TryGetValue(reaction, out r)) {
					responseAilias.Remove(reaction);
				} else {
					// Debug.LogWarning("the given response is not in the response table");
				}
			}
			RemoveIncident(incidentCode, r);
		}
		public void RemoveIncident(int incidentCode, Proc.edure reaction) {
			if (!incidentResponseTable[incidentCode].Remove(reaction)) {
				if (responseAilias.TryGetValue(reaction, out Proc.edure alias)) {
					incidentResponseTable[incidentCode].Remove(alias);
					responseAilias.Remove(reaction);
				} else {
					// Debug.LogWarning("the given response is not in the response table");
				}
			}
		}
		//public Incident Delay(long delay, int incidentCode) { return systemClock.Delay(delay, incidentCode); }
		//public Incident Delay(long delay, string incidentId) { return systemClock.Delay(delay, incidentId); }
		//public Incident Delay(long delay, Action action) { return systemClock.Delay(delay, action); }
		//public Incident Delay(long delay, Proc.edure response) { return systemClock.Delay(delay, response); }
	}
}