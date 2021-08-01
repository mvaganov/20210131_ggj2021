using System;
using System.Collections.Generic;

namespace NonStandard.Process {
	/// <summary>
	/// a structure for connecting <see cref="Proc.edure"/>s into something more complex, with different kinds of linear advancement
	/// </summary>
	public class Strategy {
		public Proc.edure Procedure;
		public string Identifier;
		public Strategy Prev;
		public Strategy Next;
		public bool WaitForUpdate = false;
		public bool OnlyExecuteOnPositiveMerit = false;
		public bool IsExceptionHandler = false;
		public bool IsFinallyHandler = false;
		public bool IsDeferred = false;
		/// <summary>
		/// allows this strategy to have a value for desirability.
		/// if the merit is less than or equal to zero, it will not be executed
		/// if null, it's value is considered the minimum non-zero float.
		/// </summary>
		public MeritHeuristicFunctionType MeritHeuristic;

		public delegate float MeritHeuristicFunctionType();
		public static float ZeroMeritHeuristic() => 0;
		public static float MinimumMeritHeuristic() => float.Epsilon;
		public override string ToString() { return Identifier; }

		public const string ExceptionId = "Exception";
		public const string IncidentCodeId = "IncidentCode";

		static Strategy() {
			Proc.RegisterInvoke<Strategy>(InvokeStrategy);
		}
		public static Proc.Result InvokeStrategy(object obj, Incident incident) {
			return ((Strategy)obj).InvokeChain(incident);
		}

		/// <summary>
		/// execute the procedure of this strategy, then continue to the next logical strategy
		/// </summary>
		public Proc.Result InvokeChain(Incident incident) {
			Proc.Result result = Invoke(incident);
			if (Next == null || result != Proc.Result.Success) return result;
			return ContinueChain(incident);
		}
		/// <summary>
		/// continue to the next logical strategy (without executing the procedure of this strategy)
		/// </summary>
		public Proc.Result ContinueChain(Incident incident) {
			if (!Next.WaitForUpdate) {
				return Next.InvokeChain(incident);
			}
			Proc.Delay(0, delayedIncident => Next.InvokeChain(incident));
			return Proc.Result.Success;
		}
		/// <summary>
		/// execute the procedure of this strategy (without continuing to the next logical strategy)
		/// </summary>
		public Proc.Result Invoke(Incident incident) {
			Proc.Result result = Proc.Result.Success;
			bool allowedToRun = !IsExceptionHandler &&
			            (!OnlyExecuteOnPositiveMerit || (MeritHeuristic == null || MeritHeuristic.Invoke() > 0));
			//Debug.Log("Invoking " + Identifier+ " "+allowedToRunOnMerit+" "+Procedure);
			if (allowedToRun && Procedure != null) {
				result = Invoke_Internal(incident);
			}
			return result;
		}

		internal Proc.Result Invoke_Internal(Incident incident) {
			Proc.Result result = Proc.Result.Success;
			if (Procedure == null) return result;
			try {
				result = Procedure.Invoke(incident);
			} catch (Exception e) {
				Show.Error(e);
				HandleException(e, incident);
				return Proc.Result.Halt;
			}
			return result;
		}

		public void HandleException(Exception e, Incident incident) {
			string sourceInfo = Identifier;
			if (incident != null) { sourceInfo += ": " + incident.Identifier; }
			Incident exceptionIncident = new Incident(e.GetType().Name, sourceInfo, e.ToString());
			Strategy finallyHandler = NextFinallyHandler();
			Strategy exceptionHandler = NextExceptionHandler(e.GetType());
			if (finallyHandler != null && finallyHandler.HasFutureStrategy(exceptionHandler)) {
				finallyHandler.Invoke_Internal(exceptionIncident);
			}
			exceptionHandler?.Invoke_Internal(exceptionIncident);
			Strategy whereToLookForFinallyHandlerFrom = exceptionHandler != null ? exceptionHandler : this;
			finallyHandler = whereToLookForFinallyHandlerFrom.NextFinallyHandler();
			finallyHandler?.Invoke_Internal(exceptionIncident);
		}

		public bool HasFutureStrategy(Strategy s) {
			Strategy cursor = Next;
			while (cursor != null) {
				if (cursor == s) return true;
				cursor = cursor.Next;
			}
			return false;
		}

