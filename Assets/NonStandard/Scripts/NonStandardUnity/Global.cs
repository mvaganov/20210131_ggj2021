using System.Collections.Generic;
using UnityEngine;

namespace NonStandard {
	public partial class Global : MonoBehaviour {
		private static Global _instance;
		private static List<Global> globs = new List<Global>();
		public static Global Instance() {
			if (_instance) { return _instance; }
			Global[] found = FindObjectsOfType<Global>();
			if (found.Length > 0) {
				globs.AddRange(found);
				globs.Sort((a, b) => -a.GetComponents(typeof(Component)).Length.CompareTo(b.GetComponents(typeof(Component)).Length));
				_instance = found[0];
			}
			if (!_instance) { _instance = new GameObject("<global>").AddComponent<Global>(); }
			return _instance;
		}
		public static GameObject Get() { return Instance().gameObject; }
		//public static T Get<T>(bool includeInactive = true) where T : Component {
		//	return null;
		//}
		public static T GetComponent<T>(bool includeInactive = true) where T : Component {
			T found = Get<T>(null);
			if (found != null) return found;
			T componentInstance = Instance().GetComponentInChildren<T>(includeInactive);
			if (componentInstance == null) {
				for(int i = 0; i < globs.Count; ++i) {
					Global g = globs[i];
					if (g == null) { globs.RemoveAt(i--); continue; }
					componentInstance = g.GetComponentInChildren<T>(includeInactive);
					if (componentInstance) { break; }
				}
			}
			if (componentInstance == null) { componentInstance = _instance.gameObject.AddComponent<T>(); }
			directory[typeof(T)] = componentInstance;
			return componentInstance;
		}
		public void Pause() { GameClock.Instance().Pause(); }
		public void Unpause() { GameClock.Instance().Unpause(); }
		public void TogglePause() { GameClock c = GameClock.Instance(); if(c.isPaused) { c.Unpause(); } else { c.Pause(); } }
		public void ToggleActive(GameObject go) {
			if (go != null) {
				go.SetActive(!go.activeSelf);
				//Debug.Log(go+" "+go.activeInHierarchy);
			}
		}
		public void ToggleEnabled(MonoBehaviour m) { if (m != null) { m.enabled = !m.enabled; } }
		void Start() {
			Instance();
			if (globs.IndexOf(this) < 0) { globs.Add(this); }
			Component[] components = GetComponents<Component>();
			for(int i = 0; i < components.Length; ++i) {
				System.Type t = components[i].GetType();
				if (!directory.TryGetValue(t, out object foundIt)) {
					directory[t] = components[i];
				}
			}
		}
	}
}