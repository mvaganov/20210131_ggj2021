using System.Collections.Generic;
using UnityEngine;

public class GameBound : MonoBehaviour {
    public List<GameBinder> binders = new List<GameBinder>();
    public Vector3 homePoint;
    public bool bindOnStart = true;
    public void BindHomePoint(Vector3 position) { homePoint = position; bindOnStart = false; }
    public void ReturnToHome(bool clearVelocity = true) {
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
		if (rb) { rb.velocity = Vector3.zero;rb.angularVelocity = Vector3.zero; }
        transform.position = homePoint;
    }
    void Start() {
        if (bindOnStart) { BindHomePoint(transform.position); }
    }
	private void OnTriggerEnter(Collider other) {
        GameBinder binder = other.GetComponent<GameBinder>();
		if (binder && binders.IndexOf(binder) < 0) { binders.Add(binder); }
	}
	private void OnTriggerExit(Collider other) {
        GameBinder binder = other.GetComponent<GameBinder>();
        if (binder) {
            binders.Remove(binder);
			if (binders.Count == 0) { ReturnToHome(); }
        }
    }
}