		// public void InvokeError(Incident incident, Exception error) {
		// 	//Show.Warning(ListPromises().JoinToString(", "));
		// 	Error e = null;
		// 	Type errorType = error.GetType();
		// 	e = NextExceptionHandler(errorType);
		// 	if (e != null) {
		// 		//Show.Log("error handler for "+e.exceptionType);
		// 		e.Invoke_Internal(new Incident("error", incident, error));
		// 		Strategy finalPromise = e.NextFinallyHandler();
		// 		finalPromise?.Invoke_Internal(incident);
		// 	} else {
		// 		Show.Error("broken promise: " + this + " " + incident + " " + error);
		// 	}
		// }
		public Strategy(string identifier, Strategy prev = null)
			{ Identifier = identifier; Prev = prev; }
		public Strategy(string identifier, Proc.edure action, Strategy prev = null)
			: this(identifier, prev) { Procedure = action; }
		public Strategy(string identifier, Action action, Strategy prev = null)
			: this(identifier, Proc.ConvertR(action,false), prev) {}
		public Strategy(string identifier, Proc.edureSimple action, Strategy prev = null)
			: this(identifier, Proc.ConvertR(action, false), prev) { }
		public Strategy(MeritHeuristicFunctionType merit, string identifier, Proc.edure action, Strategy prev = null)
			: this(identifier, action, prev) { MeritHeuristic= merit; }
		public Strategy(MeritHeuristicFunctionType merit, string identifier, Action action, Strategy prev = null)
			: this(merit, identifier, Proc.ConvertR(action,false), prev) {}
		public Strategy(MeritHeuristicFunctionType merit, string identifier, Proc.edureSimple action, Strategy prev = null)
			: this(merit, identifier, Proc.ConvertR(action, false), prev) { }
		public Strategy ThenImmediately(string identifier, Proc.edure procedure) {
			return Next = new Strategy(identifier, procedure, this);
		}
		public Strategy AndThen(string identifier, Proc.edure procedure) {
			Next = new Strategy(identifier, procedure, this);
			Next.WaitForUpdate = true;
			return Next;
		}
		public Strategy ThenOnIncident(Proc.Id id, Proc.edure procedure = null, int count = 1) {
			Strategy deferringStrategy = null;
			deferringStrategy = ThenImmediately("(defer)" + id, incident => {
				Strategy deferredStrategy = new Strategy("(deferred)" + id, procedure, this);
				deferredStrategy.Next = deferringStrategy.Next;
				deferredStrategy.IsDeferred = true;
				Proc.OnIncident(id, deferredStrategy.Invoke, count, deferredStrategy.ContinueChain);
				return Proc.Result.Halt;
			});
			return deferringStrategy;
		}
		/// <summary>
		/// keep executing procedure until it returns Proc.Result.Success.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="procedure">incident listener is removed if result is <see cref="Proc.Result.Success"/>. should return <see cref="Proc.Result.Halt"/> until it should stop</param>
		public Strategy ThenWhileIncident(Proc.Id id, Proc.edure procedure = null) {
			Strategy deferringStrategy = null;
			deferringStrategy = ThenImmediately("(defer)" + id, incident => {
				Strategy deferredStrategy = new Strategy("(deferred)" + id, procedure, this);
				deferredStrategy.Next = deferringStrategy.Next;
				deferredStrategy.IsDeferred = true;
				Proc.WhileIncident(id, deferredStrategy.Invoke, deferringStrategy.ContinueChain);
				return Proc.Result.Halt;
			});
			return deferringStrategy;
		}
		public Strategy ThenDelay(string identifier, int ms, Proc.edure procedure = null) {
			Strategy deferringStrategy = null;
			deferringStrategy = ThenImmediately("(wait)" + identifier, incident => {
				Strategy deferredStrategy = new Strategy(identifier, procedure, this);
				deferredStrategy.Next = deferringStrategy.Next;
				deferredStrategy.IsDeferred = true;
				Proc.Delay(ms, deferredStrategy.InvokeChain);
				return Proc.Result.Halt;
			});
			return deferringStrategy;
		}
		public List<Strategy> ConvertToStrategyList(object[] possibleStrategies) {
			List<Strategy> strats = new List<Strategy>();
			for (int i = 0; i < possibleStrategies.Length; ++i) {
				Strategy nextStrat = ConvertToStrategy(possibleStrategies[i]);
				if(nextStrat != null) strats.Add(nextStrat);
			}
			return strats;
		}
		public static Strategy ConvertToStrategy(object o) {
			switch (o) {
			case Strategy s: return s.Root();
			case Proc.edure p: return new Strategy(p.Method.Name, p);
			case Proc.edureSimple ps: return new Strategy(ps.Method.Name, ps);
			case Action a: return new Strategy(a.Method.Name, a);
			case int i: return new Strategy(IncidentCodeId, incident => { Proc.NotifyIncident(i, incident); return Proc.Result.Success; });
			case string s: return new Strategy(IncidentCodeId, incident => { Proc.NotifyIncident(s, incident); return Proc.Result.Success; });
			}
			//Debug.Log("non-strategy given in list");
			return null;
		}
		public Strategy ThenDecideBestChoice(string identifier, params object[] possibleStrategies) {
			return ThenDecideBestChoice(identifier, ConvertToStrategyList(possibleStrategies));
		}
		public Strategy ThenDecideBestChoice(string identifier, IList<Strategy> possibleStrategies) {
			Strategy deferringStrategy = null;
			deferringStrategy = ThenImmediately("(auto decide)" + identifier, incident => {
				Strategy choice = PickBest(possibleStrategies);
				//Debug.Log("Picked [" + choice.ListStrategies().JoinToString()+"]");
				if (choice == null) {
					//Debug.Log("Could not decide");
					return Proc.Result.Failure;
				}
				//Strategy deferredStrategy = new Strategy(identifier, d.Procedure, this);
				choice.Last().Next = deferringStrategy.Next;
				//Debug.Log("About to invoke [" + choice.ListStrategies().JoinToString() + "]");
				choice.InvokeChain(incident);
				return Proc.Result.Halt;
			});
			return deferringStrategy;
		}

