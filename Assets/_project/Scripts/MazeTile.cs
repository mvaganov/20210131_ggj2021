using MazeGeneration;
using NonStandard;
using UnityEngine;

public class MazeTile : MonoBehaviour
{
	public enum Kind { None, Wall, Floor }
	public Kind kind = Kind.None;
	public Coord coord;
	public bool _discovered = false;
	public bool discovered => _discovered;
	Transform t;
	public Vector3 CalcLocalPosition(Discovery d) {
		float h = 0;
		if(_discovered == false) {
			h = d.undiscoveredHeight;
		} else switch (kind) { 
		case Kind.Floor: h = d.floorHeight; break;
		case Kind.Wall: h = d.wallHeight; break;
		}
		return new Vector3(coord.X * d.tileSize.x, h* d.tileSize.y, coord.Y * d.tileSize.z);
	}
	public Vector3 CalcVisibilityTarget(Discovery d) {
		return CalcLocalPosition(d) + Vector3.up * (d.tileSize.y / 2) + t.parent.position;
	}
	public Color CalcColor(Discovery d) {
		switch (kind) {
		case Kind.Floor: return _discovered?d.discoveredFloor:d.undiscoveredFloor;
		case Kind.Wall: return _discovered ? d.discoveredWall : d.undiscoveredWall;
		}
		return Color.magenta;
	}
	public void SetDiscovered(bool discovered, Discovery d) {
		_discovered = discovered;
		t = transform;
		DoAnimate(d);
	}

	public void DoAnimate(Discovery d) {
		Clock.unsetTimeout(Animate);
		this.d = d;
		r = GetComponent<Renderer>();
		started = Clock.Now;
		startPos = t.localPosition;
		endPos = CalcLocalPosition(d);
		startColor = r.material.color;
		endColor = CalcColor(d);
		duration = (long)(d.animationTime * 1000);
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
		float p = (float)(Clock.Now - started) / duration;
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
