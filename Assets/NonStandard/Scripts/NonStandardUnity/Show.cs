﻿using UnityEngine;

namespace NonStandard {
    public partial class Show : MonoBehaviour {
        public GameObject routeOutputTo;

        static Show() {
            Route.onLog -= DefaultLog;
            Route.onError -= DefaultError;
            Route.onWarning -= DefaultWarning;
#if UNITY_EDITOR
            Route.onLog = UnityEngine.Debug.Log;
            Route.onError = UnityEngine.Debug.LogError;
            Route.onWarning = UnityEngine.Debug.LogWarning;
#endif
        }

        void Awake() {
            bool routed = RouteTMP();
            if (!routed) { routed = RouteUiText(); }
        }
        private bool RouteTMP() {
            TMPro.TMP_Text tmp = routeOutputTo.GetComponent<TMPro.TMP_Text>();
            if (tmp != null) {
                if (tmp.richText) {
                    Route.onLog = s => tmp.text += s + "\n";
                    Route.onError = s => tmp.text += "<#ff0000>" + s + "</color>\n";
                    Route.onWarning = s => tmp.text += "<#ffff00>" + s + "</color>\n";
                } else {
                    Route.onAnyMessage = s => tmp.text += s + "\n";
                }
                return true;
            }
            return false;
        }
        private bool RouteUiText() {
            UnityEngine.UI.Text txt = routeOutputTo.GetComponent<UnityEngine.UI.Text>();
            if (txt != null) {
                Route.onAnyMessage = s => txt.text += s + "\n";
                return true;
            }
            return false;
        }
    }
}