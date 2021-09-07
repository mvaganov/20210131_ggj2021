using NonStandard.Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NonStandard.GameUi.DataSheet {
	public class RowHandle : MonoBehaviour {
		public class DragAction {
			public int fromIndex;
			public int toIndex;
			public RectTransform predictionRect;
			public DragAction(int startIndex) { fromIndex = toIndex = startIndex; }
			public RectTransform startElement;
		}
		private DragAction drag = null;
		// TODO drag-and-drop interface to re-order elements
		// TODO some kind of variable to keep track of drag order
		private void Start() {
			PointerTrigger.AddEvent(gameObject, EventTriggerType.PointerDown, this, PointerDown);
			PointerTrigger.AddEvent(gameObject, EventTriggerType.Drag, this, PointerDrag);
			PointerTrigger.AddEvent(gameObject, EventTriggerType.PointerUp, this, PointerUp);
		}
		private Vector2 FramePosition(RectTransform viewport) {
			return (Vector2)viewport.position + new Vector2(0, viewport.rect.height * viewport.lossyScale.y / 2);
		}
		private Rect FrameRect() {
			ScrollRect sr = GetComponentInParent<ScrollRect>();
			if(sr != null) {
				RectTransform rt = sr.viewport.GetComponent<RectTransform>();
				Rect r = rt.rect;
				r.size *= rt.lossyScale;
				r.position += FramePosition(rt);// (Vector2)rt.position + new Vector2(0, rt.rect.height / 2);
				return r;
			}
			return new Rect();
		}
		private void PointerDown(BaseEventData bed) {
			PointerEventData ped = bed as PointerEventData;
			//Show.Log("click DOWN at " + ped.position+" "+ FrameRect().Contains(ped.position));
			drag = new DragAction(transform.parent.GetSiblingIndex());
			UnityDataSheet uds = GetComponentInParent<UnityDataSheet>();
			GameObject rObj = Instantiate(uds.prefab_dataRow);
			rObj.SetActive(true);
			rObj.transform.SetParent(transform,false);
			drag.predictionRect = rObj.GetComponent<RectTransform>();
			RectTransform pRect = transform.parent.GetComponent<RectTransform>();
			//Show.Log(pRect.sizeDelta.x);
			drag.predictionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pRect.sizeDelta.x);
			drag.predictionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pRect.sizeDelta.y);
			drag.predictionRect.anchoredPosition = Vector2.zero;
			drag.startElement = GetComponent<RectTransform>();
			// TODO mark the row being moved. maybe make it dark? or hide the braille icon?
		}
		private void PointerDrag(BaseEventData bed) {
			PointerEventData ped = bed as PointerEventData;
			//Show.Log("drag " + ped.position+ " "+ped.delta + " " + FrameRect().Contains(ped.position)+ " "+ FrameRect());
			Vector2 p = drag.startElement.position;
			p.y = ped.position.y;
			drag.startElement.position = p;
			// TODO calculate the new desired index based on position
			// TODO if the position is higher than the view frame, scroll up. if lower, scroll down.
		}
		private void PointerUp(BaseEventData bed) {
			PointerEventData ped = bed as PointerEventData;

			// put it back? TODO shift the element!
			Vector2 p = drag.startElement.anchoredPosition;
			p.y = 0;
			drag.startElement.anchoredPosition = p;

			//Show.Log("click UP at " + ped.position + " " + FrameRect().Contains(ped.position));
			Destroy(drag.predictionRect.gameObject);
			drag = null;
		}
	}
}