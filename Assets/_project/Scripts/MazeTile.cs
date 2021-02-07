using MazeGeneration;
using NonStandard;
using UnityEngine;

public class MazeTile : MonoBehaviour
{
	public enum Kind { None, Wall, Floor }
	public Kind kind = Kind.None;
	public Coord coord;
	public bool _discovered = false;
	public float goalScore;
	public MazeLevel maze;
	public bool discovered => _discovered;
	Transform t;
	public Vector3 CalcLocalPosition() {
		float h = 0;
		if(_discovered == false) {
			h = maze.undiscoveredHeight;
		} else switch (kind) { 
		case Kind.Floor: h = maze.floorHeight; break;
		case Kind.Wall: h = maze.wallHeight; break;
		}
		return new Vector3(coord.X * maze.tileSize.x, h* maze.tileSize.y, coord.Y * maze.tileSize.z);
	}
	public Vector3 CalcVisibilityTarget() {
		return CalcLocalPosition() + Vector3.up * (maze.tileSize.y / 2) + t.parent.position;
	}
	public Color CalcColor() {
		switch (kind) {
		case Kind.Floor: return _discovered ? d.discoveredFloor : maze.undiscoveredFloor;
		case Kind.Wall: return _discovered ? d.discoveredWall : maze.undiscoveredWall;
		}
		return Color.magenta;
	}
	public void SetDiscovered(bool discovered, Discovery d, MazeLevel maze) {
		this.maze = maze;
		this.d = d;
		_discovered = discovered;
		t = transform;
		DoAnimate();
	}

	public void DoAnimate() {
		Clock.unsetTimeout(Animate);
		r = GetComponent<Renderer>();
		started = Clock.Now;
		startPos = t.localPosition;
		endPos = CalcLocalPosition();
		startColor = r.material.color;
		endColor = CalcColor();
		duration = (long)(maze.animationTime * 1000);
		Animate();
	}
	Discovery d;
	Renderer r;
	long started;
	Vector3 startPos;
	Vector3 endPos;
	Color startColor;
	Color endColor;
	long duration;
	void Animate() {
		float p = duration > 0 ? (float)(Clock.Now - started) / duration : 1;
		if (p >= 1) {
			t.localPosition = endPos;
			r.material.color = endColor;
		} else {
			t.localPosition = Vector3.Lerp(startPos, endPos, p);
			r.material.color = Color.Lerp(startColor, endColor, p);
			Clock.setTimeout(Animate, 10+Random.Range(0,20));
		}
	}
}
