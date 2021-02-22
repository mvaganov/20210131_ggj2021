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

		CharacterMove currentChar = null;
		ClickToMoveFollower follower;

		private void Update() {
			if (Input.GetKey(key)) {
				//Debug.Log("click");
				if (!UiClick.IsMouseOverUi()) {
					//Debug.Log("on map");
					Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
					RaycastHit rh;
					Physics.Raycast(ray, out rh, float.PositiveInfinity, validToMove, moveToTrigger);
					if (rh.collider != null) {
						currentChar = characterToMove.target;
						if (follower == null || currentChar != follower.mover) {// calcDoneForMe) {
							follower = currentChar.GetComponent<ClickToMoveFollower>();
							if (follower == null) {
								follower = currentChar.gameObject.AddComponent<ClickToMoveFollower>();
								follower.clickToMoveUi = this;
								follower.Init(currentChar.gameObject);
							}
						}
						//Debug.Log("hit");
						follower.SetCurrentTarget(rh.point, rh.normal);
						follower.UpdateLine();
					}
					MouseCursor.Instance.currentSet = mouseSetMove;
				} else {
					MouseCursor.Instance.currentSet = mouseSetUi;
				}
			}
			if (prefab_waypoint != null && Input.GetKeyUp(key)) {
				if (follower != null) { follower.ShowCurrentWaypoint(); }
			}
		}
	}
}