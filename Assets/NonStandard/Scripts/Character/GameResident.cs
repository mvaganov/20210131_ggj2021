using System.Collections.Generic;
using UnityEngine;

public class GameResident : MonoBehaviour {
    [Tooltip("if true, set bind point as soon as object Starts")]
    public bool bindOnStart = true;
    public Vector3 homePoint;
    public List<GameArea> gameAreaInhabited = new List<GameArea>();
    public void BindHomePoint(Vector3 position) { homePoint = position; bindOnStart = false; }
    public void BindHomePointHere() { homePoint = transform.position; bindOnStart = false; }
    public void ReturnToHome(bool clearVelocity = true) {
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
		if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        transform.position = homePoint;
    }
    void Start() {
        if (bindOnStart) { BindHomePointHere(); }
    }
	private void OnTriggerEnter(Collider other) {
        GameArea binder = other.GetComponent<GameArea>();
		if (binder && gameAreaInhabited.IndexOf(binder) < 0) { gameAreaInhabited.Add(binder); }
	}
	private void OnTriggerExit(Collider other) {
        GameArea binder = other.GetComponent<GameArea>();
        if (binder) {
            gameAreaInhabited.Remove(binder);
			if (gameAreaInhabited.Count == 0) { ReturnToHome(); }
        }
    }
}
