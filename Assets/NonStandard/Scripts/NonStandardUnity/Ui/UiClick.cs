using NonStandard.Inputs;
using NonStandard.Utility.UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
			pointerEventData.position = AppInput.MousePosition;
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
		public static void ClearOnClick(GameObject obj) {
			Button button = obj.GetComponentInChildren<Button>();
			if (button != null) { button.onClick.RemoveAllListeners(); }
			// also clear EventTriggers
			EventTrigger et = obj.GetComponentInChildren<EventTrigger>();
			if (et != null) {
				EventTrigger.Entry entry = et.triggers.Find(t => t.eventID == EventTriggerType.PointerClick);
				if (entry != null) {
					entry.callback.RemoveAllListeners();
				}
			}
		}
		public static bool AddOnButtonClickIfNotAlready(GameObject obj, Object target, UnityAction action) {
			Button button = obj.GetComponent<Button>();
			if (button != null) {
				EventBind.IfNotAlready(button.onClick, target, action);
				return true;
			}
			return false;
		}
		public static void AddOnPanelClickIfNotAlready(GameObject obj, Object target, UnityAction<BaseEventData> action) {
			PointerTrigger.AddEventIfNotAlready(obj, EventTriggerType.PointerClick, target, action);
		}
	}
}