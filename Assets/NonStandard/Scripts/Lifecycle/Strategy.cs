using System;
using System.Collections.Generic;

namespace NonStandard.Procedure {
	/// <summary>
	/// a structure for connecting <see cref="Proc.edure"/>s into something more complex, with different kinds of linear advancement
	/// </summary>
	public class Strategy {
		public Proc.edure Reaction;
		public string Identifier;
		public Strategy Next;
		public Strategy Prev;
		public bool WaitForUpdate = false;

		public override string ToString() { return Identifier; }

		public Proc.Result Invoke(Incident incident) {
			Proc.Result result = Proc.Result.Success;
			//Debug.Log("Invoking " + Identifier);
			if (Reaction != null) {
				result = Reaction.Invoke(incident);
			}
			if (Next == null || result != Proc.Result.Success) return result;
			if (!Next.WaitForUpdate) {
				result = Next.Invoke(incident);
			} else {
				Proc.Delay(0, delayedIncident => Next.Invoke(incident));
			}
			return result;
		}

		public Strategy(string identifier, Strategy prev = null) { Identifier = identifier; Prev = prev; }
		public Strategy(string identifier, Proc.edure action, Strategy prev) : this(identifier, prev) {
			Reaction = action;
		}
		public Strategy ThenImmediately(string identifier, Proc.edure reaction) {
			return Next = new Strategy(identifier, reaction, this);
		}
		public Strategy AndThen(string identifier, Proc.edure reaction) {
			Next = new Strategy(identifier, reaction, this);
			Next.WaitForUpdate = true;
			return Next;
		}
		public Strategy ThenOnIncident(string incidentId, Proc.edure reaction = null) {
			Strategy deferringStragegy = null;
			deferringStragegy = ThenImmediately("(defer)" + incidentId, incident => {
				Strategy deferredStrategy = new Strategy("(deferred)" + incidentId, reaction, this);
				deferredStrategy.Next = deferringStragegy.Next;
				Proc.OnIncident(incidentId, deferredStrategy.Invoke, 1);
				return Proc.Result.Halt;
			});
			return deferringStragegy;
		}
		public Strategy ThenDelay(string identifier, int ms, Proc.edure reaction = null) {
			Strategy deferringStrategy = null;
			deferringStrategy = ThenImmediately("(wait)" + identifier, incident => {
				Strategy deferredStrategy = new Strategy(identifier, reaction, this);
				deferredStrategy.Next = deferringStrategy.Next;
				Proc.Delay(ms, deferredStrategy.Invoke);
				return Proc.Result.Halt;
			});
			return deferringStrategy;
		}
		public List<Strategy> Convert(object[] possibleStrategies) {
			List<Strategy> strats = new List<Strategy>();
			for (int i = 0; i < possibleStrategies.Length; ++i) {
				if (possibleStrategies[i] is Strategy s) {
					s = s.Root();
					strats.Add(s);
				} else {
					//Debug.Log("non-strategy given in list");
				}
			}
			return strats;
		}
		public Strategy ThenDecideBestChoice(string identifier, params object[] possibleStrategies) {
			return ThenDecideBestChoice(identifier, Convert(possibleStrategies));
		}
		public Strategy ThenDecideBestChoice(string identifier, IList<Strategy> possibleStrategies) {
			Strategy deferringStrategy = null;
			deferringStrategy = ThenImmediately("(decide)" + identifier, incident => {
				Strategy choice = Contingency.PickBest(possibleStrategies);
				//Debug.Log("Picked [" + d.ListStrategies().JoinToString()+"]");
				if (choice == null) {
					//Debug.Log("Could not decide");
					return Proc.Result.Failure;
				}
				//Strategy deferredStrategy = new Strategy(identifier, d.Reaction, this);
				choice.Last().Next = deferringStrategy.Next;
				//Debug.Log("About to invoke [" + d.ListStrategies().JoinToString() + "]");
				choice.Invoke(incident);
				return Proc.Result.Halt;
			});
			return deferringStrategy;
		}
		public Strategy Last() { Strategy s = this; while (s.Next != null) { s = s.Next; } return s; }
		public Strategy Root() { Strategy s = this; while (s.Prev != null) { s = s.Prev; } return s; }
		public Contingency RootDecision() { Strategy s = this; while (s.Prev != null) { s = s.Prev; } return s as Contingency; }
		public List<Strategy> ListStrategies() {
			Strategy s = this;
			List<Strategy> strats = new List<Strategy>();
			while (s != null) {
				strats.Add(s);
				s = s.Next;
			}
			return strats;
		}
		public Strategy(string identifier, Proc.edureSimple action, Strategy prev) : this(identifier, Proc.ConvertR(action), prev) { }
		public Strategy(string identifier, Action action, Strategy prev) : this(identifier, Proc.ConvertR(action), prev) { }
		public Strategy ThenImmediately(string identifier, Proc.edureSimple reaction) { return ThenImmediately(identifier, Proc.ConvertR(reaction)); }
		public Strategy ThenImmediately(string identifier, Action reaction) { return ThenImmediately(identifier, Proc.ConvertR(reaction)); }
		public Strategy AndThen(string identifier, Proc.edureSimple reaction) { return AndThen(identifier, Proc.ConvertR(reaction)); }
		public Strategy AndThen(string identifier, Action reaction) { return AndThen(identifier, Proc.ConvertR(reaction)); }
		public Strategy ThenOnIncident(string incidentId, Proc.edureSimple reaction) { return ThenOnIncident(incidentId, Proc.ConvertR(reaction)); }
		public Strategy ThenOnIncident(string incidentId, Action reaction) { return ThenOnIncident(incidentId, Proc.ConvertR(reaction)); }
		public Strategy ThenDelay(string identifier, int ms, Proc.edureSimple reaction) { return ThenDelay(identifier, ms, Proc.ConvertR(reaction)); }
		public Strategy ThenDelay(string identifier, int ms, Action reaction) { return ThenDelay(identifier, ms, Proc.ConvertR(reaction)); }
	}
}