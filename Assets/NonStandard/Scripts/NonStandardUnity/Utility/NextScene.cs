using NonStandard.Process;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NonStandard.Utility {
	public class NextScene : MonoBehaviour {
		public string sceneName;
		public Image progressBar;
		public void DoActivateTrigger() {
			AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
			if (progressBar != null) {
				void UpdateProgressVisual() {
					if (ao.isDone) return;
					progressBar.fillAmount = ao.progress;
					//Clock.setTimeout(UpdateProgressVisual, 20);
					Proc.Delay(20, UpdateProgressVisual);
				}
				UpdateProgressVisual();
			}
		}
	}
}