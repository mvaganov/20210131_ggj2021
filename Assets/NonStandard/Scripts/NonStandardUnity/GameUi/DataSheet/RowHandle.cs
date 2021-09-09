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
			public Vector3 startingLocalPositionForStartElement;
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
			return (Vector2)viewport.position;// + new Vector2(0, viewport.rect.height * viewport.lossyScale.y / 2);
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
			drag.startingLocalPositionForStartElement = drag.startElement.localPosition;
			// TODO mark the row being moved. maybe make it dark? or hide the braille icon?
		}
		private void PointerDrag(BaseEventData bed) {
			PointerEventData ped = bed as PointerEventData;
			//Show.Log(FrameRect().Contains(ped.position) + " drag " + ped.position+ " "+ped.delta + " " + FrameRect());
			Vector2 p = drag.startElement.position;
		//	RectTransform rowRt = drag.startElement.parent.GetComponent<RectTransform>();
		//	float rowHeight = rowRt.sizeDelta.y;
		//	float y = rowRt.localPosition.y;
			//float tableHeight = drag.startElement.parent.parent.GetComponent<RectTransform>().sizeDelta.y;
			//int rowCount = drag.startElement.parent.parent.childCount;
			//float calculatedRowCount = tableHeight / rowHeight;
			p.y = ped.position.y;
			drag.startElement.position = p;
		//	float index = -(y+drag.startElement.localPosition.y) / rowHeight;
			//Show.Log($"rh{rowHeight}  th{tableHeight}  rc{rowCount}  crc{calculatedRowCount}  i{index}");
		}
		void StateOfDrag(PointerEventData ped, out int oldIndex, out int newIndex, out bool insideFrame) {
			insideFrame = FrameRect().Contains(ped.position);

			RectTransform rowRt = drag.startElement.parent.GetComponent<RectTransform>();
			float rowHeight = rowRt.sizeDelta.y;
			float y = rowRt.localPosition.y;
			oldIndex = (int)(-y / rowHeight);
			newIndex = (int)(-(y + drag.startElement.localPosition.y) / rowHeight);
		}
		private void PointerUp(BaseEventData bed) {
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
			drag.startElement.localPosition = drag.startingLocalPositionForStartElement;
			Destroy(drag.predictionRect.gameObject);
			drag = null;
		}
	}
}