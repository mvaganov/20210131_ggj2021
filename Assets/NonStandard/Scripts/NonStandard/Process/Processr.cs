using NonStandard.Extension;
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
			if (procedure == null) return null;
			if (!responseAilias.TryGetValue(procedure, out Proc.edure p)) {
				p = incident => { procedure.Invoke(incident); return Proc.Result.Success; };
				if (cacheIfNotFound) { responseAilias[procedure] = p; }
			}
			return p;
		}
		public Proc.edure ConvertR(Action action, bool cacheIfNotFound) {
			if (action == null) return null;
			if (!responseAilias.TryGetValue(action, out Proc.edure p)) {
				p = unusedIncident => { action.Invoke(); return Proc.Result.Success; };
				if (cacheIfNotFound) { responseAilias[action] = p; }
			}
			return p;
		}
		public Proc.edure ConvertR(Strategy strategy, bool cacheIfNotFound) {
			if (strategy == null) return null;
			if (!responseAilias.TryGetValue(strategy, out Proc.edure p)) {
				p = incident => { return strategy.InvokeChain(incident); };
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
		public void OnIncident(string incidentId, Proc.edure procedure, int count = -1, Proc.edure onLast=null) {
			OnIncident(Code(incidentId, true), procedure, count, onLast);
		}
		
		/// <param name="incidentCode">what incident to execute the given procedure on</param>
		/// <param name="procedure">what to do for each of the count iterations</param>
		/// <param name="count">how many times to execute the given procedure</param>
		/// <param name="onLast">special logic to do in addition to the standard procedure on the last iteration</param>
		/// <exception cref="Exception"></exception>
		public void OnIncident(int incidentCode, Proc.edure procedure, int count = -1, Proc.edure onLast = null) {
			if (count == 0) return;
			List<Proc.edure> responses = incidentResponseTable[incidentCode];
			if (count < 0) {
				responses.Add(procedure);
				return;
			}
			Proc.edure countLimitedProcedure = incident => {
				if (count <= 0) {
					throw new Exception("how was count decremented outside of this function?");
				}
				Proc.Result result = procedure.Invoke(incident);
				--count;
				if (count > 0) {
					return result;
				}
				if (onLast != null && result == Proc.Result.Success) {
					result = onLast.Invoke(incident); 
				}
				RemoveIncident(incidentCode, procedure);
				return result;
			};
			responseAilias[procedure] = countLimitedProcedure;
			responses.Add(countLimitedProcedure);
		}
		public int GetResponseIndex(int incidentCode, Proc.edure response) {
			if (incidentCode <= 0 || incidentCode >= incidentResponseTable.Count) { throw new Exception("bad incident code"); }
			return incidentResponseTable[incidentCode].IndexOf(response);
		}
		/// <summary>
		/// if a non-<see cref="Proc.edure"/> is used as a response to the incident, this should clear it
		/// </summary>
		public bool RemoveIncident(int incidentCode, object procedure) {
			Proc.edure r = procedure as Proc.edure;
			bool removed = false;
			if (r == null) {
				if (responseAilias.TryGetValue(procedure, out r)) {
					removed = responseAilias.Remove(procedure);
				} else {
					// Debug.LogWarning("the given response is not in the response table");
				}
			}
			removed |= RemoveIncident(incidentCode, r);
			return removed;
		}
		public bool RemoveIncident(int incidentCode, Proc.edure procedure) {
			bool removed = false;
			if (incidentCode > 0 && incidentCode < incidentResponseTable.Count
			&& !incidentResponseTable[incidentCode].Remove(procedure)) {
				if (responseAilias.TryGetValue(procedure, out Proc.edure alias)) {
					//RemoveIncident(incidentCode, alias);
					incidentResponseTable[incidentCode].Remove(alias);
					removed |= responseAilias.Remove(procedure);
				} else {
					// Debug.LogWarning("the given response is not in the response table");
				}
			}
			return removed;
		}

		public Incident RemoveScheduled(Action procedure) { return SystemClock.RemoveScheduled(procedure); }
		public Incident RemoveScheduled(object procedure) { return SystemClock.RemoveScheduled(procedure); }
	}
}