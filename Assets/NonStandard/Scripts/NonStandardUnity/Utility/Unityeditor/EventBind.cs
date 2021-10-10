using NonStandard.Data;
using NonStandard.Extension;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Utility.UnityEditor {
	public class EventBind {
		public object target;
		public string methodName;
		public object value;

		public EventBind(object target, string setMethodName, object value = null) {
			this.target = target; this.methodName = setMethodName; this.value = value;
		}
		public EventBind(object target, string setMethodName) {
			this.target = target; this.methodName = setMethodName; value = null;
		}
		public UnityAction<T> GetAction<T>(object target, string setMethodName) {
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[] { typeof(T) });
			if (targetinfo == null) {
				Debug.LogError("no method \"" + setMethodName + "\" (" + typeof(T).Name + ") in " + target.ToString());
				return null;
			}
			return Delegate.CreateDelegate(typeof(UnityAction<T>), target, targetinfo, false) as UnityAction<T>;
		}
		public static bool IfNotAlready<T>(UnityEvent<T> @event, UnityEngine.Object target, string methodName) {
			for(int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == methodName) { return false; }
			}
			On(@event, target, methodName);
			return true;
		}
		public static bool IfNotAlready(UnityEvent @event, UnityEngine.Object target, UnityAction action) {
			for (int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == action.Method.Name) { return false; }
			}
			On(@event, target, action);
			return true;
		}
		public static bool IfNotAlready<T>(UnityEvent<T> @event, UnityEngine.Object target, UnityAction<T> action) {
			for (int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == action.Method.Name) { return false; }
			}
			On(@event, target, action);
			return true;
		}
		public static void On(UnityEvent @event, object target, UnityAction action) {
#if UNITY_EDITOR
			if (target != null) {
				new EventBind(target, action.Method.Name).Bind(@event);
				return;
			}
#endif
			@event.AddListener(action.Invoke);
		}
		public static void On<T>(UnityEvent<T> @event, object target, UnityAction<T> action) {
#if UNITY_EDITOR
			if (target != null) {
				new EventBind(target, action.Method.Name).Bind(@event);
				return;
			}
#endif
			@event.AddListener(action.Invoke);
		}
		public static void On<T>(UnityEvent<T> @event, object target, string methodName) {
			new EventBind(target, methodName).Bind(@event);
		}
		public bool IsAlreadyBound(UnityEventBase @event) {
			UnityEngine.Object obj = target as UnityEngine.Object;
			return obj != null && IsAlreadyBound(@event, obj, methodName);
		}
		public static bool IsAlreadyBound(UnityEventBase @event, UnityEngine.Object target, string methodName) {
			for (int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == methodName) { return true; }
			}
			return false;
		}
		public static bool IfNotAlready(UnityEvent @event, UnityEngine.Object target, string methodName) {
			if (IsAlreadyBound(@event, target, methodName)) return false;
			On(@event, target, methodName);
			return true;
		}
		public static void On(UnityEvent @event, object target, string methodName) {
			new EventBind(target, methodName).Bind(@event);
		}
		public void Bind<T>(UnityEvent<T> @event) {
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener(@event, GetAction<T>(target, methodName));
#else
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[]{typeof(T)});
			@event.AddListener((val) => targetinfo.Invoke(target, new object[] { val }));
#endif
		}
		public void Bind(UnityEvent_string @event) {
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener(@event, GetAction<string>(target, methodName));
#else
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[0]);
			@event.AddListener((str) => targetinfo.Invoke(target, new object[] { str }));
#endif
		}
		public void Bind(UnityEvent @event) {
#if UNITY_EDITOR
			if (value == null) {
				System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, methodName, new Type[0]);
				if (targetinfo == null) { Debug.LogError("no method " + methodName + "() in " + target.ToString()); }
				UnityAction action = Delegate.CreateDelegate(typeof(UnityAction), target, targetinfo, false) as UnityAction;
				UnityEventTools.AddVoidPersistentListener(@event, action);
			} else if (value is int) {
				UnityEventTools.AddIntPersistentListener(@event, GetAction<int>(target, methodName), (int)value);
			} else if (value is float) {
				UnityEventTools.AddFloatPersistentListener(@event, GetAction<float>(target, methodName), (float)value);
			} else if (value is string) {
				UnityEventTools.AddStringPersistentListener(@event, GetAction<string>(target, methodName), (string)value);
			} else if (value is bool) {
				UnityEventTools.AddBoolPersistentListener(@event, GetAction<bool>(target, methodName), (bool)value);
			} else if (value is GameObject) {
				Bind<GameObject>(@event);
			} else if (value is Transform) {
				Bind<Transform>(@event);
			} else {
				Debug.LogError("unable to assign " + value.GetType());
			}
#else
				System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[0]);
				@event.AddListener(() => targetinfo.Invoke(target, new object[] { value }));
#endif
		}
#if UNITY_EDITOR
		public void Bind<T>(UnityEvent @event) where T : UnityEngine.Object {
			if (value is T) {
				UnityEventTools.AddObjectPersistentListener(@event, GetAction<T>(target, methodName), (T)value);
			} else {
				Debug.LogError("unable to assign " + value.GetType());
			}
		}
#endif
		public static List<EventBind> GetList(UnityEventBase @event) {
			List<EventBind> eb = new List<EventBind>();
			for(int i = 0; i <@event.GetPersistentEventCount(); ++i) {
				eb.Add(new EventBind(@event.GetPersistentTarget(i), @event.GetPersistentMethodName(i)));
			}
			return eb;
		}
		public static string DebugPrint(UnityEventBase @event) {
			List<EventBind> eb = GetList(@event);
			return eb.JoinToString();
		}
		public override string ToString() {
			string t = null;
			if (target is UnityEngine.Object o) { t = o.name; }
			else if (target != null) { t = target.ToString(); }
			return t+"."+methodName;
		}
	}

}