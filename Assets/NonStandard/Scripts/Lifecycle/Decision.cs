using System;
using System.Collections.Generic;

public class Decision : Strategy
{
	public Func<int> ValueHeuristic;

	public Decision(string identifier, Func<int> valueHeuristic, Procedure.Reaction reaction, Strategy prev = null) 
	: base(identifier, reaction, prev) {
		ValueHeuristic = valueHeuristic;
	}
	public Decision(string identifier, Func<int> valueHeuristic, Procedure.ReactionNoReturn reaction, Strategy prev = null)
	: base(identifier, Procedure.ToReaction(reaction), prev) {
		ValueHeuristic = valueHeuristic;
	}
	public static Decision PickFirstGreatherThanZero(IList<Decision> decisions) {
		for (int i = 0; i < decisions.Count; ++i) {
			int v = decisions[i].ValueHeuristic.Invoke();
			if (v > 0) {
				return decisions[i];
			}
		}
		return null;
	}
	public static Decision PickBest(IList<Decision> decisions) {
		int bestValue = 0;
		int best = -1;
		for(int i = 0; i < decisions.Count; ++i) {
			int v = decisions[i].ValueHeuristic.Invoke();
			if(best < 0 || v > bestValue) {
				best = i;
				bestValue = v;
			}
		}
		if(best >= 0) { return decisions[best]; }
		return null;
	}
}
