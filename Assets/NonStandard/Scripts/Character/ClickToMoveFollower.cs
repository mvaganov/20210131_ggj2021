using NonStandard;
using NonStandard.Character;
using NonStandard.GameUi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NonStandard.Lines;

public class ClickToMoveFollower : MonoBehaviour {
	public List<Interact3dItem> waypoints = new List<Interact3dItem>();
	public Interact3dItem currentWaypoint;
	internal Wire line;
	Vector3 targetPosition;
	Vector3[] positionSample = new Vector3[10];
	int sampleIndex, samplesGroupsRead;
	public CharacterMove mover;
	public ClickToMove clickToMoveUi;
	float characterHeight = 0, characterRadius = 0;

	private void Start() {
		Init(gameObject);
	}
	public void Init(GameObject go) {
		if(mover != null) { return; }
		mover = GetComponent<CharacterMove>();
		CapsuleCollider cap = mover.GetComponent<CapsuleCollider>();
		characterHeight = cap.height / 2;
		characterRadius = cap.radius;
		if (line == null) { line = Lines.MakeWire(); }
		line.Line(Vector3.zero);
	}
	public void UpdateLine() {
		List<Vector3> points = new List<Vector3>();
		points.Add(mover.transform.position);
		for(int i = 0; i < waypoints.Count; ++i) {
			points.Add(waypoints[i].transform.position);
		}
		if(waypoints.Count == 0 || (waypoints[waypoints.Count-1].transform.position != currentWaypoint.transform.position)) {
			points.Add(targetPosition);
		}
		line.Line(points, Color.red, Lines.End.Arrow);
	}
	float ManhattanDistance(Vector3 a, Vector3 b) {
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
	}
	public float manhattanDistance;
	public bool IsStuck(Vector3 currentPosition) {
		positionSample[sampleIndex] = currentPosition;
		if (++sampleIndex >= positionSample.Length) { sampleIndex = 0; ++samplesGroupsRead; }
		manhattanDistance = 0;
		for (int i = 0; i < positionSample.Length; ++i) {
			manhattanDistance += ManhattanDistance(currentPosition, positionSample[i]);
		}
		return (samplesGroupsRead > 0 && manhattanDistance < 0.5f);
	}
	public void FixedUpdate() {
		if (mover.IsAutoMoving()) {
			if (IsStuck(mover.transform.position)) { NotifyEndPointReached(); }
		} else {
			if (waypoints.Count > 0) {
				NotifyEndPointReached();
			}
		}
	}
	void NotifyEndPointReached() {
		mover.DisableAutoMove();
		//line.Line(Vector3.zero, Vector3.zero);
		if (waypoints.Count > 0) {
			Interact3dItem wpObj = waypoints[0];
			waypoints.RemoveAt(0);
			Destroy(wpObj.gameObject);
			if (waypoints.Count > 0) {
				wpObj = waypoints[0];
			} else if (currentWaypoint != null) {
				wpObj = currentWaypoint;
			}
			mover.SetAutoMovePosition(wpObj.transform.position, NotifyEndPointReached, 0);
		} else {
			if (currentWaypoint != null && currentWaypoint.showing) { currentWaypoint.showing = false; }
		}
	}

	public void SetCurrentTarget(Vector3 position, Vector3 normal) {
		targetPosition = position;
		if (Vector3.Dot(position, Vector3.up) > 0.5f) {
			targetPosition += characterHeight * Vector3.up;
		} else {
			targetPosition += characterRadius * normal;
		}
		samplesGroupsRead = 0;
		if (waypoints.Count == 0) {
			mover.SetAutoMovePosition(targetPosition, NotifyEndPointReached, 0);
			//line.Arrow(mover.transform.position, targetPosition, Color.red);
		} else {
			//line.Arrow(waypoints[waypoints.Count - 1].transform.position, targetPosition, Color.red);
		}
		if (currentWaypoint != null) {
			currentWaypoint.transform.position = targetPosition;
			Interact3dUi.Instance.UpdateItem(currentWaypoint);
			currentWaypoint.showing = false; // hide the waypoint button during drag
		}
	}

	public void ShowCurrentWaypoint() {
		if (currentWaypoint == null) {
			currentWaypoint = Instantiate(clickToMoveUi.prefab_waypoint.gameObject).GetComponent<Interact3dItem>();
			currentWaypoint.OnInteract = AddWaypoint;
			//Debug.Log("waypoint made " + targetPosition);
		}
		bool showIt = mover.IsAutoMoving() && (waypoints.Count == 0 ||
			waypoints[waypoints.Count - 1].transform.position != currentWaypoint.transform.position);
		if (showIt) {
			currentWaypoint.showing = true;
			currentWaypoint.transform.position = targetPosition;
			Interact3dUi.Instance.UpdateItem(currentWaypoint);
		}
	}
	public void AddWaypoint() {
		Interact3dItem newWayPoint = Instantiate(clickToMoveUi.prefab_middleWaypoint.gameObject).GetComponent<Interact3dItem>();
		newWayPoint.transform.position = currentWaypoint.transform.position;
		newWayPoint.OnInteract = CancelWaypoints;// ()=>RemoveWaypoint(newWayPoint);
		newWayPoint.gameObject.SetActive(true);
		waypoints.Add(newWayPoint);
		currentWaypoint.showing = false;
	}
	public void CancelWaypoints() {
		for(int i = 0; i < waypoints.Count; ++i) {
			Destroy(waypoints[i].gameObject);
		}
		waypoints.Clear();
		NotifyEndPointReached();
	}
	public void RemoveWaypoint(Interact3dItem wp) {
		int i = waypoints.IndexOf(wp);
		if(i >= 0) { waypoints.RemoveAt(i); }
		Destroy(wp.gameObject);
	}
}
