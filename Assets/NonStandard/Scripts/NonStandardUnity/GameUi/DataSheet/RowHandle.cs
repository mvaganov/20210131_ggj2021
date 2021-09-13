using NonStandard.Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonStandard.GameUi.DataSheet {
	public class RowHandle : MonoBehaviour {
		// TODO some kind of variable to keep track of drag order too.
		private DragAction drag = null;
		public class DragAction {
			public int fromIndex;
			public int toIndex;
			public RectTransform predictionRect;
			public DragAction(Transform transform) {
				fromIndex = toIndex = transform.parent.GetSiblingIndex();
				UnityDataSheet uds = transform.GetComponentInParent<UnityDataSheet>();
				GameObject rObj = Instantiate(uds.prefab_dataRow);
				rObj.SetActive(true);
				rObj.transform.SetParent(transform, false);
				predictionRect = rObj.GetComponent<RectTransform>();
				RectTransform pRect = transform.parent.GetComponent<RectTransform>();
				//Show.Log(pRect.sizeDelta.x);
				predictionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pRect.sizeDelta.x);
				predictionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pRect.sizeDelta.y);
				predictionRect.anchoredPosition = Vector2.zero;
				startElement = transform.GetComponent<RectTransform>();
				startingLocalPositionForStartElement = startElement.localPosition;
				sr = transform.GetComponentInParent<ScrollRect>();
				viewport = sr.viewport.GetComponent<RectTransform>();
			}
			public void PointerDrag(PointerEventData ped) {
				const float scrollSpeed = 2;
				Vector2 p = startElement.position;
				p.y = ped.position.y;
				startElement.position = p;
				Direction2D dir = DragWithMouse.CalculatePointerOutOfBounds(viewport, ped.position, out Vector2 offset);
				if (dir != Direction2D.None) {
					scrollVelocity = offset * scrollSpeed;
				}
			}

			public RectTransform startElement;
			public Vector3 startingLocalPositionForStartElement;
			public Vector2 scrollVelocity = Vector2.zero;
			public ScrollRect sr;
			public RectTransform viewport;

			public void Update() {
				if (scrollVelocity != Vector2.zero) { sr.velocity = scrollVelocity; }
			}
		}
		private void Start() {
			PointerTrigger.AddEvent(gameObject, EventTriggerType.PointerDown, this, PointerDown);
			PointerTrigger.AddEvent(gameObject, EventTriggerType.Drag, this, PointerDrag);
			PointerTrigger.AddEvent(gameObject, EventTriggerType.PointerUp, this, PointerUp);
		}
		private void PointerDown(BaseEventData bed) {
			PointerEventData ped = bed as PointerEventData;
			//Show.Log("click DOWN at " + ped.position+" "+ FrameRect().Contains(ped.position));
			ClearDrag();
			drag = new DragAction(transform);
			enabled = true;
		}
		private void PointerDrag(BaseEventData bed) {
			drag.PointerDrag(bed as PointerEventData);
			drag.Update();
		}
		private void Update() {
			if (drag == null) return;
			drag.Update();
		}
		void StateOfDrag(PointerEventData ped, out int oldIndex, out int newIndex, out bool insideFrame) {
			ScrollRect sr = GetComponentInParent<ScrollRect>();
			RectTransform viewport = sr.viewport.GetComponent<RectTransform>();
			Vector3 point = viewport.InverseTransformPoint(ped.position);
			insideFrame = viewport.rect.Contains(point);
			if (drag == null) { throw new System.Exception("missing drag datum"); }
			if (drag.startElement == null) { throw new System.Exception("dunno what started this?"); }
			if (drag.startElement.parent == null) { throw new System.Exception("weird hierarchy?"); }
			RectTransform rowRt = drag.startElement.parent.GetComponent<RectTransform>();
			float rowHeight = rowRt.sizeDelta.y;
			float y = rowRt.localPosition.y;
			oldIndex = (int)(-y / rowHeight);
			newIndex = (int)(-(y + drag.startElement.localPosition.y) / rowHeight);
		}
		private void PointerUp(BaseEventData bed) {
			if (drag == null) { return; } // ignore invalid releases
			PointerEventData ped = bed as PointerEventData;
			StateOfDrag(ped, out int oldIndex, out int newIndex, out bool inFrame);
			//Show.Log($"old{oldIndex}  new{newIndex}  in{inFrame}");
			if (inFrame) {
				UnityDataSheet uds = GetComponentInParent<UnityDataSheet>();
				if (newIndex >= 0 && newIndex < uds.Count) {
					uds.MoveRow(oldIndex, newIndex);
				}
			}
			//Show.Log("click UP at " + ped.position + " " + FrameRect().Contains(ped.position));
			ClearDrag();
			enabled = false;
		}
		void ClearDrag() {
			if (drag == null) return;
			drag.startElement.localPosition = drag.startingLocalPositionForStartElement;
			Destroy(drag.predictionRect.gameObject);
			drag = null;
		}
	}
}