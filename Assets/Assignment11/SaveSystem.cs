using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class ProgressData
{
    public int version;
    public int highestUnlocked;
    public int currentLevel;
    public bool[] completed;
    public int[] bestScores;
    public float volume;
    public int audioVersion;
    public float musicVolume;
    public float sfxVolume;
}

public static class SaveSystem
{
    public static readonly string[] Scenes = { "Level1", "Level2", "BossFight" };
    public static ProgressData Data { get; private set; }
    public static string LastError { get; private set; }
    public static event Action Changed;
#if UNITY_EDITOR
    public static string TestPath;
#endif
    public static string FilePath {
        get {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(TestPath)) return TestPath;
#endif
            return Path.Combine(Application.persistentDataPath, "sakura-progress.json");
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitializeStatics() { Data = null; Changed = null; LastError = null; }
    public static ProgressData Defaults() => new ProgressData {
        version = 1, highestUnlocked = 0, currentLevel = 0,
        completed = new bool[3], bestScores = new int[3], volume = 1f, audioVersion = 1, musicVolume = .75f, sfxVolume = .85f
    };
    public static void EnsureLoaded() { if(Data == null) Reload(); }
    public static void Reload()
    {
        LastError = null;
        bool recovered = false;
        try {
            var json = File.ReadAllText(FilePath);
            // Require every schema field: JsonUtility otherwise silently accepts missing members.
            foreach(var key in new[] { "version", "highestUnlocked", "currentLevel", "completed", "bestScores" })
                if(!json.Contains("\"" + key + "\"")) throw new FormatException("Incomplete save.");
            Data = JsonUtility.FromJson<ProgressData>(json);
            if(Data != null) {
                Data.volume = json.Contains("\"volume\"") ? CleanVolume(Data.volume,1f) : 1f;
                Data.musicVolume = Data.audioVersion == 0 || !json.Contains("\"musicVolume\"") ? .75f : CleanVolume(Data.musicVolume,.75f);
                Data.sfxVolume = Data.audioVersion == 0 || !json.Contains("\"sfxVolume\"") ? .85f : CleanVolume(Data.sfxVolume,.85f);
                Data.audioVersion = 1;
            }
            if(!Valid(Data)) throw new FormatException("Invalid or incompatible progress.");
        }
        catch(Exception e) when(e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is FormatException) {
            Data = Defaults();
            recovered = true;
        }
        AudioListener.volume = 1f;
        if(recovered) Save();
        Changed?.Invoke();
    }
    public static bool Valid(ProgressData d)
    {
        if(d == null || d.version != 1 || d.completed == null || d.completed.Length != 3 ||
           d.bestScores == null || d.bestScores.Length != 3 || d.highestUnlocked < 0 ||
           d.highestUnlocked > 2 || d.currentLevel < 0 || d.currentLevel > d.highestUnlocked ||
           float.IsNaN(d.volume) || float.IsInfinity(d.volume) || d.volume < 0 || d.volume > 1) return false;
        int expectedUnlocked = 0;
        for(int i=0; i<3; i++) {
            if(d.bestScores[i] < 0) return false;
            if(i > 0 && d.completed[i] && !d.completed[i-1]) return false;
            if(d.completed[i]) expectedUnlocked = Math.Min(2, i+1);
        }
        return d.highestUnlocked == expectedUnlocked;
    }
    public static bool Save()
    {
        EnsureLoaded();
        try {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            string temp = FilePath + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(Data, true));
            if(File.Exists(FilePath)) File.Replace(temp, FilePath, null);
            else File.Move(temp, FilePath);
            LastError = null;
            return true;
        }
        catch(Exception e) when(e is IOException || e is UnauthorizedAccessException || e is NotSupportedException) {
            LastError = "Progress could not be saved. Check available storage.";
            Debug.LogWarning("Sakura save: " + e.Message);
            return false;
        }
    }
    public static bool PlayLevel(int index)
    {
        EnsureLoaded();
        if(index < 0 || index > Data.highestUnlocked || index >= Scenes.Length) return false;
        SelectLevel(index);
        Time.timeScale = 1;
        SceneManager.LoadScene(Scenes[index]);
        return true;
    }
    public static void SelectLevel(int index)
    {
        EnsureLoaded();
        if(index >= 0 && index <= Data.highestUnlocked) { Data.currentLevel = index; Save(); }
    }
    public static void Complete(int index, int score)
    {
        EnsureLoaded();
        // Opening a locked scene directly in the Editor must not create inconsistent progress.
        if(index < 0 || index > Data.highestUnlocked || index >= Scenes.Length) return;
        Data.completed[index] = true;
        Data.bestScores[index] = Math.Max(Data.bestScores[index], Math.Max(0,score));
        Data.highestUnlocked = Math.Max(Data.highestUnlocked, Math.Min(2,index+1));
        Data.currentLevel = Math.Min(2,index+1);
        Save(); Changed?.Invoke();
    }
    public static void SetVolume(float value)
    {
        EnsureLoaded();
        if(float.IsNaN(value) || float.IsInfinity(value)) return;
        Data.volume = Mathf.Clamp01(value);
        AudioListener.volume = 1f;
        Save(); Changed?.Invoke();
    }
    static float CleanVolume(float v, float fallback) => float.IsNaN(v) || float.IsInfinity(v) || v < 0 || v > 1 ? fallback : v;
    public static void SetAudioVolume(bool music, float value) {
        EnsureLoaded();
        if(float.IsNaN(value) || float.IsInfinity(value)) return;
        if(music) Data.musicVolume = Mathf.Clamp01(value); else Data.sfxVolume = Mathf.Clamp01(value);
        Save(); Changed?.Invoke();
    }
    public static void ResetProgress()
    {
        Data = Defaults(); AudioListener.volume = 1f;
        Save(); Changed?.Invoke();
    }
}

