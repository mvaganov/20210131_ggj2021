﻿using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Extension;
using NonStandard.Ui;
using System.Collections.Generic;
using UnityEngine;
using Random = NonStandard.Data.Random;

public class MazeStarWalker : MonoBehaviour {
	public CharacterMove characterMover;
	public MazeLevel maze;
	public GameObject textOutput;
	public bool canJump;
	public Discovery discovery;
	public Game game;
	public ClickToMoveFollower follower;
	public GameObject prefab_debug_astar;

	private Transform _t;

	public enum AiBehavior { None, RandomLocalEdges, RandomInVision }
	public enum TileSurfaceChoice { Center, Random, Closest }
	public AiBehavior aiBehavior = AiBehavior.None;
	public TileSurfaceChoice tileSurfaceChoice = TileSurfaceChoice.Center;
	public LayerMask pathingIgnores;
	Map2dAStar mapAstar;

	Vector3 MoveablePosition(Coord coord, Vector3 currentPosition) {
		Vector3 p = maze.GetGroundPosition(coord);
		switch (tileSurfaceChoice) {
		case TileSurfaceChoice.Center: break;
		case TileSurfaceChoice.Random: {
			Vector3 extents = maze.tileSize / 2; extents.y = 0;
			Vector3 r = new Vector3(Random.NextFloat(-1,1) * extents.x, 0, extents.z * Random.NextFloat(-1,1));
			// raycasting hits something early quite often, resulting in very high points. not sure what is going on there.
			r.y = maze.tileSize.y;
			if (Physics.Raycast(p + r, Vector3.down, out RaycastHit rh, maze.tileSize.y * 2, ~pathingIgnores, QueryTriggerInteraction.Ignore)) {
				p = rh.point;
			}
		}
		break;
		case TileSurfaceChoice.Closest: {
			Vector3 extents = maze.tileSize / 2; extents.y = 0;
			extents.z -= follower.CharacterRadius*2;
			extents.x -= follower.CharacterRadius*2;
			float fDot = Vector3.Dot(Vector3.forward, currentPosition - (p + Vector3.forward * extents.z));
			float bDot = Vector3.Dot(Vector3.back, currentPosition - (p + Vector3.back * extents.z));
			float lDot = Vector3.Dot(Vector3.left, currentPosition - (p + Vector3.left * extents.x));
			float rDot = Vector3.Dot(Vector3.right, currentPosition - (p + Vector3.right* extents.x));
			Vector3 inner = currentPosition;
			if (fDot > 0) { inner.z = p.z+extents.z; }
			if (bDot > 0) { inner.z = p.z-extents.z; }
			if (rDot > 0) { inner.x = p.x + extents.x; }
			if (lDot > 0) { inner.x = p.x - extents.x; }
			inner.y = p.y + maze.tileSize.y;
			if (Physics.Raycast(inner, Vector3.down, out RaycastHit rh, maze.tileSize.y * 2, ~pathingIgnores, QueryTriggerInteraction.Ignore)) {
				p = rh.point;
			}
		}
		break;
		}
		return p + Vector3.up * follower.CharacterHeight;
	}
	List<Vector3> MoveToWorld(List<Coord> moves, float y) {
		List<Vector3> world = new List<Vector3>();
		world.Capacity = moves.Count;
		for(int i = 0; i < moves.Count; ++i) {
			Vector3 v = maze.GetPosition(moves[i]);
			v.y = y;
			world.Add(v);
		}
		return world;
	}
	void Start()
	{
		if (characterMover == null) { characterMover = GetComponent<CharacterMove>(); }
		_t = characterMover.transform;
		follower = game.clickToMove.Follower(characterMover);
		discovery = game.EnsureExplorer(characterMover.gameObject);
		visionParticle = GetComponentInChildren<ParticleSystem>();
	}
	public bool useVisionParticle = false;
	ParticleSystem visionParticle;
	private float timer = 0;

