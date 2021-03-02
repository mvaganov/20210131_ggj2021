using MazeGeneration;
using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeStarWalker : MonoBehaviour
{
    public MazeLevel maze;
    public GameObject textOutput;
    public bool canJump;

    GenericAStar<Coord, int> astar;

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
    public List<int> GetEdges(Coord node) {
        List<int> edges = new List<int>();
        char thisSquare = maze.Map[node];
        Coord size = maze.Map.GetSize();
        int dirOptions = canJump ? edgeDirs.Length : 4;
        switch (thisSquare) {
        case '#':
            for (int i = 0; i < dirOptions; ++i) { edges.Add(i); }
            break;
        case 'n': case 'w': case 's': case 'e':
            for (int i = 0; i < 4; ++i) { edges.Add(i); }
            break;
        case ' ':
            for (int i = 0; i < 4; ++i) {
                Coord other = node + edgeDirs[i];
                if (!size.Contains(other)) { continue; }
                char otherSq = maze.Map[other];
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

    Coord NextNode(Coord here, int dirIndex) { return here + edgeDirs[dirIndex]; }
    float Dist(Coord a, Coord b) { return ((Vector2)a - (Vector2)b).magnitude; }

    List<Coord> Moves(Coord c) {
        List<int> edges = GetEdges(c);
        List<Coord> coords = new List<Coord>();
        coords.Capacity = edges.Count;
        for(int i = 0; i < edges.Count; ++i) {
            coords.Add(NextNode(c, edges[i]));
		}
        return coords;
	}

    List<Vector3> MoveToWorld(List<Coord> moves, float y) {
        List<Vector3> world = new List<Vector3>();
        world.Capacity = moves.Count;
        Vector3 o = maze.transform.position;
        for(int i = 0; i < moves.Count; ++i) {
            Vector3 v = maze.GetPosition(moves[i]) + o;
            v.y = y;
            world.Add(v);
		}
        return world;
    }

    void Start()
    {
        astar = new GenericAStar<Coord, int>(Coord.Zero, Coord.One * 3, GetEdges, NextNode, Dist);
    }

    List<Lines.Wire> wires = new List<Lines.Wire>();
    void Update()
    {
        Vector3 p = transform.position;
        Coord c = maze.GetCoord(p);
        List<Coord> moves = Moves(c);
        UiText.SetText(textOutput, c.ToString()+":"+(p - maze.transform.position)+" "+moves.JoinToString(", "));
        List<Vector3> world = MoveToWorld(moves, p.y);
        for(int i = 0; i < world.Count; ++i) {
			while (wires.Count <= i) { wires.Add(Lines.MakeWire()); }
            //Debug.Log(wires[i] + "  " + p + " " + world[i]);
            wires[i].Line(p, world[i], Color.yellow);
            wires[i].gameObject.SetActive(true);
        }
        for (int i = world.Count; i < wires.Count; ++i) {
            wires[i].gameObject.SetActive(false);
		}
    }
}
