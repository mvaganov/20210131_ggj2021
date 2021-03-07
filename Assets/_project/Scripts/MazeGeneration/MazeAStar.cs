using MazeGeneration;
using System;
using System.Collections.Generic;
using Random = NonStandard.Data.Random;

public class MazeAStar : GenericAStar<Coord, int> {
	public VisionMapping vision;
	Func<Map2d> getMap;
	public Map2d Map => getMap();
	public MazeAStar() { }
	public MazeAStar(VisionMapping vision, Func<Map2d> getMap, Action reset_state,
		Func<Coord, Coord> get_came_from, Action<Coord, Coord> set_came_from,
		Func<Coord, float> get_f_score, Action<Coord, float> set_f_score,
		Func<Coord, float> get_g_score, Action<Coord, float> set_g_score) {
		SetAstarSourceData(vision, getMap, reset_state, get_came_from, set_came_from, get_f_score, set_f_score, get_g_score, set_g_score);
	}

	public void SetAstarSourceData(VisionMapping vision, Func<Map2d> getMap, Action reset_state,
		Func<Coord, Coord> get_came_from, Action<Coord, Coord> set_came_from,
		Func<Coord, float> get_f_score, Action<Coord, float> set_f_score,
		Func<Coord, float> get_g_score, Action<Coord, float> set_g_score) {
		this.getMap = getMap;
		this.vision = vision;
		SetNodeAndEdgeMethods(GetEdges, NextNode, Dist, reset_state, get_came_from, set_came_from, get_f_score, set_f_score, get_g_score, set_g_score);
	}

	Coord[] edgeDirs = new Coord[] {
		Coord.Up, Coord.Left, Coord.Down, Coord.Right,
		Coord.Up+Coord.Left,Coord.Up+Coord.Right,Coord.Down+Coord.Left,Coord.Down+Coord.Right,
		Coord.Up+Coord.Up,Coord.Left+Coord.Left,Coord.Down+Coord.Down,Coord.Right+Coord.Right,
			Coord.Up+Coord.Up+Coord.Left,Coord.Up+Coord.Up+Coord.Right,
			Coord.Down+Coord.Down+Coord.Left,Coord.Down+Coord.Down+Coord.Right,
			Coord.Up+Coord.Left+Coord.Left,Coord.Up+Coord.Right+Coord.Right,
			Coord.Down+Coord.Left+Coord.Left,Coord.Down+Coord.Right+Coord.Right,
	};
	public enum EdgeDir { N, W, S, E, NW, NE, SW, SE, NN, WW, SS, EE, NNW, NNE, SSW, SSE, NWW, NEE, SWW, SEE }
	public List<int> GetEdges(Coord node) { return GetEdges(node, false); }
	public List<int> GetEdgesWithJump(Coord node) { return GetEdges(node, true); }
	public List<int> GetEdges(Coord node, bool canJump) {
		Coord size = Map.GetSize();
		List<int> edges = new List<int>();
		if (!size.Contains(node) || vision == null || !vision[node]) { return edges; } // no edges from unknown tiles
		char thisSquare = size.Contains(node) ? Map[node].letter : '\0';
		int dirOptions = canJump ? edgeDirs.Length : 4;
		switch (thisSquare) {
		case '#':
			for (int i = 0; i < dirOptions; ++i) {
				if (Map.Contains(edgeDirs[i] + node)) { edges.Add(i); }
			}
			break;
		case 'n':
		case 'w':
		case 's':
		case 'e':
			for (int dir = 0; dir < 4; ++dir) {
				Coord other = node + edgeDirs[dir];
				if (!size.Contains(other)) { continue; }
				char otherSq = Map[other];
				bool walkable = false, jumpable = false;
				switch (otherSq) {
				case ' ': walkable = true; break;
				case '#':
					walkable = false;
					jumpable = true;
					switch (thisSquare) {
					case 'n': walkable = (dir == (int)EdgeDir.N); break;
					case 'w': walkable = (dir == (int)EdgeDir.W); break;
					case 's': walkable = (dir == (int)EdgeDir.S); break;
					case 'e': walkable = (dir == (int)EdgeDir.E); break;
					}
					break;
				case 'n':
					walkable = (dir == (int)EdgeDir.N);
					jumpable = (dir != (int)EdgeDir.S && dir < 4);
					break;
				case 'w':
					walkable = (dir == (int)EdgeDir.W);
					jumpable = (dir != (int)EdgeDir.E && dir < 4);
					break;
				case 's':
					walkable = (dir == (int)EdgeDir.S);
					jumpable = (dir != (int)EdgeDir.N && dir < 4);
					break;
				case 'e':
					walkable = (dir == (int)EdgeDir.E);
					jumpable = (dir != (int)EdgeDir.W && dir < 4);
					break;
				}
				if (walkable || (canJump && jumpable)) {
					edges.Add(dir);
				}
			}
			break;
		case ' ':
			for (int i = 0; i < 4; ++i) {
				Coord other = node + edgeDirs[i];
				if (!size.Contains(other)) { continue; }
				char otherSq = Map[other];
				bool walkable = false, jumpable = false;
				switch (otherSq) {
				case ' ': walkable = true; break;
				case 'n':
					walkable = (i == (int)EdgeDir.N);
					jumpable = (i != (int)EdgeDir.S && i < 4);
					break;
				case 'w':
					walkable = (i == (int)EdgeDir.W);
					jumpable = (i != (int)EdgeDir.E && i < 4);
					break;
				case 's':
					walkable = (i == (int)EdgeDir.S);
					jumpable = (i != (int)EdgeDir.N && i < 4);
					break;
				case 'e':
					walkable = (i == (int)EdgeDir.E);
					jumpable = (i != (int)EdgeDir.W && i < 4);
					break;
				}
				if (walkable || (canJump && jumpable)) {
					edges.Add(i);
				}
			}
			break;
		}
		return edges;
	}

	public Coord NextNode(Coord here, int dirIndex) { return here + edgeDirs[dirIndex]; }
	public float Dist(Coord a, Coord b) {
		int dx = a.X - b.X, dy = a.Y - b.Y;
		return (float)Math.Sqrt(dx*dx+dy*dy);
	}

	public List<Coord> Moves(Coord c, bool canJump) {
		List<int> edges = GetEdges(c, canJump);
		List<Coord> coords = new List<Coord>();
		coords.Capacity = edges.Count;
		for (int i = 0; i < edges.Count; ++i) {
			coords.Add(NextNode(c, edges[i]));
		}
		return coords;
	}

	public bool RandomVisibleNode(out Coord c, Coord currentNode) {
		Coord size = Map.GetSize();
		c = new Coord(Random.A.Next(size.X), Random.A.Next(size.Y));
		if (vision[c]) { return true; }
		return false;
	}
	public bool RandomNeighborNode(out Coord c, Coord currentNode) {
		c = currentNode;
		List<int> edges = GetEdges(c);
		if (edges.Count > 0) {
			c = NextNode(c, edges[Random.A.Next(edges.Count)]);
			return true;
		}
		return false;
	}
}
