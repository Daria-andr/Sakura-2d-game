using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class Assignment11Tests
{
    static Assignment11Tests() { EditorApplication.playModeStateChanged += OnState; }
    public static void Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        EditorApplication.LockReloadAssemblies();
        SessionState.SetBool("Assignment11Tests",true);
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.isPlaying = true;
    }
    static void OnState(PlayModeStateChange state)
    {
        if(!SessionState.GetBool("Assignment11Tests",false)) return;
        if(state == PlayModeStateChange.EnteredPlayMode)
            new GameObject("Assignment11 automated verification").AddComponent<Assignment11SmokeTests>();
        if(state == PlayModeStateChange.ExitingPlayMode) SessionState.SetBool("Assignment11Tests",false);
    }
    public static void FinalChecks()
    {
        VerifyRestart();
        SaveSystem.TestPath=null; SaveSystem.Reload();
        Assignment11Build.Android();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
    }
    public static void VerifyRestart()
    {
        SaveSystem.TestPath=Path.Combine(Application.persistentDataPath,"Assignment11Tests","progress.json");
        SaveSystem.Reload();
        if(!SaveSystem.Data.completed[0] || SaveSystem.Data.bestScores[0]!=700 ||
           Mathf.Abs(SaveSystem.Data.volume-.37f)>.001f) throw new Exception("Cross-process save reload failed.");
        File.WriteAllText("Logs/Assignment11-restart.txt","PASS: progress, best score and volume survived a separate Unity process.\nProduction save: "+Path.Combine(Application.persistentDataPath,"sakura-progress.json"));
    }
}
