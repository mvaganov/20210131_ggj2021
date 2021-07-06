using System;
using System.Collections.Generic;

namespace NonStandard.Procedure {
	/// <summary>
	/// no vowel at the end to reduce namespace collision
	/// </summary>
	[Serializable] public class Processr {
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
		/// maps the sequential codes of incidents to string identifier and other meta data
		/// </summary>
		public List<IncidentKind> codeToIncident = new List<IncidentKind>();
		/// <summary>
		/// when an Action of some kind is wrapped around another <see cref="Proc.edure"/>, this keeps track of the alias
		/// </summary>
		Dictionary<object, Proc.edure> responseAilias = new Dictionary<object, Proc.edure>();
		/// <summary>
		/// used for time-based processing "chronos"
		/// </summary>
		public TimeKeeper SystemClock = new TimeKeeper();

		[Serializable]
		public class IncidentKind {
			public string Id;
			public int Code;
			public int Count;
			public IncidentKind(string id, int code) { Id = id; Code = code; Count = 0; }
		}

		public Proc.edure ConvertR(Proc.edureSimple procedure, bool cacheIfNotFound) {
			if (!responseAilias.TryGetValue(procedure, out Proc.edure p)) {
				p = incident => { procedure.Invoke(incident); return Proc.Result.Success; };
				if (cacheIfNotFound) { responseAilias[procedure] = p; }
			}
			return p;
		}
		public Proc.edure ConvertR(Action action, bool cacheIfNotFound) {
			if (!responseAilias.TryGetValue(action, out Proc.edure p)) {
				p = unusedIncident => { action.Invoke(); return Proc.Result.Success; };
				if (cacheIfNotFound) { responseAilias[action] = p; }
			}
			return p;
		}
		public Proc.edure ConvertR(Strategy strategy, bool cacheIfNotFound) {
			if (!responseAilias.TryGetValue(strategy, out Proc.edure p)) {
				p = incident => { return strategy.Invoke(incident); };
				if (cacheIfNotFound) { responseAilias[strategy] = p; }
			}
			return p;
		}

		public int Code(string identifier, bool createIfNotFound = false) {
			if (!incidentIdTocode.TryGetValue(identifier, out int code)) {
				if (createIfNotFound) {
					if (incidentResponseTable.Count == 0) {
						incidentResponseTable.Add(null);
						codeToIncident.Add(new IncidentKind(null,0));
					}
					incidentIdTocode[identifier] = code = incidentResponseTable.Count;
					//Debug.Log("creating incident type '" + identifier + "': " + code);
					incidentResponseTable.Add(new List<Proc.edure>());
					codeToIncident.Add(new IncidentKind(identifier, code));
				} else {
					//Debug.Log("Could not find code for "+identifier+"\n"+NonStandard.Show.Stringify(codes));
				}
			}
			return code;
		}
		public void Update() { SystemClock.Update(); }
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
			++codeToIncident[incidentCode].Count;
			responses.ForEach(response => response.Invoke(incident));
		}
		public void OnIncident(string incidentId, Proc.edure procedure, int count = -1) {
			OnIncident(Code(incidentId, true), procedure, count);
		}
		public void OnIncident(int incidentCode, Proc.edure procedure, int count = -1) {
			List<Proc.edure> responses = incidentResponseTable[incidentCode];
			if (count < 0) {
				responses.Add(procedure);
			}
			if (count > 0) {
				Proc.edure countLimitedProcedure = incident => {
					if (count > 0) {
						procedure.Invoke(incident);
						if (--count <= 0) {
							RemoveIncident(incidentCode, procedure);
						}
					} else {
						RemoveIncident(incidentCode, procedure);
					}
					return Proc.Result.Success;
				};
				responseAilias[procedure] = countLimitedProcedure;
				responses.Add(countLimitedProcedure);
			}
		}
		public int GetResponseIndex(int incidentCode, Proc.edure response) {
			if (incidentCode <= 0 || incidentCode >= incidentResponseTable.Count) { throw new Exception("bad incident code"); }
			return incidentResponseTable[incidentCode].IndexOf(response);
		}
		/// <summary>
		/// if a non-<see cref="Reaction"/> is used as a response to the incident, this should clear it
		/// </summary>
		public void RemoveIncident(int incidentCode, object procedure) {
			Proc.edure r = procedure as Proc.edure;
			if (r == null) {
				if (responseAilias.TryGetValue(procedure, out r)) {
					responseAilias.Remove(procedure);
				} else {
					// Debug.LogWarning("the given response is not in the response table");
				}
			}
			RemoveIncident(incidentCode, r);
		}
		public void RemoveIncident(int incidentCode, Proc.edure procedure) {
			if (!incidentResponseTable[incidentCode].Remove(procedure)) {
				if (responseAilias.TryGetValue(procedure, out Proc.edure alias)) {
					//RemoveIncident(incidentCode, alias);
					incidentResponseTable[incidentCode].Remove(alias);
					responseAilias.Remove(procedure);
				} else {
					// Debug.LogWarning("the given response is not in the response table");
				}
			}
		}
	}
}