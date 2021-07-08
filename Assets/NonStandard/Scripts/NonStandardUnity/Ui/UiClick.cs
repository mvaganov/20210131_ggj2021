using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonStandard.Ui {
	public class UiClick : MonoBehaviour {
		public bool ignoreClicksOnThisElement = true;
		public static void Click(Button b, bool forceExecute = true) {
			if (forceExecute) {
				b.onClick.Invoke();
			} else {
				b.OnPointerClick(Click());
			}
		}
		public static PointerEventData Click(Vector2 position = default(Vector2)) {
			return new PointerEventData(EventSystem.current) { position = position };
		}
		public static bool IsMouseOverUi() {
			if (DragWithMouse.beingDragged != null) { return true; }
			if (!EventSystem.current.IsPointerOverGameObject()) return false;
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = Input.mousePosition;
			List<RaycastResult> raycastResult = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, raycastResult);
			for (int i = 0; i < raycastResult.Count; ++i) {
				UiClick uiClick = raycastResult[i].gameObject.GetComponent<UiClick>();
				if (uiClick && uiClick.ignoreClicksOnThisElement) { raycastResult.RemoveAt(i--); }
			}
			//if (raycastResult.Count > 0) {
			//	Debug.Log(raycastResult.JoinToString(", ", r => r.gameObject.name));
			//}
			return raycastResult.Count > 0;
		}
	}
}