using System;
#if UNITY_2017_1_OR_NEWER
using UnityEngine;
#endif
namespace NonStandard {
#if UNITY_2017_1_OR_NEWER
	public class Show : MonoBehaviour {
		public GameObject routeOutputTo;
		void Awake() {
			TMPro.TMP_Text tmpText = routeOutputTo.GetComponent<TMPro.TMP_Text>();
			if (tmpText != null) { AddListener(s => tmpText.text += s + "\n"); }
			UnityEngine.UI.Text txt = routeOutputTo.GetComponent<UnityEngine.UI.Text>();
			if (txt != null) { AddListener(s => txt.text += s + "\n"); }
		}
#else
	public class Show {
#endif
		public static Action<string> onLog;
		public static Action<string> onError;
		public static Action<string> onWarning;
		public static void AddListener(Action<string> listener) { onLog += listener; onError += listener; onWarning += listener; }
		public static void Log(object obj) { onLog.Invoke(obj != null ? obj.ToString() : ""); }
		public static void Log(string str) { onLog.Invoke(str); }
		public static void Error(object obj) { onError.Invoke(obj.ToString()); }
		public static void Error(string str) { onError.Invoke(str); }
		public static void Warning(object obj) { onWarning.Invoke(obj.ToString()); }
		public static void Warning(string str) { onWarning.Invoke(str); }

		static Show() {
#if UNITY_2017_1_OR_NEWER
			onLog += UnityEngine.Debug.Log;
			onError += UnityEngine.Debug.LogError;
			onWarning += UnityEngine.Debug.LogWarning;
#else
			onLog += Console.WriteLine;
			onError += s => {
				ConsoleColor c = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(s);
				Console.ForegroundColor = c;
			};
			onWarning += s => {
				ConsoleColor c = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(s);
				Console.ForegroundColor = c;
			};
#endif
		}

	}
}