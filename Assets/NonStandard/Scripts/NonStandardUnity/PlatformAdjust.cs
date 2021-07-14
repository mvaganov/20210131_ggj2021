using UnityEngine;
using UnityEngine.Events;

namespace NonStandard {
	public class PlatformAdjust : MonoBehaviour {
#if UNITY_EDITOR
		public UnityEvent onUnityEditorStart;
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER
		public UnityEvent onWebplayerStart;
#endif
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE
		public UnityEvent onMobileStart;
#endif
		public UnityEvent onPause;
		public UnityEvent onUnpause;

		void Start() {
#if UNITY_EDITOR
			onUnityEditorStart.Invoke();
#endif
#if UNITY_WEBPLAYER
			onWebplayerStart.Invoke();
#endif
#if UNITY_ANDROID || UNITY_IPHONE
			onMobileStart.Invoke();
#endif
			if (onPause != null) { GameClock.Instance().pauseEvents.onPause.AddListener(onPause.Invoke); }
			if (onUnpause != null) { GameClock.Instance().pauseEvents.onUnpause.AddListener(onUnpause.Invoke); }
		}
		public void Quit() { Exit(); }

		public static void Exit() {
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
			Application.OpenURL(webplayerQuitURL);
#else
			Application.Quit();
#endif
		}
	}
}