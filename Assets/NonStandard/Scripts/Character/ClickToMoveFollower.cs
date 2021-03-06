using NonStandard;
using NonStandard.Character;
using NonStandard.GameUi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NonStandard.Lines;

public class ClickToMoveFollower : MonoBehaviour {
	[System.Serializable] public struct Waypoint {
		public Vector3 p;
		public Interact3dItem ui;
		public Waypoint(Interact3dItem ui) {
			p = ui.transform.position;
			this.ui = ui;
		}
		public Waypoint(Vector3 p) { ui = null; this.p = p; }
	}
	public List<Waypoint> waypoints = new List<Waypoint>();
	public Interact3dItem currentWaypoint;
	internal Wire line;
	Vector3 targetPosition;
	Vector3[] shortTermPositionHistory = new Vector3[10];
	int historyIndex, historyDuringThisMove;
	public CharacterMove mover;
	public ClickToMove clickToMoveUi;
	float characterHeight = 0, characterRadius = 0;
	public Color color = Color.white;
	public Action<Vector3> onTargetSet;

	public float CharacterHeight => characterHeight;
	public static List<ClickToMoveFollower> allFollowers = new List<ClickToMoveFollower>();

	private void Awake() {
		allFollowers.Add(this);
	}

	private void Start() {
		Init(gameObject);
	}
	public void Init(GameObject go) {
		if(mover != null) { return; }
		mover = GetComponent<CharacterMove>();
		CapsuleCollider cap = mover.GetComponent<CapsuleCollider>();
		if (cap != null) {
			characterHeight = cap.height / 2;
			characterRadius = cap.radius;
		} else {
			characterHeight = characterRadius = 0;
		}
		if (line == null) { line = Lines.MakeWire(); }
		line.Line(Vector3.zero);
	}
	public void HidePath() { ShowPath(false); }
	public void ShowPath(bool show=true) {
		line.gameObject.SetActive(show);
		waypoints.ForEach(w => w.ui?.gameObject.SetActive(show));
	}
	public void UpdateLine() {
		List<Vector3> points = new List<Vector3>();
		points.Add(mover.transform.position);
		for(int i = 0; i < waypoints.Count; ++i) {
			points.Add(waypoints[i].p);
		}
		if(waypoints.Count == 0 || (waypoints[waypoints.Count-1].p != targetPosition)) {
			points.Add(targetPosition);
		}
		line.Line(points, color, Lines.End.Arrow);
		line.gameObject.SetActive(true);
	}
	float ManhattanDistance(Vector3 a, Vector3 b) {
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
	}
	//public float manhattanDistance;
	public bool IsStuck(Vector3 currentPosition) {
		shortTermPositionHistory[historyIndex] = currentPosition;
		if (++historyIndex >= shortTermPositionHistory.Length) { historyIndex = 0; ++historyDuringThisMove; }
		float manhattanDistance = 0;
		for (int i = 0; i < shortTermPositionHistory.Length; ++i) {
			manhattanDistance += ManhattanDistance(currentPosition, shortTermPositionHistory[i]);
		}
		return (historyDuringThisMove > 0 && manhattanDistance < 0.5f);
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
			Interact3dItem wpObj = waypoints[0].ui;
			waypoints.RemoveAt(0);
			if(wpObj) Destroy(wpObj.gameObject);
			Vector3 p = targetPosition;
			if (waypoints.Count > 0) {
				p = waypoints[0].p;
			} else if (currentWaypoint != null) {
				p = currentWaypoint.transform.position;
			}
			mover.SetAutoMovePosition(p, NotifyEndPointReached, 0);
		} else {
			if (currentWaypoint != null && currentWaypoint.showing) { currentWaypoint.showing = false; }
			ShowPath(false);
		}
	}
	public void SetCurrentTarget(Vector3 position) { SetCurrentTarget(position, Vector3.zero); }
	public void SetCurrentTarget(Vector3 position, Vector3 normal) {
		targetPosition = position;
		onTargetSet?.Invoke(position);
		if (normal != Vector3.zero) {
			if (Vector3.Dot(normal, Vector3.up) > 0.5f) {
				targetPosition += characterHeight * Vector3.up;
			} else {
				targetPosition += characterRadius * normal;
			}
		}
		historyDuringThisMove = 0;
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
			currentWaypoint.OnInteract = AddWaypointHere;
			//Debug.Log("waypoint made " + targetPosition);
		}
		bool showIt = mover.IsAutoMoving() && (waypoints.Count == 0 ||
			waypoints[waypoints.Count - 1].p != currentWaypoint.transform.position);
		if (showIt) {
			currentWaypoint.showing = true;
			currentWaypoint.transform.position = targetPosition;
			Interact3dUi.Instance.UpdateItem(currentWaypoint);
		}
	}
	public void AddWaypointHere() {
		AddWaypoint(currentWaypoint.transform.position, true);
	}
	public void AddWaypoint(Vector3 position, bool includeUiElement) {
		if (includeUiElement) {
			Interact3dItem newWayPoint = Instantiate(clickToMoveUi.prefab_middleWaypoint.gameObject).GetComponent<Interact3dItem>();
			newWayPoint.transform.position = position;
			newWayPoint.OnInteract = ClearWaypoints;// ()=>RemoveWaypoint(newWayPoint);
			newWayPoint.gameObject.SetActive(true);
			waypoints.Add(new Waypoint(newWayPoint));
		} else {
			waypoints.Add(new Waypoint(position));
		}
		if(currentWaypoint != null) currentWaypoint.showing = false;
	}
	public void ClearWaypoints() {
		for(int i = 0; i < waypoints.Count; ++i) {
			if (waypoints[i].ui) { Destroy(waypoints[i].ui.gameObject); }
		}
		waypoints.Clear();
		NotifyEndPointReached();
		ShowPath(false);
	}
	public void RemoveWaypoint(Interact3dItem wp) {
		int i = waypoints.FindIndex(w=>w.ui==wp);
		if(i >= 0) { waypoints.RemoveAt(i); }
		Destroy(wp.gameObject);
	}
}
