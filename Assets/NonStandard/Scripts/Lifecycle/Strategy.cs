using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Procedure {
	/// <summary>
	/// a structure for connecting <see cref="Proc.edure"/>s into something more complex, with different kinds of linear advancement
	/// </summary>
	public class Strategy {
		public Proc.edure Procedure;
		public string Identifier;
		public Strategy Next;
		public Strategy Prev;
		public bool WaitForUpdate = false;
		public bool ExecuteEvenWithoutMerit = false;
		/// <summary>
		/// allows this strategy to have a value for desirability.
		/// if the merit is less than or equal to zero, it will not be executed
		/// if null, it's value is considered the minimum non-zero float.
		/// </summary>
		public MeritHeuristicFunctionType MeritHeuristic;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public delegate float MeritHeuristicFunctionType();
		public static float ZeroMeritHeuristic() => 0;
		public static float MinimumMeritHeuristic() => float.MinValue;
		public override string ToString() { return Identifier; }

		public Proc.Result Invoke(Incident incident) {
			Proc.Result result = Proc.Result.Success;
			bool allowedToRunOnMerit = ExecuteEvenWithoutMerit || (MeritHeuristic == null || MeritHeuristic.Invoke() > 0);
			//Debug.Log("Invoking " + Identifier+ " "+allowedToRunOnMerit+" "+Procedure);
			if (allowedToRunOnMerit && Procedure != null) {
				result = Procedure.Invoke(incident);
			}
			if (Next == null || result != Proc.Result.Success) return result;
			if (!Next.WaitForUpdate) {
				result = Next.Invoke(incident);
			} else {
				Proc.Delay(0, delayedIncident => Next.Invoke(incident));
			}
			return result;
		}

		public Strategy(string identifier, Strategy prev = null)
			{ Identifier = identifier; Prev = prev; }
		public Strategy(string identifier, Proc.edure action, Strategy prev)
			: this(identifier, prev) { Procedure = action; }
		public Strategy(string identifier, Action action, Strategy prev)
			: this(identifier, Proc.ConvertR(action,false), prev) {}
		public Strategy(string identifier, Proc.edureSimple action, Strategy prev)
			: this(identifier, Proc.ConvertR(action, false), prev) { }
		public Strategy(MeritHeuristicFunctionType merit, string identifier, Proc.edure action, Strategy prev = null)
			: this(identifier, action, prev) { MeritHeuristic= merit; }
		public Strategy(MeritHeuristicFunctionType merit, string identifier, Action action, Strategy prev = null)
			: this(merit, identifier, Proc.ConvertR(action,false), prev) {}
		public Strategy(MeritHeuristicFunctionType merit, string identifier, Proc.edureSimple action, Strategy prev = null)
			: this(merit, identifier, Proc.ConvertR(action, false), prev) { }
		public static Strategy If(MeritHeuristicFunctionType merit, string identifier, Proc.edureSimple action) {
			Strategy strat = new Strategy(merit, identifier, action, null);
			return strat;
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
				Strategy choice = Strategy.PickBest(possibleStrategies);
				//Debug.Log("Picked [" + choice.ListStrategies().JoinToString()+"]");
				if (choice == null) {
					//Debug.Log("Could not decide");
					return Proc.Result.Failure;
				}
				//Strategy deferredStrategy = new Strategy(identifier, d.Reaction, this);
				choice.Last().Next = deferringStrategy.Next;
				//Debug.Log("About to invoke [" + choice.ListStrategies().JoinToString() + "]");
				choice.Invoke(incident);
				return Proc.Result.Halt;
			});
			return deferringStrategy;
		}
		public static Strategy PickFirstGreatherThanZero(IList<Strategy> decisions) {
			for (int i = 0; i < decisions.Count; ++i) {
				float v = decisions[i].MeritHeuristic?.Invoke() ?? float.MinValue;
				if (v > 0) {
					return decisions[i];
				}
			}
			return null;
		}
		public static Strategy PickBest(IList<Strategy> decisions) {
			float bestValue = 0;
			int best = -1;
			for (int i = 0; i < decisions.Count; ++i) {
				float v = decisions[i].MeritHeuristic?.Invoke() ?? float.MinValue;
				if (best < 0 || v > bestValue) {
					best = i;
					bestValue = v;
				}
			}
			if (best >= 0) { return decisions[best]; }
			return null;
		}

		public Strategy Last() { Strategy s = this; while (s.Next != null) { s = s.Next; } return s; }
		public Strategy Root() { Strategy s = this; while (s.Prev != null) { s = s.Prev; } return s; }
		public List<Strategy> ListStrategies() {
			Strategy s = this;
			List<Strategy> strats = new List<Strategy>();
			while (s != null) {
				strats.Add(s);
				s = s.Next;
			}
			return strats;
		}
		public Strategy ThenImmediately(string identifier, Proc.edureSimple reaction) { return ThenImmediately(identifier, Proc.ConvertR(reaction, true)); }
		public Strategy ThenImmediately(string identifier, Action reaction) { return ThenImmediately(identifier, Proc.ConvertR(reaction, true)); }
		public Strategy AndThen(string identifier, Proc.edureSimple reaction) { return AndThen(identifier, Proc.ConvertR(reaction, true)); }
		public Strategy AndThen(string identifier, Action reaction) { return AndThen(identifier, Proc.ConvertR(reaction, true)); }
		public Strategy ThenOnIncident(string incidentId, Proc.edureSimple reaction) { return ThenOnIncident(incidentId, Proc.ConvertR(reaction, true)); }
		public Strategy ThenOnIncident(string incidentId, Action reaction) { return ThenOnIncident(incidentId, Proc.ConvertR(reaction, true)); }
		public Strategy ThenDelay(string identifier, int ms, Proc.edureSimple reaction) { return ThenDelay(identifier, ms, Proc.ConvertR(reaction, true)); }
		public Strategy ThenDelay(string identifier, int ms, Action reaction) { return ThenDelay(identifier, ms, Proc.ConvertR(reaction, true)); }
	}
}