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
				if (!IsMouseOverUi()) {
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
								follower.mover = currentChar;
								follower.Init();
							}
						}
						//Debug.Log("hit");
						follower.SetCurrentTarget(rh.point, rh.normal);
					}
				}
			}
			if (prefab_waypoint != null && Input.GetKeyUp(key)) {
				if (follower != null) { follower.ShowCurrentWaypoint(); }
			}
		}
		private bool IsMouseOverUi() {
			if (DragWithMouse.beingDragged != null) { return true; }
			if (!EventSystem.current.IsPointerOverGameObject()) return false;
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = Input.mousePosition;
			List<RaycastResult> raycastResult = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, raycastResult);
			for(int i = 0; i < raycastResult.Count; ++i) {
				ClickToMovePassthrough c2mpt = raycastResult[i].gameObject.GetComponent<ClickToMovePassthrough>();
				if (c2mpt) { raycastResult.RemoveAt(i--); }
			}
			//if (raycastResult.Count > 0) {
			//	Debug.Log(raycastResult.JoinToString(", ", r => r.gameObject.name));
			//}
			return raycastResult.Count > 0;
		}
	}
}