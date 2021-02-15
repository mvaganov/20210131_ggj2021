using NonStandard;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextScene : MonoBehaviour {
	public string sceneName;
	public Image progressBar;
	public void DoActivateTrigger() {
		AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
		if(progressBar != null) {
			void UpdateProgressVisual() {
				if (ao.isDone) return;
				progressBar.fillAmount = ao.progress;
				Clock.setTimeout(UpdateProgressVisual, 20);
			}
			UpdateProgressVisual();
		}
	}
}
