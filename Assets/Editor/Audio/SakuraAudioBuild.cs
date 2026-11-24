using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
public static class SakuraAudioBuild
{
    [MenuItem("Tools/Sakura/Build Assignment 12 APK")]
    public static void Android() {
        Directory.CreateDirectory("Logs"); Directory.CreateDirectory("Builds/Android");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            target=BuildTarget.Android, locationPathName="Builds/Android/Sakura-Assignment12.apk", options=BuildOptions.None
        });
        File.WriteAllText("Logs/Assignment12-android.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nBytes: "+report.summary.totalSize);
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Assignment 12 Android build failed; inspect build log.");
    }
    public static void VerifyRestart() {
        SaveSystem.TestPath=Path.GetFullPath("Logs/Assignment12TestSave/progress.json"); SaveSystem.Reload();
        if(Mathf.Abs(SaveSystem.Data.musicVolume-.32f)>.001f || Mathf.Abs(SaveSystem.Data.sfxVolume-.48f)>.001f || SaveSystem.Data.bestScores[0]!=700)
            throw new Exception("Separate-process audio persistence failed");
        File.WriteAllText("Logs/Assignment12-restart.txt","PASS: separate Unity process loaded both audio settings and preserved score 700. Test JSON only.");
        SaveSystem.TestPath=null;
    }
    public static void Audit() {
        var report=new System.Text.StringBuilder(); int missing=0,unrouted=0;
        foreach(var guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets"})) {
            var path=AssetDatabase.GUIDToAssetPath(guid); if(path.Contains("TextMesh Pro")||path.Contains("Settings/Scenes")) continue;
            var scene=EditorSceneManager.OpenScene(path);
            foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true)) {
                missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                foreach(var source in t.GetComponents<AudioSource>()) if(!source.outputAudioMixerGroup) unrouted++;
            }
            report.AppendLine(path+": inspected");
        }
        report.AppendLine("Missing scripts: "+missing+"; unrouted sources: "+unrouted);
        File.WriteAllText("Logs/Assignment12-audit.txt",report.ToString());
        if(missing!=0 || unrouted!=0) throw new Exception("Scene audit failed");
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
    }
    public static void FinalChecks() { VerifyRestart(); Audit(); Android(); }
}
