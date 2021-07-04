using System.Collections.Generic;
using UnityEngine;

public class Strategy
{
	public Procedure.Reaction Reaction;
	public string Identifier;
	public Strategy Next;
	public Strategy Prev;
	public bool WaitForUpdate = false;

	public override string ToString() { return Identifier; }

	public Procedure.Result Invoke(Incident incident) {
		Procedure.Result result = Procedure.Result.Success;
		//Debug.Log("Invoking " + Identifier);
		if(Reaction != null) {
			result = Reaction.Invoke(incident);
		}
		if (Next == null || result != Procedure.Result.Success) return result;
		if (!Next.WaitForUpdate) {
			result = Next.Invoke(incident);
		} else {
			Procedure.Delay(0, delayedIncident => Next.Invoke(incident));
		}
		return result;
	}

	public Strategy(string identifier, Strategy prev = null) { Identifier = identifier; Prev = prev; }
	public Strategy(string identifier, Procedure.Reaction action, Strategy prev) : this(identifier, prev) {
		Reaction = action;
	}
	public Strategy ThenImmediately(string identifier, Procedure.Reaction reaction) {
		return Next = new Strategy(identifier, reaction, this);
	}
	public Strategy AndThen(string identifier, Procedure.Reaction reaction) {
		Next = new Strategy(identifier, reaction, this);
		Next.WaitForUpdate = true;
		return Next;
	}
	public Strategy ThenOnIncident(string incidentId, Procedure.Reaction reaction) {
		Strategy deferringStragegy = null;
		deferringStragegy = ThenImmediately("(defer)" + incidentId, incident => {
			Strategy deferredStrategy = new Strategy("(deferred)" + incidentId, reaction, this);
			deferredStrategy.Next = deferringStragegy.Next;
			Procedure.OnIncident(incidentId, deferredStrategy.Invoke, 1);
			return Procedure.Result.Halt;
		});
		return deferringStragegy;
	}
	public Strategy ThenDelay(string identifier, int ms, Procedure.Reaction reaction) {
		Strategy deferringStrategy = null;
		deferringStrategy = ThenImmediately("(wait)" + identifier, incident => {
			Strategy deferredStrategy = new Strategy(identifier, reaction, this);
			deferredStrategy.Next = deferringStrategy.Next;
			Procedure.Delay(ms, deferredStrategy.Invoke);
			return Procedure.Result.Halt;
		});
		return deferringStrategy;
	}
	public Strategy ThenDecideBestChoice(string identifier, IList<Decision> decisions) {
		Strategy deferringStrategy = null;
		deferringStrategy = ThenImmediately("(decide)" + identifier, incident => {
			Decision d = Decision.PickBest(decisions);
			//Debug.Log("Picked [" + d.ListStrategies().JoinToString()+"]");
			if (d == null) {
				Debug.Log("Could not decide");
				return Procedure.Result.Failure;
			}
			//Strategy deferredStrategy = new Strategy(identifier, d.Reaction, this);
			d.Last().Next = deferringStrategy.Next;
			//Debug.Log("About to invoke [" + d.ListStrategies().JoinToString() + "]");
			d.Invoke(incident);
			return Procedure.Result.Halt;
		});
		return deferringStrategy;
	}
	public Strategy Last() { Strategy s = this; while (s.Next != null) { s = s.Next; } return s; }
	public Strategy Root() { Strategy s = this; while(s.Prev != null) { s = s.Prev; } return s; }
	public Decision RootDecision() { Strategy s = this; while (s.Prev != null) { s = s.Prev; } return s as Decision; }
	public List<Strategy> ListStrategies() {
		Strategy s = this;
		List<Strategy> strats = new List<Strategy>();
		while (s != null) {
			strats.Add(s);
			s = s.Next;
		}
		return strats;
	}
	public Strategy(string identifier, Procedure.ReactionNoReturn action, Strategy prev)
		: this(identifier, Procedure.ToReaction(action), prev) {}
	public Strategy ThenImmediately(string identifier, Procedure.ReactionNoReturn reaction) {
		return ThenImmediately(identifier, Procedure.ToReaction(reaction));
	}
	public Strategy AndThen(string identifier, Procedure.ReactionNoReturn reaction) {
		return AndThen(identifier, Procedure.ToReaction(reaction));
	}
	public Strategy ThenOnIncident(string incidentId, Procedure.ReactionNoReturn reaction) {
		return ThenOnIncident(incidentId, Procedure.ToReaction(reaction));
	}
	public Strategy ThenDelay(string identifier, int ms, Procedure.ReactionNoReturn reaction) {
		return ThenDelay(identifier, ms, Procedure.ToReaction(reaction));
	}
}
