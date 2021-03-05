using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenericAStar<NODE_TYPE, EDGE_TYPE> {

	bool finished = false;
	public List<NODE_TYPE> BestPath;
	public bool IsFinished() { return finished; }

	public delegate List<EDGE_TYPE> GetEdges(NODE_TYPE node);
	public delegate NODE_TYPE GetTo(NODE_TYPE from, EDGE_TYPE edge);
	public delegate float DistBetween(NODE_TYPE a, NODE_TYPE b);

	GetEdges getEdges;
	GetTo getEdgeToNode;
	//public static float dist_between(NODE_TYPE a, NODE_TYPE b) { return Vector3.Distance(a.owner.transform.position, b.owner.transform.position); }
	DistBetween dist_between;

	public void SetNodeAndEdgeMethods(GetEdges getEdges, GetTo getEdgeToNode, DistBetween distanceHeuristic) {
		this.getEdges = getEdges;
		this.getEdgeToNode = getEdgeToNode;
		this.dist_between = distanceHeuristic;
	}

	public float heuristicWeight = 10;

	//function A*(start,goal)
	public NODE_TYPE start, goal;
	// closedset := the empty set    // The set of nodes already evaluated.
	public List<NODE_TYPE> closedset = new List<NODE_TYPE>(); // TODO make this a HashSet
	public List<NODE_TYPE> openset = new List<NODE_TYPE>();
	// came_from := the empty map    // The map of navigated nodes.
	public Dictionary<NODE_TYPE, NODE_TYPE> came_from = new Dictionary<NODE_TYPE, NODE_TYPE>();
	public Dictionary<NODE_TYPE, float> g_score = new Dictionary<NODE_TYPE, float>();
	public Dictionary<NODE_TYPE, float> f_score = new Dictionary<NODE_TYPE, float>();

	public GenericAStar(GetEdges getEdges, GetTo getEdgeToNode, DistBetween distanceHeuristic) {
		SetNodeAndEdgeMethods(getEdges, getEdgeToNode, distanceHeuristic);
	}
	public void Start(NODE_TYPE start, NODE_TYPE goal) {
		openset.Clear();
		closedset.Clear();
		came_from.Clear();
		g_score.Clear();
		f_score.Clear();
		BestPath = null;
		finished = false;
		this.start = start;
		this.goal = goal;
		// openset := {start} // The set of tentative nodes to be evaluated, initially containing the start node
		openset.Add(start);
		// g_score[start] := 0 // Cost from start along best known path.
		g_score[start] = 0;
		// // Estimated total cost from start to goal through y.
		// f_score[start] := g_score[start] + heuristic_cost_estimate(start, goal)
		f_score[start] = g_score[start] + heuristic_cost_estimate(start, goal);
	}

	/// <summary>calculates A-star, one iteration at a time</summary>
	/// <returns>null until a path is found. this.finished is set to true when a path is found, or all posibilities are exhausted.</returns>
	public List<NODE_TYPE> Update() {
		List<EDGE_TYPE> edges;
		// while openset is not empty
		/*while*/
		if (openset.Count > 0) {
			// current := the node in openset having the lowest f_score[] value
			NODE_TYPE current = smallestFrom(openset, f_score);
			// remove current from openset
			openset.Remove(current);
			// add current to closedset
			closedset.Add(current);
			// if current = goal
			if (current.Equals(goal)) {
				// return reconstruct_path(came_from, goal)
				return BestPath = reconstruct_path(came_from, goal);
			}
			// for each neighbor in neighbor_nodes(current)
			edges = getEdges(current);
			for (int i = 0; edges != null && i < edges.Count; ++i) {
				NODE_TYPE neighbor = getEdgeToNode(current, edges[i]); // current.edges[i].end2;
				// tentative_g_score := g_score[current] + dist_between(current,neighbor)
				float tentative_g_score = g_score[current] + dist_between(current, neighbor);
				// if neighbor in closedset and tentative_g_score >= g_score[neighbor]
				if (closedset.IndexOf(neighbor) >= 0 && tentative_g_score >= g_score[neighbor]) {
					// continue
					continue;
				}
				// if neighbor not in openset or tentative_g_score < g_score[neighbor] 
				if (openset.IndexOf(neighbor) < 0 || tentative_g_score < g_score[neighbor]) {
					// came_from[neighbor] := current
					came_from[neighbor] = current;
					// g_score[neighbor] := tentative_g_score
					g_score[neighbor] = tentative_g_score;
					// f_score[neighbor] := g_score[neighbor] + heuristic_cost_estimate(neighbor, goal)
					f_score[neighbor] = g_score[neighbor] + heuristic_cost_estimate(neighbor, goal);
					// if neighbor not in openset
					if (openset.IndexOf(neighbor) < 0) {
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
	public static NODE_TYPE smallestFrom(List<NODE_TYPE> list, Dictionary<NODE_TYPE, float> values) {
		int minIndex = 0;
		float smallest = values[list[minIndex]], score;
		for (int i = 1; i < list.Count; ++i) {
			score = values[list[i]];
			if (score < smallest) {
				minIndex = i;
				smallest = score;
			}
		}
		return list[minIndex];
	}
	static List<NODE_TYPE> reconstruct_path(Dictionary<NODE_TYPE, NODE_TYPE> previous, NODE_TYPE goal) {
		List<NODE_TYPE> list = new List<NODE_TYPE>();
		NODE_TYPE prev = goal;
		bool hasPreviousNode;
		do {
			list.Add(prev);
			hasPreviousNode = previous.TryGetValue(prev, out prev);
		} while (hasPreviousNode);
		return list;
	}
}
