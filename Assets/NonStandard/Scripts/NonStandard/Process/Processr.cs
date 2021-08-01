using NonStandard.Extension;
using System;
using System.Collections.Generic;

namespace NonStandard.Process {
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

		/// <summary>
		/// convenience method: creates and <see cref="Incident"/> and passes it to any <see cref="Proc.edure"/>s waiting for it 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="source"></param>
		/// <param name="detail"></param>
		/// <param name="identifier"></param>
		public void NotifyIncident(Proc.Id id, object source = null, object detail = null, string identifier = null) {
			NotifyIncident(id, new Incident(SystemClock.GetTime(), identifier, source, detail));
		}
		public void NotifyIncident(Proc.Id id, Incident incident) {
			// make an array copy of the list because the list might be modified by the execution of elements in the list.
			Proc.edure[] responses = incidentResponseTable[id.Value].ToArray();
			++codeToIncident[id.Value].Count;
			responses.ForEach(response => response.Invoke(incident));
		}
		
		/// <param name="id">what incident to execute the given procedure on</param>
		/// <param name="procedure">what to do for each of the count iterations</param>
		/// <param name="count">how many times to execute the given procedure</param>
		/// <param name="onLast">special logic to do in addition to the standard procedure on the last iteration</param>
		/// <exception cref="Exception"></exception>
		public void OnIncident(Proc.Id id, Proc.edure procedure, int count = -1, Proc.edure onLast = null) {
			if (count == 0) return;
			List<Proc.edure> responses = incidentResponseTable[id.Value];
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
				RemoveIncident(id.Value, procedure);
				return result;
			};
			responseAilias[procedure] = countLimitedProcedure;
			responses.Add(countLimitedProcedure);
		}

		/// <summary>
		/// keep executing procedure until it returns Proc.Result.Success.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="procedure">incident listener is removed if result is <see cref="Proc.Result.Success"/>. should return <see cref="Proc.Result.Halt"/> until it should stop</param>
		/// <param name="onFinish"></param>
		public void WhileIncident(Proc.Id id, Proc.edure procedure, Proc.edure onFinish) {
			List<Proc.edure> responses = incidentResponseTable[id.Value];
			Proc.edure resultLimitedProcedure = incident => {
				Proc.Result result = procedure.Invoke(incident);
				if (result == Proc.Result.Success || result == Proc.Result.Failure) {
					RemoveIncident(id, procedure);
					onFinish.Invoke(incident);
					return Proc.Result.Success;
				}
				return result;
			};
			responseAilias[procedure] = resultLimitedProcedure;
			responses.Add(resultLimitedProcedure);
		}

		public bool IsValidCode(int code) { return code > 0 && code < incidentResponseTable.Count; }
		public int GetResponseIndex(Proc.Id id, Proc.edure response) {
			if (!IsValidCode(id.Value)) { throw new Exception("bad incident code"); }
			return incidentResponseTable[id.Value].IndexOf(response);
		}
		/// <summary>
		/// if a non-<see cref="Proc.edure"/> is used as a response to the incident, this should clear it
		/// </summary>
		public bool RemoveIncident(Proc.Id id, object procedure) {
			Proc.edure r = procedure as Proc.edure;
			bool removed = false;
			if (r == null) {
				if (responseAilias.TryGetValue(procedure, out r)) {
					removed = responseAilias.Remove(procedure);
				} else {
					// Debug.LogWarning("the given response is not in the response table");
				}
			}
			removed |= RemoveIncident(id, r);
			return removed;
		}
		public bool RemoveIncident(Proc.Id id, Proc.edure procedure) {
			bool removed = false;
			if (IsValidCode(id.Value) && !incidentResponseTable[id.Value].Remove(procedure)) {
				if (responseAilias.TryGetValue(procedure, out Proc.edure alias)) {
					//RemoveIncident(incidentCode, alias);
					incidentResponseTable[id.Value].Remove(alias);
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