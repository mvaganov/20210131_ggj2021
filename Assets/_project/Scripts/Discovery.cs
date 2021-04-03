using NonStandard;
using NonStandard.Character;
using System.Collections.Generic;
using UnityEngine;
using static NonStandard.Lines;

public class Discovery : MonoBehaviour
{
    public Color discoveredWall = new Color(.75f, .75f, .75f), discoveredFloor = new Color(.25f,.25f,.25f), discoveredRamp = new Color(.5f,.5f,.5f);
    public MazeLevel maze;

    List<MazeTile> pending = new List<MazeTile>();
    private static int playerLayer = -1;
    private SphereCollider sc;
    public VisionMapping vision;

    static List<Discovery> allDiscovery = new List<Discovery>();

	private void Awake() {
        allDiscovery.Add(this);
	}

    public static void ResetAll() {
        //Show.Log("reset all");
        for(int i = allDiscovery.Count-1; i >= 0; --i) {
            if (allDiscovery[i] == null) { allDiscovery.RemoveAt(i); continue; }
            allDiscovery[i].ResetCalc();
		}
	}

	private void Start() {
		if(maze == null) { maze = FindObjectOfType<MazeLevel>(); }
        CharacterMove cm = transform.parent.GetComponent<CharacterMove>();
        if(cm == null) {
            Debug.Log(transform.HierarchyPath());
		}
        sc = GetComponent<SphereCollider>();
        cm.callbacks.jumped.AddListener(v => Blink());
        //cm.callbacks.stand.AddListener(v => Blink());
        cm.callbacks.fall.AddListener(() => Blink());
        if (playerLayer == -1) { LayerMask.NameToLayer("player"); }
        ResetCalc();
    }
	public void ResetCalc() {
        if (vision == null) {
            if (maze != null && maze.Map != null) {
                vision = new VisionMapping(()=>maze.Map.GetSize()           );
            }
        } else {
            vision.Reset();
        }
        Blink();
	}
	public void Blink() {
        //Debug.Log("blink");
        gameObject.SetActive(false);
        pending.Clear();
        Clock.setTimeout(() => gameObject.SetActive(true), 0);
	}
	private void OnTriggerEnter(Collider other) {
        //Debug.Log("triggered");
        MazeTile mt = other.GetComponent<MazeTile>();
		if (mt) {
			if (!mt.discovered || !vision[mt.coord]) {
                if(!VisTest(mt, -1)) {
                    pending.Add(mt);
                }
            }
		}
	}
    private bool VisTest(MazeTile mt, int i) {
        if (maze.GetTileSrc(mt.coord) == '#') {
            int neighborsWhoAreUp = maze.CountDiscoveredNeighborWalls(mt.coord);
            if (neighborsWhoAreUp >= 2) { 
                mt.SetDiscovered(true, this, maze);
                vision[mt.coord] = true;
                return true; }
        }
        Vector3 p = transform.position;
        Vector3 t = mt.transform.position;//CalcVisibilityTarget();
        Vector2 c = Random.insideUnitCircle;
        t.x += c.x * maze.tileSize.x / 2;
        t.z += c.y * maze.tileSize.z / 2;
        t.y -= 0.0625f;
        Vector3 delta = t - p;
        float dist = delta.magnitude;
        if(dist < maze.tileSize.x) {
            mt.SetDiscovered(true, this, maze);
            vision[mt.coord] = true;
            return true;
        }
        if(dist > sc.radius + maze.tileSize.x) { Blink(); return true; } // remove from list without resolving visibility
        Ray r = new Ray(p, delta/dist);
        bool blocked = Physics.Raycast(r, out RaycastHit rh);
        if (wires != null && i >= 0) {
            while (wires.Count <= i) { wires.Add(Lines.MakeWire().Line(Vector3.zero)); }
        }
        //Lines.Mak
        if (rh.transform == mt.transform) {
            if(wires != null && i >= 0) wires[i].Line(p, t, Color.green);
            mt.SetDiscovered(true, this, maze);
            vision[mt.coord] = true;
            return true;
        } else {
            if (wires != null && i >= 0) wires[i].Line(p, rh.point, Color.red);
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
