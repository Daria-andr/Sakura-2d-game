using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class LevelContext : MonoBehaviour
{
    public LevelData levelData;
    public static LevelData Current { get; private set; }
    void Awake()
    {
        Current = levelData;
        SaveSystem.EnsureLoaded();
        if (levelData != null) SaveSystem.SelectLevel(levelData.levelIndex);
    }
    void OnApplicationPause(bool paused) { if(paused) SaveSystem.Save(); }
    void OnApplicationFocus(bool focused) { if(!focused) SaveSystem.Save(); }
    void OnApplicationQuit() { SaveSystem.Save(); }
    void OnDestroy() { if(Current == levelData) Current = null; }
}
