using System.Collections.Generic;
using UnityEngine;

namespace NonStandard {
	public class Global : MonoBehaviour {
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
		public static T Get<T>(bool includeInactive = true) where T : Component {
			T componentInstance = Instance().GetComponentInChildren<T>(includeInactive);
			if (componentInstance == null) {
				for(int i = 0; i < globs.Count; ++i) {
					Global g = globs[i];
					if (g == null || g == _instance) { globs.RemoveAt(i--); continue; }
					componentInstance = g.GetComponentInChildren<T>(includeInactive);
					if (componentInstance) return componentInstance;
				}
			}
			if (componentInstance == null) { componentInstance = _instance.gameObject.AddComponent<T>(); }
			return componentInstance;
		}
		public void Pause() { Clock.Instance.Pause(); }
		public void Unpause() { Clock.Instance.Unpause(); }
		public void TogglePause() { Clock c = Clock.Instance; if(c.IsPaused) { c.Unpause(); } else { c.Pause(); } }
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
		}
	}
}