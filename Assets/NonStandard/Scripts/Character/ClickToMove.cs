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
		private void Start() { }

		void Update() {
			if (Input.GetKey(key)) {
				if (!IsMouseOverUi()) {
					Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
					RaycastHit rh;
					Physics.Raycast(ray, out rh, float.PositiveInfinity, validToMove, moveToTrigger);
					if (rh.collider != null) {
						characterToMove.SetAutoMovePosition(rh.point, () => { characterToMove.DisableAutoMove(); });
					}
				}
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
			//if(raycastResult.Count > 0) {
			//	Debug.Log(raycastResult.JoinToString(", ", r => r.gameObject.name));
			//}
			return raycastResult.Count > 0;
		}
	}
}