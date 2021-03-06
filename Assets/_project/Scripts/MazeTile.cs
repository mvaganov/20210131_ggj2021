using MazeGeneration;
using NonStandard;
using UnityEngine;

public class MazeTile : MonoBehaviour
{
	public enum Kind { None, Wall, Floor, RampNorth, RampSouth, RampEast, RampWest }
	public Kind kind = Kind.None;
	public Coord coord;
	public bool _discovered = false;
	public float goalScore;
	public MazeLevel maze;
	public bool discovered => _discovered;
	Transform t;
	public void CalcLocal(out Vector3 pos, out Vector3 scale, out Quaternion rot, out Color color, Discovery d) {
		float h = 0;
		Vector3 r = Vector3.zero;
		scale = Vector3.one;
		color = maze.undiscoveredFloor;
		if (_discovered == false) {
			h = maze.undiscoveredHeight;
		} else switch (kind) {
			case Kind.Floor: h = maze.floorHeight; color = d.discoveredFloor; break;
			case Kind.Wall: h = maze.wallHeight; color = d.discoveredWall; break;
			case Kind.RampEast: h = maze.rampHeight; r.z = maze.rampAngle; 
				scale.x = maze.rampScale; scale.z = 63f / 64; color = d.discoveredRamp; break;
			case Kind.RampWest: h = maze.rampHeight; r.z = -maze.rampAngle; 
				scale.x = maze.rampScale; scale.z = 63f / 64; color = d.discoveredRamp; break;
			case Kind.RampNorth: h = maze.rampHeight; r.x = maze.rampAngle; 
				scale.z = maze.rampScale; scale.x = 63f / 64; color = d.discoveredRamp; break;
			case Kind.RampSouth: h = maze.rampHeight; r.x = -maze.rampAngle; 
				scale.z = maze.rampScale; scale.x = 63f / 64; color = d.discoveredRamp; break;
		}
		rot = Quaternion.Euler(r);
		pos = new Vector3(coord.X * maze.tileSize.x, h * maze.tileSize.y, coord.Y * maze.tileSize.z);
		Vector3 s = discovered ? maze.tileSize : maze.undiscoveredTileSize;
		scale.Scale(s);
	}
	public Vector3 CalcLocalPosition() {
		float h = 0;
		if(_discovered == false) {
			h = maze.undiscoveredHeight;
		} else switch (kind) { 
		case Kind.Floor: h = maze.floorHeight; break;
		case Kind.Wall: h = maze.wallHeight; break;
		case Kind.RampEast: case Kind.RampWest: case Kind.RampSouth: case Kind.RampNorth: 
			h = maze.rampHeight; break;
		}
		Vector3 p = maze.GetLocalPosition(coord);
		p.y += h * maze.tileSize.y;
		return p;
	}
	public Vector3 CalcVisibilityTarget() {
		Vector3 s = discovered ? maze.tileSize : maze.undiscoveredTileSize;
		return CalcLocalPosition() + Vector3.up * (s.y / 2) + t.parent.position;
	}
	public Color CalcColor() {
		switch (kind) {
		case Kind.Floor: return _discovered ? d.discoveredFloor : maze.undiscoveredFloor;
		case Kind.Wall: return _discovered ? d.discoveredWall : maze.undiscoveredWall;
		case Kind.RampEast: case Kind.RampWest: case Kind.RampNorth: case Kind.RampSouth:
			return _discovered ? d.discoveredWall : maze.undiscoveredWall;
		}
		return Color.magenta;
	}
	public void SetDiscovered(bool discovered, Discovery d, MazeLevel maze) {
		this.maze = maze;
		maze.seen[coord] = discovered;
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
		startScale = t.localScale;
		startColor = r.material.color;
		duration = (long)(maze.animationTime * 1000);
		startRot = t.rotation;
		CalcLocal(out endPos, out endScale, out endRot, out endColor, d);
		Animate();
	}
	Discovery d;
	Renderer r;
	long started;
	Vector3 startPos, endPos, startScale, endScale;
	Quaternion startRot, endRot;
	Color startColor, endColor;
	long duration;
	void Animate() {
		float p = duration > 0 ? (float)(Clock.Now - started) / duration : 1;
		Color c;
		if (p >= 1) {
			t.localPosition = endPos;
			c = endColor;
			r.material.color = c;
			t.rotation = endRot;
			t.localScale = endScale;
		} else {
			t.localPosition = Vector3.Lerp(startPos, endPos, p);
			t.localScale = Vector3.Lerp(startScale, endScale, p);
			t.rotation = Quaternion.Lerp(startRot, endRot, p);
			c = Color.Lerp(startColor, endColor, p);
			r.material.color = c;
			Clock.setTimeout(Animate, 10+Random.Range(0,20));
		}
		if (r.enabled) { if (c.a == 0) { r.enabled = false; } }
		else { if(c.a != 0) { r.enabled = true; } }
	}
}
