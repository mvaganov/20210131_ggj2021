using NonStandard.Data;
using System;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Utility.UnityEditor {
	public class EventBind {
		public object target;
		public string setMethodName;
		public object value;

		public EventBind(object target, string setMethodName, object value = null) {
			this.target = target; this.setMethodName = setMethodName; this.value = value;
		}
		public EventBind(object target, string setMethodName) {
			this.target = target; this.setMethodName = setMethodName; value = null;
		}
		public UnityAction<T> GetAction<T>(object target, string setMethodName) {
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[] { typeof(T) });
			if (targetinfo == null) { Debug.LogError("no method " + setMethodName + "(" + typeof(T).Name + ") in " + target.ToString()); }
			return Delegate.CreateDelegate(typeof(UnityAction<T>), target, targetinfo, false) as UnityAction<T>;
		}
		public static bool IfNotAlready<T>(UnityEvent<T> @event, UnityEngine.Object target, string methodName) {
			for(int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == methodName) { return false; }
			}
			On(@event, target, methodName);
			return true;
		}
		public static void On<T>(UnityEvent<T> @event, object target, string methodName) {
			new EventBind(target, methodName).Bind(@event);
		}
		public static bool IfNotAlready(UnityEvent @event, UnityEngine.Object target, string methodName) {
			for (int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == methodName) { return false; }
			}
			On(@event, target, methodName);
			return true;
		}
		public static void On(UnityEvent @event, object target, string methodName) {
			new EventBind(target, methodName).Bind(@event);
		}
		public void Bind<T>(UnityEvent<T> @event) {
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener(@event, GetAction<T>(target, setMethodName));
#else
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[]{typeof(T)});
			@event.AddListener((val) => targetinfo.Invoke(target, new object[] { val }));
#endif
		}
		public void Bind(UnityEvent_string @event) {
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener(@event, GetAction<string>(target, setMethodName));
#else
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[0]);
			@event.AddListener((str) => targetinfo.Invoke(target, new object[] { str }));
#endif
		}
		public void Bind(UnityEvent @event) {
#if UNITY_EDITOR
			if (value == null) {
				System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, setMethodName, new Type[0]);
				if (targetinfo == null) { Debug.LogError("no method " + setMethodName + "() in " + target.ToString()); }
				UnityAction action = Delegate.CreateDelegate(typeof(UnityAction), target, targetinfo, false) as UnityAction;
				UnityEventTools.AddVoidPersistentListener(@event, action);
			} else if (value is int) {
				UnityEventTools.AddIntPersistentListener(@event, GetAction<int>(target, setMethodName), (int)value);
			} else if (value is float) {
				UnityEventTools.AddFloatPersistentListener(@event, GetAction<float>(target, setMethodName), (float)value);
			} else if (value is string) {
				UnityEventTools.AddStringPersistentListener(@event, GetAction<string>(target, setMethodName), (string)value);
			} else if (value is bool) {
				UnityEventTools.AddBoolPersistentListener(@event, GetAction<bool>(target, setMethodName), (bool)value);
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
				UnityEventTools.AddObjectPersistentListener(@event, GetAction<T>(target, setMethodName), (T)value);
			} else {
				Debug.LogError("unable to assign " + value.GetType());
			}
		}
#endif
	}

}