using NonStandard.GameUi;
using NonStandard.Ui;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NonStandard.Character {
	public class ClickToMove : MonoBehaviour {
		public CharacterMoveProxy characterToMove;
		public KeyCode key = KeyCode.Mouse0;
		public Camera _camera;
		public LayerMask validToMove = -1;
		public QueryTriggerInteraction moveToTrigger = QueryTriggerInteraction.Ignore;
		public Interact3dItem prefab_waypoint;
		public Interact3dItem prefab_middleWaypoint;
		public int mouseSetMove = 1, mouseSetUi = 0;

#if UNITY_EDITOR
		/// called when created by Unity Editor
		void Reset() {
			if (characterToMove == null) { characterToMove = transform.GetComponentInParent<CharacterMoveProxy>(); }
			if (characterToMove == null) { characterToMove = FindObjectOfType<CharacterMoveProxy>(); }
			if (_camera == null) { _camera = GetComponent<Camera>(); }
			if (_camera == null) { _camera = Camera.main; }
			if (_camera == null) { _camera = FindObjectOfType<Camera>(); ; }
		}
#endif
		private void Awake() {
			if (prefab_middleWaypoint == null) {
				prefab_middleWaypoint = Instantiate(prefab_waypoint);
				prefab_middleWaypoint.Text = "cancel";
				prefab_middleWaypoint.name = prefab_waypoint.name + " cancel";
				if (prefab_waypoint.transform.parent != prefab_middleWaypoint.transform.parent) {
					prefab_middleWaypoint.transform.SetParent(prefab_waypoint.transform.parent);
				}
			}
		}

		public List<ClickToMoveFollower> selection = new List<ClickToMoveFollower>();
		public void SetSelection(CharacterMove characterMove) {
			selection.ForEach(f => f.ShowPath(false));
			selection.Clear();
			if (characterMove != null) {
				selection.Add(Follower(characterMove));
			}
			selection.ForEach(f => f.ShowPath(true));
		}
		public ClickToMoveFollower Follower(CharacterMove currentChar) {
			ClickToMoveFollower follower = currentChar.GetComponent<ClickToMoveFollower>();
			if (follower == null) {
				follower = currentChar.gameObject.AddComponent<ClickToMoveFollower>();
				follower.clickToMoveUi = this;
				follower.Init(currentChar.gameObject);
			}
			return follower;
		}

		private void ClickFor(ClickToMoveFollower follower, RaycastHit rh) {
			follower.SetCurrentTarget(rh.point, rh.normal);
			follower.UpdateLine();
		}

		private void Update() {
			if (selection.Count == 0 && characterToMove.Target != null) {
				SetSelection(characterToMove.Target);
			}
			if (Input.GetKey(key)) {
				//Debug.Log("click");
				if (!UiClick.IsMouseOverUi()) {
					//Debug.Log("on map");
					Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
					RaycastHit rh;
					Physics.Raycast(ray, out rh, float.PositiveInfinity, validToMove, moveToTrigger);
					if (rh.collider != null) {
						if (selection.Count > 0) {
							for (int i = 0; i < selection.Count; ++i) {
								if (selection[i] == null) {
									selection.RemoveAt(i--);
									continue;
								}
								ClickFor(selection[i], rh);
							}
						}
					}
					MouseCursor.Instance.currentSet = mouseSetMove;
				} else {
					MouseCursor.Instance.currentSet = mouseSetUi;
				}
			}
			if (prefab_waypoint != null && Input.GetKeyUp(key)) {
				//if (follower != null) { follower.ShowCurrentWaypoint(); }
				if (selection.Count > 0) {
					for (int i = 0; i < selection.Count; ++i) {
						selection[i].ShowCurrentWaypoint();
					}
				}
			}
		}
	}
}