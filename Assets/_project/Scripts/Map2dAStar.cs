using MazeGeneration;
using NonStandard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map2dAStar : MazeAStar {
	MazeLevel maze;
	GameObject prefab_debug_astar;
	public Map2dAStar(MazeLevel maze, VisionMapping vision, Transform transform, GameObject prefab_debug_astar) {
		this.maze = maze;
		this.prefab_debug_astar = prefab_debug_astar;
		SetAstarSourceData(vision, () => maze.Map, ResetCalcSpace,
			(Coord c, out int e) => {
				AStarData a = calcSpace.At(c);
				Coord f = a.from; e = a._edge;
				return f != Coord.NegativeOne ? f : c;
			},
			(c, f, e) => {
				AStarData a = calcSpace.At(c);
				a._edge = e; a.from = f;
			},
			c => calcSpace.At(c).f, (c, f) => { calcSpace.At(c).f = f; },
			c => calcSpace.At(c).g, (c, f) => { calcSpace.At(c).g = f; });
		ResetCalcSpace(Map.GetSize());
		Vector3 p = transform.position;
		Coord here = maze.GetCoord(p);
		Start(here, here);
		if (prefab_debug_astar != null) {
			maze.seen.onChange += RefreshVision;
			vision.onChange += RefreshVision;
		}
	}
	public class AStarData {
		private float _f = -1, _g = -1; private Coord _from = Coord.NegativeOne; public int _edge = -1;
		public Coord coord;
		public GameObject debugVisibleObject;
		public Lines.Wire fromArrow;
		public Map2dAStar astar;
		public VisionMapping visMap;
		public AStarData(Coord c, Map2dAStar a, VisionMapping vm, GameObject prefab_data_vis) { Reset(c, a, vm, prefab_data_vis); }
		public void RefreshDebug() {
			if (!debugVisibleObject) { return; }
			Vector3 p = astar.maze.GetGroundPosition(coord), u = Vector3.up * .125f;
			debugVisibleObject.transform.position = p + u;
			bool isVisible = visMap[coord];
			string text = (_f < 0 && _g < 0)
				? coord.ToString()
				: $"{coord}\nf:{_f}\ng:{_g}\n{_edge}";
			UiText.SetText(debugVisibleObject, text);
			UiText.SetColor(debugVisibleObject, isVisible ? Color.white : Color.black);
		}
		void RefreshDebugFromArrow() {
			if (debugVisibleObject == null) return;
			Vector3 u = Vector3.up * .125f, start = u+astar.maze.GetGroundPosition(_from), end = u+astar.maze.GetGroundPosition(coord);
			if (fromArrow == null) {
				fromArrow = Lines.MakeWire(); fromArrow.gameObject.transform.SetParent(debugVisibleObject.transform);
			}
			EdgeMoveType mtype = GetMoveType(_edge);
			switch (mtype) {
			case EdgeMoveType.None:
				fromArrow.Arrow(start, end, Color.yellow);
				break;
			case EdgeMoveType.Walk:
				fromArrow.Arrow(start, end, Color.cyan);
				break;
			case EdgeMoveType.Fall:
				Vector3 startCp = end;
				startCp.y = start.y + 1;
				fromArrow.Bezier(start, startCp, end + Vector3.up, end, Color.blue);
				break;
			case EdgeMoveType.Jump:
				fromArrow.Bezier(start, start + Vector3.up, end + Vector3.up, end, Color.green);
				break;
			case EdgeMoveType.OOB:
				fromArrow.Arrow(start, end, Color.magenta);
				break;
			}
		}
		public AStarData Reset(Coord c, Map2dAStar a, VisionMapping vm, GameObject prefab_data_vis) {
			_f = _g = _edge = -1; _from = c;
			coord = c;
			astar = a;
			visMap = vm;
			if (debugVisibleObject == null && prefab_data_vis != null) {
				debugVisibleObject = GameObject.Instantiate(prefab_data_vis);
				RefreshDebug();
			}
			if (debugVisibleObject) {
				debugVisibleObject.name = prefab_data_vis.name + " (" + coord + ")";
				if (fromArrow != null) { RefreshDebugFromArrow(); }
			}
			return this;
		}
		public float f { get => _f; set { _f = value; RefreshDebug(); } }
		public float g { get => _g; set { _g = value; RefreshDebug(); } }
		public Coord from { get => _from; set { _from = value; RefreshDebugFromArrow(); } }
	}
	AStarData[,] calcSpace;
	void ResetCalcSpace(Coord size) {
		AStarData[,] newSpace = new AStarData[size.row, size.col];
		if (calcSpace != null) {
			Coord oldSize = calcSpace.GetSize();
			int count = Math.Min(oldSize.Area, size.Area);
			Coord fromOld = Coord.Zero, toNew = Coord.Zero;
			for (int i = 0; i < count; ++i) {
				newSpace.SetAt(toNew, calcSpace.At(fromOld).Reset(toNew, this, vision, prefab_debug_astar));
				toNew.Increment(size);
				fromOld.Increment(oldSize);
			}
		}
		vision?.Reset();
		size.ForEach(c => {
			if (newSpace.At(c) == null) { newSpace.SetAt(c, new AStarData(c, this, vision, prefab_debug_astar)); }
		});
		calcSpace = newSpace;
	}
	void ResetCalcSpace() {
		calcSpace?.GetSize().ForEach(c => calcSpace.At(c).Reset(c, this, vision, prefab_debug_astar));
	}
	void RefreshVision(Coord coord, bool visible) {
		Coord mapSize = maze.Map.GetSize();
		if (mapSize != calcSpace.GetSize()) { ResetCalcSpace(mapSize); }
		calcSpace.At(coord).RefreshDebug();
	}
	public void UpdateMapSize() {
		if (calcSpace == null || calcSpace.GetSize() != Map.GetSize()) {
			ResetCalcSpace(Map.GetSize());
		}
	}
}
