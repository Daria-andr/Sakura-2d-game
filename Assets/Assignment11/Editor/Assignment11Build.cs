using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class Assignment11Build
{
    [MenuItem("Tools/Sakura/Build Assignment 11 APK")]
    public static void Android()
    {
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android)) {
            string reason="BLOCKED: Unity 6000.6.2f1 has no Android Build Support module. Install Android Build Support with SDK/NDK Tools and OpenJDK through Unity Hub, then use Tools > Sakura > Build Assignment 11 APK.";
            File.WriteAllText("Logs/Assignment11-android.txt",reason);
            Debug.LogWarning(reason);return;
        }
        Directory.CreateDirectory("Builds/Android");
        EditorUserBuildSettings.buildAppBundle=false;
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=new[]{"Assets/Scenes/MainMenu.unity","Assets/Scenes/Level1.unity","Assets/Scenes/Level2.unity","Assets/Scenes/BossFight.unity"},
            locationPathName="Builds/Android/Sakura-Assignment11.apk",
            target=BuildTarget.Android,options=BuildOptions.None
        });
        File.WriteAllText("Logs/Assignment11-android.txt",report.summary.result.ToString());
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Android build failed; see Unity build log.");
    }
}
