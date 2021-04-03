#if UNITY_EDITOR
using NonStandard;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class MyCustomSceneProcessor : IProcessSceneWithReport {
    public int callbackOrder { get { return 0; } }
    public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report) {
        Debug.Log("MyCustomSceneProcessor.OnProcessScene " + scene.name);
        string path = Show.GetStackFullPath(1, 0)[0];
        char P = Path.DirectorySeparatorChar;
        path = path.Substring(0, path.LastIndexOf(P) + 1);
        //string filename = path + "V.cs", filedata = "namespace NonStandard{public static class V{public static string v=\"" +DateTime.Now.ToString("yyyyMMdd.HHmmss") + "\";}}";
        //File.WriteAllText(filename, filedata);
        File.WriteAllText(path + "Resources" + P + "app_build_time.txt", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}
class MyCustomBuildProcessor : IPreprocessBuildWithReport {
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report) {
        Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
        string path = Show.GetStackFullPath(1, 0)[0];
        char P = Path.DirectorySeparatorChar;
        path = path.Substring(0, path.LastIndexOf(P) + 1);
        //string filename = path + "V.cs", filedata = "namespace NonStandard{public static class V{public static string v=\"" +DateTime.Now.ToString("yyyyMMdd.HHmmss") + "\";}}";
        //File.WriteAllText(filename, filedata);
        File.WriteAllText(path+"Assets"+P+"Resources"+P+"app_build_time.txt", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")); 
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}
#endif