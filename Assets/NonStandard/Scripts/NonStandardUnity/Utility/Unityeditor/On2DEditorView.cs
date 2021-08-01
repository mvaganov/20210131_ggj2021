using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Utility.UnityEditor {
	/// <summary>
	/// used to turn off UI canvases in 3D edit mode, and turn them back on in 2D edit mode
	/// </summary>
	[ExecuteInEditMode]
	public class On2DEditorView : MonoBehaviour {
#if UNITY_EDITOR
		public UnityEvent on2DEditorEnter;
		public UnityEvent on2DEditorExit;
		bool wasInEditorModeLastTime = false;
		private void Start() {
			if (Application.isPlaying) { enabled = false; }
		}
		void Update() {
			SceneView sv = SceneView.sceneViews[0] as SceneView;
			if (sv.in2DMode != wasInEditorModeLastTime) {
				if (!wasInEditorModeLastTime) { on2DEditorEnter?.Invoke(); }
				if (wasInEditorModeLastTime) { on2DEditorExit?.Invoke(); }
			}
			wasInEditorModeLastTime = sv.in2DMode;
		}
#endif
	}
}