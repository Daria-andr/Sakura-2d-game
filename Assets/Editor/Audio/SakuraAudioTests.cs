using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class SakuraAudioTests
{
    static SakuraAudioTests() { EditorApplication.playModeStateChanged += State; }
    [MenuItem("Tools/Sakura/Test Assignment 12 Audio")]
    public static void Run() {
        File.WriteAllText("Logs/Assignment12-tests.txt", "Automated Unity Editor Play Mode checks\n");
        SessionState.SetBool("SakuraAudioTesting",true);
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.isPlaying=true;
    }
    static void State(PlayModeStateChange state) {
        if(state==PlayModeStateChange.EnteredEditMode) { SaveSystem.TestPath=null; SaveSystem.Reload(); }
        if(!SessionState.GetBool("SakuraAudioTesting",false)) return;
        if(state==PlayModeStateChange.EnteredPlayMode) new GameObject("Audio test runner").AddComponent<SakuraAudioTestRunner>();
    }
}


