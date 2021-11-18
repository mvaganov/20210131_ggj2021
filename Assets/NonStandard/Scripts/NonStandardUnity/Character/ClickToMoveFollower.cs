using NonStandard.Extension;
using NonStandard.GameUi;
using System;
using System.Collections.Generic;
using UnityEngine;
using static NonStandard.Lines;

namespace NonStandard.Character {
	public class ClickToMoveFollower : MonoBehaviour {
		[System.Serializable]
		public class Waypoint {
			public enum Act { None, Move, Jump, Fall }
			public Act act;
			public Vector3 positon;
			public float value = 0;
			/// <summary>optionally change the speed of motion (lowered speed)</summary>
			public float speed = 1;
			/// <summary>optionally do this action for a certain time limit. if zero, there are no time limits.</summary>
			public float timeLimit;
			public Interact3dItem ui;
			public Waypoint(Interact3dItem _ui, Act a = Act.Move, float v = 0) {
				positon = _ui.transform.position; ui = _ui; act = a; value = v;
			}
			public Waypoint(Vector3 p, Act a = Act.Move, float v = 0) { ui = null; positon = p; act = a; value = v; }
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

		public float CharacterRadius => characterRadius;
		public float CharacterHeight => characterHeight;
		public static List<ClickToMoveFollower> allFollowers = new List<ClickToMoveFollower>();

		private void Awake() {
			allFollowers.Add(this);
		}

