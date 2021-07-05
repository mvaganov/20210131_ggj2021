//using System;
//using System.Collections.Generic;

//namespace NonStandard.Procedure {
//	/// <summary>
//	/// a more advanced kind of strategy, one that uses a heuristic to determine if it should be executed
//	/// </summary>
//	public class Contingency : Strategy {
//		//public delegate float ValueHeuristicFunction();
//		//public ValueHeuristicFunction ValueHeuristic;
//		//public static float ZeroHeuristic() => 0;

//		//public Contingency(string identifier, ValueHeuristicFunction valueHeuristic, Proc.edure reaction, Strategy prev = null)
//		//: base(identifier, reaction, prev) {
//		//	ValueHeuristic = valueHeuristic;
//		//}
//		//public Contingency(string identifier, ValueHeuristicFunction valueHeuristic, Proc.edureSimple reaction, Strategy prev = null)
//		//: base(identifier, Proc.ConvertR(reaction), prev) {
//		//	ValueHeuristic = valueHeuristic;
//		//}
//		//public Contingency(string identifier, Strategy prev = null)
//		//: base(identifier, null, prev) { ValueHeuristic = ZeroHeuristic; }
//		public static Contingency PickFirstGreatherThanZero(IList<Contingency> decisions) {
//			for (int i = 0; i < decisions.Count; ++i) {
//				float v = decisions[i].ValueHeuristic.Invoke();
//				if (v > 0) {
//					return decisions[i];
//				}
//			}
//			return null;
//		}
//		public static Strategy PickBest(IList<Strategy> decisions) {
//			float bestValue = 0;
//			int best = -1;
//			for (int i = 0; i < decisions.Count; ++i) {
//				float v = 0;
//				if(decisions[i] is Contingency c) {
//					v = c.ValueHeuristic.Invoke();
//				}
//				if (best < 0 || v > bestValue) {
//					best = i;
//					bestValue = v;
//				}
//			}
//			if (best >= 0) { return decisions[best]; }
//			return null;
//		}
//	}
//}