		public Strategy ThenDoFirstIncident(string identifier, (Proc.Id, Proc.edure) option0) {
			return ThenDoFirstIncident(identifier, new []{option0});
		}
		public Strategy ThenDoFirstIncident(string identifier, (Proc.Id, Proc.edure) option0, (Proc.Id, Proc.edure) option1) {
			return ThenDoFirstIncident(identifier, new []{option0,option1});
		}
		public Strategy ThenDoFirstIncident(string identifier, (Proc.Id, Proc.edure) option0, (Proc.Id, Proc.edure) option1, (Proc.Id, Proc.edure) option2) {
			return ThenDoFirstIncident(identifier, new []{option0,option1,option2});
		}
		public Strategy ThenDoFirstIncident(string identifier, (Proc.Id, Proc.edure) option0, (Proc.Id, Proc.edure) option1, (Proc.Id, Proc.edure) option2, (Proc.Id, Proc.edure) option3) {
			return ThenDoFirstIncident(identifier, new []{option0,option1,option2,option3});
		}

		public Strategy ThenDoFirstIncident(string identifier, (Proc.Id, Proc.edure)[] options) {
			Strategy deferringStrategy = null;
			bool finished = false;
			Proc.Result DoneWithOptionsAndContinue(Incident incident) {
				if (finished) return Proc.Result.Halt;
				finished = true;
				for (int i = 0; i < options.Length; ++i) {
					Proc.RemoveIncident(options[i].Item1, options[i].Item2);
				}
				return deferringStrategy.ContinueChain(incident);
			}
			deferringStrategy = ThenImmediately("(incident decide)" + identifier, incident => {
				for (int i = 0; i < options.Length; ++i) {
					Proc.OnIncident(options[i].Item1, options[i].Item2, 1, DoneWithOptionsAndContinue);
				}
				return Proc.Result.Halt;
			});
			return deferringStrategy;
		}
		public static Strategy PickFirstGreaterThanZero(IList<Strategy> decisions) {
			for (int i = 0; i < decisions.Count; ++i) {
				float v = decisions[i].MeritHeuristic?.Invoke() ?? MinimumMeritHeuristic();
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
				float v = decisions[i].MeritHeuristic?.Invoke() ?? MinimumMeritHeuristic();
				if (best < 0 || v > bestValue) {
					best = i;
					bestValue = v;
				}
			}
			if (best >= 0) { return decisions[best]; }
			return null;
		}
		public Error NextExceptionHandler(Type exceptionType) {
			Strategy s = Next;
			bool exceptionBlockStarted = false;
			while (s != null) {
				if (s.IsExceptionHandler) {
					exceptionBlockStarted = true;
					if (s is Error e && e.ErrorType.IsAssignableFrom(exceptionType)) {
						return e;
					}
				} else if (exceptionBlockStarted) {
					return null;
				}
				s = s.Next;
			} return null;
		}
		public Strategy NextFinallyHandler() { Strategy s = Next; while (s != null && !s.IsFinallyHandler) { s = s.Next; } return s; }
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
		public void ClearEvent() {
			if (IsDeferred) {
				Proc.RemoveIncident(Identifier, InvokeChain);
			}
		}
		public void ClearAllEvents() {
			Strategy s = this;
			while (s != null) {
				s.ClearEvent();
				s = s.Next;
			}
		}
		public Strategy ThenImmediately(string identifier, Proc.edureSimple procedure) { return ThenImmediately(identifier, Proc.ConvertR(procedure, true)); }
		public Strategy ThenImmediately(string identifier, Action procedure) { return ThenImmediately(identifier, Proc.ConvertR(procedure, true)); }
		public Strategy AndThen(string identifier, Proc.edureSimple procedure) { return AndThen(identifier, Proc.ConvertR(procedure, true)); }
		public Strategy AndThen(string identifier, Action procedure) { return AndThen(identifier, Proc.ConvertR(procedure, true)); }
		public Strategy ThenOnIncident(Proc.Id id, Proc.edureSimple procedure, int count = 1) { return ThenOnIncident(id, Proc.ConvertR(procedure, true), count); }
		public Strategy ThenOnIncident(Proc.Id id, Action procedure, int count = 1) { return ThenOnIncident(id, Proc.ConvertR(procedure, true), count); }
		public Strategy ThenDelay(string identifier, int ms, Proc.edureSimple procedure) { return ThenDelay(identifier, ms, Proc.ConvertR(procedure, true)); }
		public Strategy ThenDelay(string identifier, int ms, Action procedure) { return ThenDelay(identifier, ms, Proc.ConvertR(procedure, true)); }
		public Strategy Finally(string name, Proc.edure response) {
			Strategy p = Last();
			p.Next = ThenImmediately(name, response);
			p.Next.IsFinallyHandler = true;
			return p.Next;
		}
		public Strategy Finally(Proc.edure procedure) { return Finally("finally", procedure); }
		public Strategy Finally(Proc.edureSimple procedure) { return Finally("finally", Proc.ConvertR(procedure, false)); }
		public Strategy Finally(Action procedure) { return Finally("finally", Proc.ConvertR(procedure, false)); }

