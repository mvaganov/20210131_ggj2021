using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GenericAStar<NODE_TYPE, EDGE_TYPE> {

	bool finished = false;
	public List<NODE_TYPE> BestPath;
	public bool IsFinished() { return finished; }

	Func<NODE_TYPE, List<EDGE_TYPE>> getEdges;
	Func<NODE_TYPE, EDGE_TYPE, NODE_TYPE> getEdgeToNode;
	Func<NODE_TYPE, NODE_TYPE, float> dist_between;

	public void SetNodeAndEdgeMethods(Func<NODE_TYPE, List<EDGE_TYPE>> getEdges, Func<NODE_TYPE, EDGE_TYPE, NODE_TYPE> getEdgeToNode, Func<NODE_TYPE, NODE_TYPE, float> distanceHeuristic,
		Action reset_state,
		Func<NODE_TYPE, NODE_TYPE> get_came_from, Action<NODE_TYPE, NODE_TYPE> set_came_from,
		Func<NODE_TYPE, float> get_f_score, Action<NODE_TYPE, float> set_f_score,
		Func<NODE_TYPE, float> get_g_score, Action<NODE_TYPE, float> set_g_score) {
		this.getEdges = getEdges;
		this.getEdgeToNode = getEdgeToNode;
		this.dist_between = distanceHeuristic;
		this.reset_state = reset_state;
		this.get_came_from = get_came_from;
		this.set_came_from = set_came_from;
		this.get_f_score = get_f_score;
		this.set_f_score = set_f_score;
		this.get_g_score = get_g_score;
		this.set_g_score = set_g_score;
	}

	public float heuristicWeight = 10;

	//function A*(start,goal)
	public NODE_TYPE start, goal;
	// closedset := the empty set    // The set of nodes already evaluated.
	public HashSet<NODE_TYPE> closedset = new HashSet<NODE_TYPE>();
	public HashSet<NODE_TYPE> openset = new HashSet<NODE_TYPE>();
	// came_from := the empty map    // The map of navigated nodes.
	public Action reset_state;
	public Func<NODE_TYPE, NODE_TYPE> get_came_from;
	public Action<NODE_TYPE, NODE_TYPE> set_came_from;
	public Func<NODE_TYPE, float> get_f_score;
	public Action<NODE_TYPE, float> set_f_score;
	public Func<NODE_TYPE, float> get_g_score;
	public Action<NODE_TYPE, float> set_g_score;

	public GenericAStar() { }
	public GenericAStar(Func<NODE_TYPE, List<EDGE_TYPE>> getEdges, Func<NODE_TYPE, EDGE_TYPE, NODE_TYPE> getEdgeToNode, Func<NODE_TYPE, NODE_TYPE, float> distanceHeuristic,
				Action reset_state,
		Func<NODE_TYPE, NODE_TYPE> get_came_from, Action<NODE_TYPE, NODE_TYPE> set_came_from,
		Func<NODE_TYPE, float> get_f_score, Action<NODE_TYPE, float> set_f_score,
		Func<NODE_TYPE, float> get_g_score, Action<NODE_TYPE, float> set_g_score) {
		SetNodeAndEdgeMethods(getEdges, getEdgeToNode, distanceHeuristic, reset_state, get_came_from, set_came_from, get_f_score, set_f_score, get_g_score, set_g_score);
	}
	public void Start(NODE_TYPE start, NODE_TYPE goal) {
		openset.Clear();
		closedset.Clear();
		reset_state();
		BestPath = null;
		finished = false;
		this.start = start;
		this.goal = goal;
		// openset := {start} // The set of tentative nodes to be evaluated, initially containing the start node
		openset.Add(start);
		set_g_score(start, 0); // Cost from start along best known path.
		// Estimated total cost from start to goal through y.
		set_f_score(start, get_g_score(start) + heuristic_cost_estimate(start, goal));
	}

	/// <summary>calculates A-star, one iteration at a time</summary>
	/// <returns>null until a path is found. this.finished is set to true when a path is found, or all posibilities are exhausted.</returns>
	public List<NODE_TYPE> Update() {
		List<EDGE_TYPE> edges;
		// while openset is not empty
		/*while*/
		if (openset.Count > 0) {
			// current := the node in openset having the lowest f_score[] value
			NODE_TYPE current = smallestFrom(openset, get_f_score);
			// remove current from openset
			openset.Remove(current);
			// add current to closedset
			closedset.Add(current);
			// if current = goal
			if (current.Equals(goal)) {
				// return reconstruct_path(came_from, goal)
				return BestPath = reconstruct_path(get_came_from, goal);
			}
			// for each neighbor in neighbor_nodes(current)
			edges = getEdges(current);
			for (int i = 0; edges != null && i < edges.Count; ++i) {
				NODE_TYPE neighbor = getEdgeToNode(current, edges[i]); // current.edges[i].end2;
				// tentative_g_score := g_score[current] + dist_between(current,neighbor)
				float tentative_g_score = get_g_score(current) + dist_between(current, neighbor);
				// if neighbor in closedset and tentative_g_score >= g_score[neighbor]
				if (closedset.Contains(neighbor) && tentative_g_score >= get_g_score(neighbor)) {
					// continue
					continue;
				}
				// if neighbor not in openset or tentative_g_score < g_score[neighbor] 
				if (!openset.Contains(neighbor) || tentative_g_score < get_g_score(neighbor)) {
					// came_from[neighbor] := current
					set_came_from(neighbor, current);
					// g_score[neighbor] := tentative_g_score
					set_g_score(neighbor, tentative_g_score);
					// f_score[neighbor] := g_score[neighbor] + heuristic_cost_estimate(neighbor, goal)
					set_f_score(neighbor, get_g_score(neighbor) + heuristic_cost_estimate(neighbor, goal));
					// if neighbor not in openset
					if (!openset.Contains(neighbor)) {
						// add neighbor to openset
						openset.Add(neighbor);
					}
				}
			}
		}
		// return failure
		finished = openset.Count == 0;
		return null;
	}
	public float heuristic_cost_estimate(NODE_TYPE start, NODE_TYPE goal) {
		return dist_between(start, goal) * heuristicWeight;
	}
	//public static NODE_TYPE smallestFrom(ICollection<NODE_TYPE> list, Dictionary<NODE_TYPE, float> values) {
	public static NODE_TYPE smallestFrom(ICollection<NODE_TYPE> list, Func<NODE_TYPE, float> values) {
		//int minIndex = 0;
		IEnumerator<NODE_TYPE> e = list.GetEnumerator();
		if (!e.MoveNext()) return default(NODE_TYPE);
		NODE_TYPE bestNode = e.Current;
		float smallest = values(bestNode), score;
		//for (int i = 1; i < list.Count; ++i) {
		while (e.MoveNext()) {
			score = values(e.Current);
			if (score < smallest) {
				//minIndex = i;
				bestNode = e.Current;
				smallest = score;
			}
		}
		e.Dispose();
		return bestNode;//list[minIndex];
	}
	static List<NODE_TYPE> reconstruct_path(Func<NODE_TYPE, NODE_TYPE> previous, NODE_TYPE goal) {
		List<NODE_TYPE> list = new List<NODE_TYPE>();
		NODE_TYPE prev = goal;
		int loopGuard = 0;
		do {
			list.Add(prev);
			prev = previous(prev);
			if(prev.Equals(list[list.Count - 1])) { break; }
		} while (++loopGuard < 1<<20);// hasPreviousNode);
		return list;
	}
}
