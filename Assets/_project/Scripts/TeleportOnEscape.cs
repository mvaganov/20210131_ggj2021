using NonStandard;
using UnityEngine;

public class TeleportOnEscape : MonoBehaviour {
    public Collider boundary;
    public Vector3 homePoint;
    public bool bindOnStart = true;
    public void BindHomePoint(Vector3 position) { homePoint = position; bindOnStart = false; }
    void Start() {
        if (boundary == null) {
            boundary = Global.Get<Collider>();
        }
        if (bindOnStart) { BindHomePoint(transform.position); }
    }
	private void OnTriggerExit(Collider other) { if(other == boundary) { transform.position = homePoint; } }
}
