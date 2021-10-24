using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace NonStandard.Ui {
	public class DragWithMouse : MonoBehaviour {
		protected RectTransform rt;
		public bool disableDrag;
		public static GameObject beingDragged = null;
		protected Vector3 whereClickHappened;
		protected virtual void Awake() {
			rt = GetComponent<RectTransform>();
			AddPointerEvent(EventTriggerType.Drag, this, OnDrag);
			AddPointerEvent(EventTriggerType.PointerUp, this, PointerUp);
			AddPointerEvent(EventTriggerType.PointerDown, this, PointerDown);
		}

		public void AddPointerEvent(EventTriggerType type, Object target, UnityAction<BaseEventData> pointerEvent) {
			PointerTrigger.AddEvent(gameObject, type, target, pointerEvent);
		}
		public virtual void PointerUp(BaseEventData data) {
			if (beingDragged == gameObject) {
				//Debug.Log("released " + this);
				beingDragged = null;
			}
		}
		public virtual void PointerDown(BaseEventData basedata) {
			PointerEventData data = basedata as PointerEventData;
			whereClickHappened = transform.InverseTransformPoint(data.position);
			//Show.Log("clicked "+ data.position+" : "+whereClickHappened);
		}
		public virtual void OnDrag(BaseEventData basedata) {
			if (disableDrag) return;
			PointerEventData data = basedata as PointerEventData;
			rt.localPosition = transform.parent.InverseTransformPoint(data.position) - whereClickHappened;
			//rt.localPosition += (Vector3)data.delta;
			//if(rt.parent != null) {
			//	Vector3 move = (Vector3)data.delta;
			//	move.Scale(rt.parent.lossyScale);
			//	rt.anchoredPosition += (Vector2)move;
			//	//	RectTransform canvasRt = rt.parent.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
			//	//	if (canvasRt != null) {
			//	//		KeepInBounds(canvasRt);
			//	//	}
			//}
			beingDragged = gameObject;
		}

		public void KeepInBounds(RectTransform canvasRt) {
			Rect area = rt.rect;
			Vector3 pos = canvasRt.TransformPoint(rt.position);//rt.localPosition;
			Vector2 size = canvasRt.sizeDelta;
			Show.Log(pos+" in "+area);
			bool change = false;
			if (area.yMin < 0) { pos.y = 0; change = true; }
			if (area.xMin < 0) { pos.x = 0; change = true; }
			if (area.yMax > size.y) { pos.y = 0; change = true; }
			if (area.xMax > size.x) { pos.x = 0; change = true; }
			if (change) {
				rt.localPosition = pos;
			}
		}
		public bool IsPointerInBounds(Vector3 pointerPosition) { return IsPointerInBounds(rt, pointerPosition); }
		public bool IsPointerInBounds(RectTransform rt, Vector3 pointerPosition) {
			Vector3 p = rt.InverseTransformPoint(pointerPosition);
			return rt.rect.Contains(p);
		}
		// TODO make some kind of FloatRect class, and put this functionality in that?
		public static Direction2D CalculatePointerOutOfBounds(RectTransform rt, Vector3 pointerPosition, out Vector2 offset, float borderExpansion = 0) {
			Vector3 p = rt.InverseTransformPoint(pointerPosition);
			Rect rect = rt.rect;
			if (borderExpansion != 0) {
				Vector2 adjust = Vector2.one * borderExpansion;
				rect.min -= adjust;
				rect.max += adjust;
			}
			float y = p.y, yMin = rect.yMin, yMax = rect.yMax, yDelta = 0;
			float x = p.x, xMin = rect.xMin, xMax = rect.xMax, xDelta = 0;
			//Show.Log("yMin:"+viewport.rect.yMin + "  yMax:" + viewport.rect.yMax+"  y:" + point.y);
			Direction2D dir = Direction2D.None;
			if (y > yMax) { yDelta += (yMax - y); dir |= Direction2D.Top; }
			if (y < yMin) { yDelta += (yMin - y); dir |= Direction2D.Bottom; }
			if (x > xMax) { xDelta += (xMax - x); dir |= Direction2D.Right; }
			if (x < xMin) { xDelta += (xMin - x); dir |= Direction2D.Left; }
			offset = new Vector2(xDelta, yDelta);
			return dir;
		}
	}
}