		private void Start() {
			Init(gameObject);
		}
		public void Init(GameObject go) {
			if (mover != null) { return; }
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
		public void ShowPath(bool show = true) {
			line.gameObject.SetActive(show);
			waypoints.ForEach(w => { if (w.ui != null) w.ui.gameObject.SetActive(show); });
		}
		public void UpdateLine() {
			List<Vector3> points = new List<Vector3>();
			points.Add(mover.transform.position);
			Vector3 here;
			for (int i = 0; i < waypoints.Count; ++i) {
				here = waypoints[i].positon;
				switch (waypoints[i].act) {
				case Waypoint.Act.Move: points.Add(here); break;
				case Waypoint.Act.Fall:
				case Waypoint.Act.Jump: {
					points.Add(here);
					Vector3 nextPosition = (i < waypoints.Count) ? waypoints[i].positon : targetPosition;
					Vector3 delta = (nextPosition - here);
					if (delta == Vector3.zero) break;
					float dist = delta.magnitude;
					float jumpMove = Mathf.Min(1, dist / 2);
					Vector3 dir = delta / dist;
					points.Add(here + Vector3.up * 4 * waypoints[i].value + dir * jumpMove);
					if (waypoints[i].act == Waypoint.Act.Jump) {
						points.Add(here + dir * 2);
					}
				}
				break;
				}
			}
			if (waypoints.Count == 0 || (waypoints[waypoints.Count - 1].positon != targetPosition)) {
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
				if (IsStuck(mover.transform.position)) { NotifyWayPointReached(); }
			} else {
				if (waypoints.Count > 0) {
					NotifyWayPointReached();
				}
			}
		}

		public bool doPrediction = false;
		public float predictionSeconds;
		public float predictionProgressOnWaypoint;
		public int predictionWaypointIndex = -1;
		public Vector3 predictionPos;
		const float timeJump = 1f / 16;
		const int maxPredictionsPerCycle = 16;
		public List<Vector3> predictionPath = new List<Vector3>();
		public Wire predictionLine, startArrow, endArrow;

		void PredictionPathAdd(Vector3 p) {
			if (predictionPath.Count == 0) { predictionPath.Add(p); return; }
			Vector3 last = predictionPath[predictionPath.Count - 1];
			if (p.DistanceManhattan(last) > 1f / 1024f) { predictionPath.Add(p); }
		}

		// the Update method does fine-grained prediction, unlike FixedUpdate, which does more standard timed game logic
		public void Update() {
			//return; // TODO work on this code.
			// if the path is known, walk down path ahead of time with collision detection to get a more precise prediction
			if (waypoints.Count == 0) { doPrediction = false; }
			if (!doPrediction) return;
			// if the prediction is starting, initialize prediction state
			if (predictionWaypointIndex < 0) {
				predictionProgressOnWaypoint = 0;
				predictionSeconds = 0;
				predictionWaypointIndex = 0;
				predictionPos = waypoints[0].positon;
				//Debug.Log("start"+predictionPos);
				predictionPath.Clear();
				predictionPath.Add(predictionPos);
				if (predictionLine != null) { predictionLine.Line(Vector3.zero); }
				if (startArrow == null) { startArrow = Lines.MakeWire("start"); }
				startArrow.Arrow(predictionPos + Vector3.up, predictionPos, Color.red, .25f);
			}
			int predictionsMadeThisTime = 0;
			float speed = mover.move.speed;
			float move = speed * timeJump;
			Vector3 capT, capB;
			float rad;
			float gForce = Mathf.Abs(Physics.gravity.y);
			mover.GetLocalCapsule(out capT, out capB, out rad);
			do {
				if (predictionWaypointIndex + 1 >= waypoints.Count) { doPrediction = false; break; }
				Vector3 next = waypoints[predictionWaypointIndex + 1].positon;
				if (waypoints[predictionWaypointIndex].act != Waypoint.Act.Move) {
					//Debug.Log("not move?");
					break;
				}
				Vector3 delta = next - predictionPos;
				float dist = delta.magnitude;
				Vector3 dir = delta / dist;
				//Vector3 expectedAfterTimeJump = predictionPos + dir * move;
				//float timeCollisionOccuredAt = 0;
				delta.y = 0;
				dist = delta.magnitude;
				while (predictionWaypointIndex < waypoints.Count - 1) {
					if (Physics.CapsuleCast(capT + predictionPos, capB + predictionPos, rad, dir, out RaycastHit hitInfo, move)) {
						if (hitInfo.distance > 0) {
							float timePassed = hitInfo.distance / speed;
							predictionSeconds += timePassed;
							predictionPos += dir * timePassed;
							PredictionPathAdd(predictionPos);
						}
						Vector3 r = Vector3.Cross(hitInfo.normal, dir).normalized;
						dir = Vector3.Cross(r, dir);
						float shorterMove = move - hitInfo.distance;
						if (Physics.CapsuleCast(capT + predictionPos, capB + predictionPos, rad, dir, out hitInfo, shorterMove)) {
							if (hitInfo.distance > 0) {
								float timePassed = hitInfo.distance / speed;
								predictionSeconds += timePassed;
								predictionPos += dir * timePassed;
								PredictionPathAdd(predictionPos);
							} else {
								Debug.Log("blocked!");
								doPrediction = false;
							}
						} else {
							float timePassed = shorterMove / speed;
							PredictionMove_ThenToGround(dir, shorterMove, timePassed, rad, gForce);
						}
						doPrediction = false; // till the collision logic can be tested, stop after the first collision
					} else {
						PredictionMove_ThenToGround(dir, move, timeJump, rad, gForce);
					}
					delta = next - predictionPos;
					delta.y = 0;
					float nextDist = delta.magnitude;
					if (nextDist > dist) {
						++predictionWaypointIndex;
						break;
					} else {
						dist = nextDist;
					}
					if (++predictionsMadeThisTime >= maxPredictionsPerCycle) { break; }
				}
				if (predictionWaypointIndex >= waypoints.Count - 1) { doPrediction = false; ; }
			} while (predictionsMadeThisTime < maxPredictionsPerCycle);
			if (predictionLine == null) { predictionLine = Lines.MakeWire("prediction"); }
			if (!doPrediction) { predictionWaypointIndex = -1; }
			predictionLine.Line(predictionPath, Color.magenta, End.Normal, 0.25f);
			if (endArrow == null) { endArrow = Lines.MakeWire("end"); }
			Vector3 endP = predictionPath[predictionPath.Count - 1];
			endArrow.Arrow(endP, endP + Vector3.up, Color.yellow, .25f);
		}
		public void PredictionMove_ThenToGround(Vector3 dir, float distToMove, float timeToFall, float rad, float gForce) {
			predictionPos += dir * distToMove;
			float fallMove = timeToFall * timeToFall * gForce;
			if (Physics.SphereCast(predictionPos, rad, Vector3.down, out RaycastHit hitInfo, fallMove)) {
				predictionPos += Vector3.down * hitInfo.distance;
				timeToFall = Mathf.Sqrt(hitInfo.distance / gForce);
			} else {
				predictionPos += Vector3.down * fallMove;
			}
			PredictionPathAdd(predictionPos);
			predictionSeconds += timeToFall;
		}
		public void NotifyWayPointReached() {
			mover.DisableAutoMove();
			//line.Line(Vector3.zero, Vector3.zero);
			if (waypoints.Count > 0) {
				Interact3dItem wpObj = waypoints[0].ui;
				waypoints.RemoveAt(0);
				if (wpObj) Destroy(wpObj.gameObject);
				Vector3 p = targetPosition;
				float jumpPress = 0;
				if (waypoints.Count > 0) {
					if (waypoints[0].act == Waypoint.Act.Jump) { jumpPress = waypoints[0].value; }
					p = waypoints[0].positon;
				} else if (currentWaypoint != null) {
					p = currentWaypoint.transform.position;
				}
				if (jumpPress > 0) {
					mover.JumpButtonTimed = jumpPress;
					Vector3 delta = p - transform.position;
					// calculate the distance needed to jump to, both vertically and horizontally
					float vDist = delta.y;
					delta.y = 0;
					float hDist = delta.magnitude;
					// estimate max jump distance TODO work out this math...
					float height = mover.jump.max;
					float v = Mathf.Sqrt(height * 2);

					float jDist = mover.MoveSpeed * height;
					float distExtra = jDist - hDist;
					long howLongToWaitInAir = (long)(distExtra * 1000 / jDist);
					GameClock.Delay(howLongToWaitInAir, () => mover.SetAutoMovePosition(p, NotifyWayPointReached, 0));
				} else {
					mover.SetAutoMovePosition(p, NotifyWayPointReached, 0);
				}
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
				mover.SetAutoMovePosition(targetPosition, NotifyWayPointReached, 0);
				//line.Arrow(mover.transform.position, targetPosition, Color.red);
			} else {
				//line.Arrow(waypoints[waypoints.Count - 1].transform.position, targetPosition, Color.red);
			}
			if (currentWaypoint != null) {
				currentWaypoint.transform.position = targetPosition;
				Interact3dUi ui3d = Interact3dUi.Instance; if (ui3d != null) { ui3d.UpdateItem(currentWaypoint); }
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
				waypoints[waypoints.Count - 1].positon != currentWaypoint.transform.position);
			if (showIt) {
				currentWaypoint.showing = true;
				currentWaypoint.transform.position = targetPosition;
				Interact3dUi ui3d = Interact3dUi.Instance; if (ui3d != null) { ui3d.UpdateItem(currentWaypoint); }
			}
		}
		public void AddWaypointHere() {
			AddWaypoint(currentWaypoint.transform.position, true);
		}
		public void AddWaypoint(Vector3 position, bool includeUiElement, float jumpValue = 0, bool fall = false) {
			Waypoint.Act act = Waypoint.Act.Move;
			if (jumpValue > 0) { act = Waypoint.Act.Jump; } else if (fall) { act = Waypoint.Act.Fall; }
			if (includeUiElement) {
				Interact3dItem newWayPoint = Instantiate(clickToMoveUi.prefab_middleWaypoint.gameObject).GetComponent<Interact3dItem>();
				newWayPoint.transform.position = position;
				newWayPoint.OnInteract = ClearWaypoints;// ()=>RemoveWaypoint(newWayPoint);
				newWayPoint.gameObject.SetActive(true);
				waypoints.Add(new Waypoint(newWayPoint, act, jumpValue));
			} else {
				waypoints.Add(new Waypoint(position, act, jumpValue));
			}
			if (currentWaypoint != null) currentWaypoint.showing = false;
		}
		public void ClearWaypoints() {
			for (int i = 0; i < waypoints.Count; ++i) {
				if (waypoints[i].ui) { Destroy(waypoints[i].ui.gameObject); }
			}
			waypoints.Clear();
			NotifyWayPointReached();
			ShowPath(false);
		}
		public void RemoveWaypoint(Interact3dItem wp) {
			int i = waypoints.FindIndex(w => w.ui == wp);
			if (i >= 0) { waypoints.RemoveAt(i); }
			Destroy(wp.gameObject);
		}
	}
}