using MazeGeneration;
using NonStandard;
using NonStandard.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeStarWalker : MonoBehaviour
{
    public MazeLevel maze;
    public GameObject textOutput;
    public bool canJump;
    public Discovery discovery;
    public Game game;
    public ClickToMoveFollower follower;
    GenericAStar<Coord, int> astar;
    CharacterMove cm;

    public enum AiBehavior { None, RandomLocalEdges, RandomInVision }
    public AiBehavior aiBehavior = AiBehavior.None;

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
        Coord size = maze.Map.GetSize();
        List<int> edges = new List<int>();
		if (!size.Contains(node) || discovery == null || discovery.vision == null || !discovery.vision[node]) { return edges; } // no edges from ubknown tiles
        char thisSquare = size.Contains(node) ? maze.Map[node].letter : '\0';
        int dirOptions = canJump ? edgeDirs.Length : 4;
        switch (thisSquare) {
        case '#':
            for (int i = 0; i < dirOptions; ++i) { edges.Add(i); }
            break;
        case 'n': case 'w': case 's': case 'e':
            for (int dir = 0; dir < 4; ++dir) {
                Coord other = node + edgeDirs[dir];
                if (!size.Contains(other)) { continue; }
                char otherSq = maze.Map[other];
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
        astar = new GenericAStar<Coord, int>(GetEdges, NextNode, Dist);
        cm = GetComponent<CharacterMove>();
        follower = game.clickToMove.Follower(cm);
        discovery = game.EnsureExplorer(gameObject);
        visionParticle = GetComponentInChildren<ParticleSystem>();
        Vector3 p = transform.position;
        Coord here = maze.GetCoord(p);
        astar.Start(here, here);
    }
    ParticleSystem visionParticle;
    List<Lines.Wire> wires = null;// new List<Lines.Wire>();
    private float timer = 0;
    bool RandomVisibleNode(out Coord c) {
        Coord size = maze.Map.GetSize();
        c = Coord.Up;
        for (int i = 0; i < 50; ++i) {
            c = new Coord(Random.Range(0, size.X), Random.Range(0, size.Y));
			if (discovery.vision[c]) { return true; }
		}
        Coord found = Coord.Up;
        if(!size.ForEach(co => { if (discovery.vision[co]) { found = co; return true; } return false; })) {
            return false;
		}
        c = found;
        return true;
    }
    void Update()
    {
        Vector3 p = transform.position;
        Coord here = maze.GetCoord(p);
        List<Coord> moves = Moves(here);
        if (textOutput != null) {
            UiText.SetText(textOutput, here.ToString() + ":" + (p - maze.transform.position) + " " + moves.JoinToString(", "));
        }
        List<Vector3> world = MoveToWorld(moves, p.y);
        if (wires != null) {
            for (int i = 0; i < world.Count; ++i) {
                while (wires.Count <= i) { wires.Add(Lines.MakeWire()); }
                //Debug.Log(wires[i] + "  " + p + " " + world[i]);
                wires[i].Line(p, world[i], Color.yellow);
                wires[i].gameObject.SetActive(true);
            }
            for (int i = world.Count; i < wires.Count; ++i) {
                wires[i].gameObject.SetActive(false);
            }
        }
        if (visionParticle) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                maze.Map.GetSize().ForEach(co => {
                    if (discovery.vision[co]) {
                        Vector3 po = maze.GetPosition(co) + maze.transform.position;
                        po.y = transform.position.y;
                        if (currentBestPath != null) {
                            if(currentBestPath.IndexOf(co) >= 0) {
                                po.y += 3;
                                visionParticle.transform.position = po;
                                visionParticle.Emit(1);
                            }
                        } else {
                            visionParticle.transform.position = po;
                            visionParticle.Emit(1);
                        }
                    }
                });
                timer = .5f;
            }
        }
		switch (aiBehavior) {
        case AiBehavior.RandomLocalEdges:
            if (!cm.IsAutoMoving()) {
                Vector3 t = world[Random.Range(0, world.Count)];
                cm.SetAutoMovePosition(t);
			}
            break;
        case AiBehavior.RandomInVision:
            if (astar.goal == here) {
                if (RandomVisibleNode(out Coord there)) {
                    astar.Start(here, there);
                    //Debug.Log("goal " + there+ " "+astar.IsFinished());
                } else {
                    //Debug.Log("nothing visible " + there);
                }
            } else {
                // iterate astar algorithm
				if (!astar.IsFinished()) {
                    astar.Update();
                } else if(astar.BestPath == null) {
                    //Debug.Log("f" + astar.IsFinished() + " " + astar.BestPath);
                    astar.Start(here, here);
                }
                if(astar.BestPath != null) {
                    if(astar.BestPath != currentBestPath) {
                        currentBestPath = astar.BestPath;
                        //Debug.Log(currentBestPath.JoinToString(", "));
                        indexOnBestPath = currentBestPath.IndexOf(here);
						if (indexOnBestPath < 0) { astar.Start(here, astar.goal); }
                        for(int i = indexOnBestPath; i >= 0; --i) {
                            Vector3 pos = maze.GetPosition(currentBestPath[i]) + maze.transform.position;
                            pos.y = transform.position.y + 3;
                            follower.AddWaypoint(pos);
                        }
                    } else {
						if (!cm.IsAutoMoving()) {
                            astar.Start(here,here);
      //                      //Debug.Log("#"+ indexOnBestPath+" "+ currentBestPath.JoinToString(", "));
      //                      if(indexOnBestPath < currentBestPath.Count && indexOnBestPath >= 0) {
      //                          Vector3 pos = maze.GetPosition(currentBestPath[indexOnBestPath]) + maze.transform.position;
      //                          pos.y = transform.position.y + 3;
      //                          cm.SetAutoMovePosition(pos, () => --indexOnBestPath);
      //                      }
                        }
                    }
                }
            }
            break;
        }
    }
    int indexOnBestPath = -1;
    List<Coord> currentBestPath;
}