	void Update()
	{
		if (visionParticle != null) {
			if (characterMover.JumpButtonTimed > 0 && !visionParticle.isPlaying) {
				visionParticle.Play();
			} else if (characterMover.JumpButtonTimed == 0 && visionParticle.isPlaying) {
				visionParticle.Stop();
			}
		}
		Coord mapSize = maze.Map.GetSize();
		if (mapAstar == null) {
			mapAstar = new Map2dAStar(() => canJump, maze, discovery.vision, _t, prefab_debug_astar);
		}
		mapAstar.UpdateMapSize();
		Vector3 p = _t.position;
		Coord here = maze.GetCoord(p);
		if(follower.waypoints.Count > 0) {
			Coord there = maze.GetCoord(follower.waypoints[0].positon);
			if(here == there) {
				follower.NotifyWayPointReached();
			}
		}
		List<Coord> moves = mapAstar.Moves(here, canJump);
		if (textOutput != null) {
			UiText.SetText(textOutput, here.ToString() + ":" + (p - maze.transform.position) + " " + moves.JoinToString(", "));
		}
		if (useVisionParticle && visionParticle) {
			timer -= Time.deltaTime;
			if (timer <= 0) {
				mapSize.ForEach(co => {
					if (discovery.vision[co]) {
						Vector3 po = maze.GetPosition(co);
						po.y = _t.position.y;
						visionParticle.transform.position = po;
						visionParticle.Emit(1);
					}
				});
				timer = .5f;
			}
		}
		switch (aiBehavior) {
		case AiBehavior.RandomLocalEdges:
			if (!characterMover.IsAutoMoving()) {
				Coord c = moves[Random.Next(moves.Count)];
				characterMover.SetAutoMovePosition(MoveablePosition(c, p));
			}
			break;
		case AiBehavior.RandomInVision:
			if (mapAstar.goal == here) {
				if (mapAstar.RandomVisibleNode(out Coord there, here)) {
					//Debug.Log("startover #");
					//Debug.Log("goal " + there+ " "+astar.IsFinished());
				} else {
					mapAstar.RandomNeighborNode(out there, here);
				}
				mapAstar.Start(here, there);
			} else {
				// iterate astar algorithm
				if (!mapAstar.IsFinished()) {
					mapAstar.Update();
				} else if(mapAstar.BestPath == null) {
					//Debug.Log("f" + astar.IsFinished() + " " + astar.BestPath);
					mapAstar.Start(here, here);
					//Debug.Log("startover could not find path");
				}
				if(mapAstar.BestPath != null) {
					if(mapAstar.BestPath != currentBestPath) {
						currentBestPath = mapAstar.BestPath;
						List<Coord> nodes = new List<Coord>();
						Coord c = mapAstar.start;
						nodes.Add(c);
						for(int i = currentBestPath.Count-1; i >=0 ; --i) {
							c = mapAstar.NextNode(c, currentBestPath[i]);
							nodes.Add(c);
						}
						//Debug.Log(currentBestPath.JoinToString(", "));
						indexOnBestPath = nodes.IndexOf(here);
						if (indexOnBestPath < 0) {
							mapAstar.Start(here, mapAstar.goal);
							//Debug.Log("startover new better path");
						}
						Vector3 pos = p;
						follower.ClearWaypoints();
						for (int i = 0; i < currentBestPath.Count; ++i) {
							pos = MoveablePosition(nodes[i+1], pos);
							//pos.y += follower.CharacterHeight;
							//Show.Log(i + " " + nodes.Count + " " + currentBestPath.Count + " " + (currentBestPath.Count - i - 1));
							MazeAStar.EdgeMoveType moveType = MazeAStar.GetEdgeMoveType(currentBestPath[currentBestPath.Count - i - 1]);
							switch (moveType) {
							case MazeAStar.EdgeMoveType.Walk: follower.AddWaypoint(pos, false); break;
							case MazeAStar.EdgeMoveType.Fall: follower.AddWaypoint(pos, false, 0, true); break;
							case MazeAStar.EdgeMoveType.Jump: follower.AddWaypoint(pos, false, characterMover.jump.fullPressDuration); break;
							}
						}
						follower.SetCurrentTarget(pos);
						follower.UpdateLine();
						follower.doPrediction = true;
					} else {
						if (!characterMover.IsAutoMoving() && follower.waypoints.Count==0) {
							mapAstar.Start(here, here);
							//Debug.Log("startover new level?");
						}
					}
				}
			}
			break;
		}
	}
	int indexOnBestPath = -1;
	//List<Coord> currentBestPath;
	List<int> currentBestPath;
}
