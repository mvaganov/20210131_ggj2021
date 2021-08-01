#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NonStandard.Utility.UnityEditor {
	class MyCustomSceneProcessor : IProcessSceneWithReport {
		public int callbackOrder { get { return 0; } }
		public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report) {
			char P = Path.DirectorySeparatorChar;
			string resourcesDir = Application.dataPath + P + "Resources";
			if (!Directory.Exists(resourcesDir)) {
				Directory.CreateDirectory(resourcesDir);
			}
			string pathToWriteTo = resourcesDir + P + "app_build_time.txt";
			File.WriteAllText(pathToWriteTo, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}
	}
	class MyCustomBuildProcessor : IPreprocessBuildWithReport {
		public int callbackOrder { get { return 0; } }
		public void OnPreprocessBuild(BuildReport report) {
			Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
		}
	}
}
#endif