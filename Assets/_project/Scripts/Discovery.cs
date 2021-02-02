using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Discovery : MonoBehaviour
{
    public Color undiscoveredWall = Color.black, undiscoveredFloor = Color.black,
    discoveredWall = Color.white, discoveredFloor = Color.gray;
	public float undiscoveredHeight = -1;
    public float floorHeight = 0;
    public float wallHeight = 4;
    public float discoveredTime = 30;
    public float animationTime = 1;
    public Vector3 tileSize = Vector3.one*4;

    List<MazeTile> pending = new List<MazeTile>();
	private void OnTriggerEnter(Collider other) {
        MazeTile mt = other.GetComponent<MazeTile>();
		if (mt) {
			if (!mt.discovered) {
                if(!VisTest(mt)) {
                    pending.Add(mt);
                }
            }
		}
	}
    private bool VisTest(MazeTile mt) {
        Vector3 p = transform.parent.position;
        Vector3 t = mt.CalcVisibilityTarget(this);
        Vector3 delta = t - p;
        float dist = delta.magnitude;
        if(dist < 5) {
            mt.SetDiscovered(true, this);
            return true;
        }
        Ray r = new Ray(p, delta/dist);
        Physics.Raycast(r, out RaycastHit rh);
        if (rh.transform == mt.transform) {
            mt.SetDiscovered(true, this);
            return true;
        }
        return false;
    }
    private void FixedUpdate() {
        for(int i = pending.Count-1; i >= 0; --i) {
            if(VisTest(pending[i])) {
                pending.Remove(pending[i]);
            }
        }
	}
}