		public Strategy Catch<E>(Proc.edure procedure) where E : Exception {
			return Catch<E>("catch " + typeof(E).Name, procedure);
		}
		public Strategy Catch<E>(Proc.edureSimple procedure) where E : Exception {
			return Catch<E>("catch " + typeof(E).Name, Proc.ConvertR(procedure, false));
		}
		public Strategy Catch<E>(Action procedure) where E : Exception {
			return Catch<E>("catch " + typeof(E).Name, Proc.ConvertR(procedure, false));
		}
		public Strategy Catch<E>(string name, Proc.edure procedure) where E : Exception {
			Strategy p = Last();
			p.Next = new Error(name, typeof(E), procedure);
			return p.Next;
		}
		public class Error : Strategy {
			public readonly Type ErrorType;
			public Error(string name, Type t, Proc.edure action) : base(name, action) {
				ErrorType = t;
				IsExceptionHandler = true;
			}
		}
	}

	public static partial class Proc {
		/// <summary>
		/// this function adds an entry to a table that might not be removed if it is called from outside <see cref="NonStandard.Procedure"/> algorithms
		/// </summary>
		internal static Proc.edure ConvertR(Strategy s, bool cacheIfNotFound) { return Main.ConvertR(s, cacheIfNotFound); }
		public static Incident Delay(long delay, Strategy strategy) { return SystemClock.Delay(delay, strategy); }
		public static Incident Enqueue(Strategy strategy) { return SystemClock.Delay(0, strategy); }
	}
}