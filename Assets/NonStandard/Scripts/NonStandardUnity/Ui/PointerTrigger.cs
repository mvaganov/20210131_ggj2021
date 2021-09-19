using NonStandard.Utility.UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NonStandard.Ui {
	public static class PointerTrigger {
		/// <param name="go"></param>
		/// <param name="type"></param>
		/// <param name="target">if not null, and in the UnityEditor, will register the event in the UI</param>
		/// <param name="pointerEvent">tip: try to typecast the <see cref="BaseEventData"/> as <see cref="PointerEventData"/></param>
		public static void AddEvent(GameObject go, EventTriggerType type, object target, UnityAction<BaseEventData> pointerEvent) {
			EventTrigger et = go.GetComponent<EventTrigger>();
			if (et == null) { et = go.AddComponent<EventTrigger>(); }
			AddEvent(et, type, target, pointerEvent);
		}
		public static void AddEventIfNotAlready(GameObject go, EventTriggerType type, Object target, UnityAction<BaseEventData> pointerEvent) {
			EventTrigger et = go.GetComponent<EventTrigger>();
			if (et == null) { et = go.AddComponent<EventTrigger>(); }
			AddEventIfNotAlready(et, type, target, pointerEvent);
		}
		/// <param name="et"></param>
		/// <param name="type"></param>
		/// <param name="target">if not null, and in the UnityEditor, will register the event in the UI</param>
		/// <param name="pointerEvent">tip: try to typecast the <see cref="BaseEventData"/> as <see cref="PointerEventData"/></param>
		public static void AddEvent(EventTrigger et, EventTriggerType type, object target, UnityAction<BaseEventData> pointerEvent) {
			EventTrigger.Entry entry = et.triggers.Find(t => t.eventID == type);
			if (entry == null) { entry = new EventTrigger.Entry { eventID = type }; }
			EventBind.On(entry.callback, target, pointerEvent);
			et.triggers.Add(entry);
		}
		public static bool AddEventIfNotAlready(EventTrigger et, EventTriggerType type, Object target, UnityAction<BaseEventData> action) {
			EventTrigger.Entry entry = et.triggers.Find(t => t.eventID == type);
			if (entry == null) { AddEvent(et, type, target, action); return true; }
			for (int i = 0; i < entry.callback.GetPersistentEventCount(); ++i) {
				if (entry.callback.GetPersistentTarget(i) == target && entry.callback.GetPersistentMethodName(i) == action.Method.Name) { return false; }
			}
			AddEvent(et, type, target, action); return true;
		}
		public static void RemoveEvent(GameObject go, EventTriggerType type, Object target, UnityAction<BaseEventData> pointerEvent) {
			EventTrigger et = go.GetComponent<EventTrigger>();
			if (et != null) { RemoveEvent(et, type, target, pointerEvent); }
		}
		public static void RemoveEvent(EventTrigger et, EventTriggerType type, Object target, UnityAction<BaseEventData> pointerEvent) {
			EventTrigger.Entry entry = et.triggers.Find(t=>t.eventID == type);
			if (entry == null) return;
			entry.callback.RemoveListener(pointerEvent);

			List<int> eventsToRemove = new List<int>();
			for (int i = 0; i < entry.callback.GetPersistentEventCount(); ++i) {
				if (entry.callback.GetPersistentTarget(i) == target && entry.callback.GetPersistentMethodName(i) == pointerEvent.Method.Name) {
					eventsToRemove.Add(i);
				}
			}
			if (eventsToRemove.Count < entry.callback.GetPersistentEventCount()) {
				// for whatever reason, UnityEventTools.RemovePersistentListener() exists, but only in the UnityEditor context.
				// to delete a unity event listener, the entire event must simply be replaced by a new event with the offending listener missing.
				EventTrigger.Entry newEntry = new EventTrigger.Entry { eventID = type };
				for (int i = 0; i < entry.callback.GetPersistentEventCount(); ++i) {
					if (!eventsToRemove.Contains(i)) {
						EventBind.On(newEntry.callback, entry.callback.GetPersistentTarget(i), entry.callback.GetPersistentMethodName(i));
					}
				}
				et.triggers.Remove(entry);
				et.triggers.Add(newEntry);
			}
		}
	}
}