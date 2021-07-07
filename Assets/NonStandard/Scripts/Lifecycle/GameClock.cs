﻿using System;
using UnityEngine;
using NonStandard.Procedure;

namespace NonStandard {
	/// <summary>
	/// a class that enables the Proc timer queue to be observed in the editor at runtime
	/// </summary>
	public class GameClock : MonoBehaviour {
		public TimeKeeper timer;
		public Processr mainProcessor;

		public long updateCount = 0;
		public long gameTimeMs = 0;
		private float gameTimeRemainder = 0;
		public long GetUpdateCount() => updateCount;
		public long GetGameTimeMs() => gameTimeMs;

		public PauseEvents pauseEvents = new PauseEvents();
		bool initialized;
		public bool isPaused;

		public static bool IsPaused => Instance().isPaused;
		public static TimeKeeper Timer => Instance().timer;
		public static long Time => Instance().GetGameTimeMs();

		public static Incident Delay(long delayMs, Action action) {
			return Instance().timer.Delay(delayMs, action);
		}
		public static long GetTime() { return Instance().GetGameTimeMs(); }

		[System.Serializable]
		public class PauseEvents {
			[Tooltip("do this when time is paused")] public UnityEngine.Events.UnityEvent onPause;
			[Tooltip("do this when time is unpaused")] public UnityEngine.Events.UnityEvent onUnpause;
		}
		public void Init() {
			if (initialized) { return; }
			initialized = true;
			timer = new TimeKeeper(GetGameTimeMs);
			mainProcessor = Proc.Get();
		}
		public void Awake() { Init(); }
		public void Pause() { isPaused = true; if (pauseEvents.onPause != null) { pauseEvents.onPause.Invoke(); } }
		public void Unpause() { isPaused = false; if (pauseEvents.onPause != null) { pauseEvents.onUnpause.Invoke(); } }

		private void IncrementTime() {
			float deltaTimeMs = (UnityEngine.Time.deltaTime * 1000) + gameTimeRemainder;
			long gameTimeDeltaMs = (long)deltaTimeMs;
			gameTimeRemainder = deltaTimeMs - gameTimeDeltaMs;
			gameTimeMs += gameTimeDeltaMs;
		}

		void Update() {
			Proc.Update();
			if (!isPaused) {
				IncrementTime();
			}
			timer.Update();
		}
		public void FixedUpdate() {
			++updateCount;
		}
		void OnApplicationPause(bool paused) { if (paused) { Pause(); } else { Unpause(); } }
		void OnDisable() { Pause(); }
		void OnEnable() { Unpause(); }
		private void OnDestroy() { }
		private void OnApplicationQuit() { }
#if UNITY_EDITOR
		public void OnValidate() { }
#endif
		internal static GameClock s_instance;
		public static GameClock Instance() {
			if (s_instance != null) return s_instance;
			s_instance = FindObjectOfType<GameClock>();
			if (s_instance == null) { // if it doesn't exist
				GameObject g = new GameObject("<Clock>");
				s_instance = g.AddComponent<GameClock>(); // create one
			}
			return s_instance;
		}
	}
}