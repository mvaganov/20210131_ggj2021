using NonStandard;
using NonStandard.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NonStandard.Lines;

public class Discovery : MonoBehaviour
{
    public Color discoveredWall = Color.white, discoveredFloor = Color.gray;
    public MazeLevel maze;

    List<MazeTile> pending = new List<MazeTile>();
    private static int playerLayer = -1;
    private SphereCollider sc;
	private void Start() {
		if(maze == null) { maze = FindObjectOfType<MazeLevel>(); }
        CharacterMove cm = transform.parent.GetComponent<CharacterMove>();
        sc = GetComponent<SphereCollider>();
        if (cm != null) {
            cm.callbacks.collisionStart.AddListener(v => Blink());
		}
        if(playerLayer == -1) { LayerMask.NameToLayer("player"); }
	}

    public void Blink() {
        gameObject.SetActive(false);
        pending.Clear();
        Clock.setTimeout(() => gameObject.SetActive(true), 0);
	}

	private void OnTriggerEnter(Collider other) {
        MazeTile mt = other.GetComponent<MazeTile>();
		if (mt) {
			if (!mt.discovered) {
                if(!VisTest(mt, -1)) {
                    pending.Add(mt);
                }
            }
		}
	}
    private bool VisTest(MazeTile mt, int i) {
        if (maze.GetTileSrc(mt.coord) == '#') {
            int neighborsWhoAreUp = maze.CountDiscoveredNeighborWalls(mt.coord);
            if (neighborsWhoAreUp >= 2) { mt.SetDiscovered(true, this, maze); return true; }
        }
        Vector3 p = transform.parent.position;
        Vector3 t = mt.CalcVisibilityTarget();
        Vector2 c = Random.insideUnitCircle;
        t.x += c.x * maze.tileSize.x / 2;
        t.z += c.y * maze.tileSize.z / 2;
        Vector3 delta = t - p;
        float dist = delta.magnitude;
        if(dist < maze.tileSize.x) {
            mt.SetDiscovered(true, this, maze);
            return true;
        }
        if(dist > sc.radius + maze.tileSize.x) { Blink(); return true; } // remove from list without resolving visibility
        Ray r = new Ray(p, delta/dist);
        Physics.Raycast(r, out RaycastHit rh, ~playerLayer);
        if (i >= 0 && wires != null) {
            while (wires.Count <= i) {
                wires.Add(Lines.MakeWire().Line(Vector3.zero));
            }
            wires[i].Line(p, t, discoveredWall);
        }
        //Lines.Mak
        if (rh.transform == mt.transform) {
            mt.SetDiscovered(true, this, maze);
            return true;
        }
        return false;
    }
    List<Wire> wires = null;// new List<Wire>();
    private void FixedUpdate() {
        for(int i = pending.Count-1; i >= 0; --i) {
            if(VisTest(pending[i], i)) {
                if (pending.Count > i) {
                    pending.Remove(pending[i]);
                } else { break; }
            }
        }
	}
}
