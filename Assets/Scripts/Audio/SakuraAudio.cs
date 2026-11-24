using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// One persistent audio service. Each overlapping effect owns a separate voice.
public sealed class SakuraAudio : MonoBehaviour
{
    public static SakuraAudio Instance { get; private set; }
    public AudioMixer mixer;
    public AudioMixerGroup musicGroup, sfxGroup;
    public AudioMixerSnapshot normal, paused;
    public AudioClip menuMusic, gameMusic;
    public AudioClip[] effects;
    public enum Cue { Jump, Hit, Pickup, Windup, Death, Click, Victory, Defeat, Shot, Attack, Land, Portal }
    AudioSource[] voices;
    AudioSource[] music;
    AudioClip wanted;
    Coroutine fade;
    bool ready;
#if UNITY_EDITOR
    public event System.Action<Cue> CuePlayed;
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Instance = null; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap() {
        if (!Instance) Instantiate(Resources.Load<GameObject>("SakuraAudio"));
    }
    void Awake() {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
        mixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
        music = new[] { Voice("Music A", musicGroup), Voice("Music B", musicGroup) };
        foreach (var source in music) { source.loop = true; source.volume = 0; }
        voices = new AudioSource[20];
        for (int i=0;i<voices.Length;i++) voices[i] = Voice("SFX " + i, sfxGroup);
        SaveSystem.EnsureLoaded();
        SaveSystem.Changed += ApplyVolumes;
        SceneManager.sceneLoaded += SceneLoaded;
    }
    AudioSource Voice(string label, AudioMixerGroup group) {
        var child = new GameObject(label); child.transform.SetParent(transform);
        var source = child.AddComponent<AudioSource>(); source.playOnAwake = false;
        source.outputAudioMixerGroup = group; source.spatialBlend = 0;
        return source;
    }
    void Start() { ready = true; ApplyVolumes(); ChooseMusic(SceneManager.GetActiveScene().name); }
    void SceneLoaded(Scene scene, LoadSceneMode mode) {
        SetPaused(false); ChooseMusic(scene.name);
        foreach (var source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!source.outputAudioMixerGroup) source.outputAudioMixerGroup = sfxGroup;
    }
    public static float ToDecibels(float value) => 20f * Mathf.Log10(Mathf.Max(value, 0.0001f));
    public void ApplyVolumes() {
        if (!ready) return;
        AudioListener.volume = 1f;
        mixer.SetFloat("MasterVolume", ToDecibels(SaveSystem.Data.volume));
        mixer.SetFloat("MusicVolume", ToDecibels(SaveSystem.Data.musicVolume));
        mixer.SetFloat("SFXVolume", ToDecibels(SaveSystem.Data.sfxVolume));
    }
    public void ChooseMusic(string scene) {
        var clip = scene == "MainMenu" ? menuMusic : gameMusic;
        if (wanted == clip) return;
        wanted = clip;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Crossfade(clip));
    }
    IEnumerator Crossfade(AudioClip clip) {
        int target = music[0].clip == clip ? 0 : music[1].clip == clip ? 1 : music[0].volume <= music[1].volume ? 0 : 1;
        if (music[target].clip != clip) { music[target].clip = clip; music[target].Play(); }
        else if (!music[target].isPlaying) music[target].Play();
        float a = music[0].volume, b = music[1].volume;
        for (float t=0;t<1;t+=Time.unscaledDeltaTime) {
            music[0].volume = Mathf.Lerp(a,target==0 ? .65f : 0,t);
            music[1].volume = Mathf.Lerp(b,target==1 ? .65f : 0,t);
            yield return null;
        }
        music[target].volume = .65f; music[1-target].volume = 0;
        music[1-target].Stop(); fade = null;
    }
    public static void Play(Cue cue) {
        if (Instance) Instance.PlayCue(cue);
    }
    void PlayCue(Cue cue) {
        var clip = effects[(int)cue];
        foreach (var source in voices) if (!source.isPlaying) {
            source.pitch = cue == Cue.Victory || cue == Cue.Defeat || cue == Cue.Click ? 1f : Random.Range(.92f,1.08f);
            source.PlayOneShot(clip,.7f);
#if UNITY_EDITOR
            CuePlayed?.Invoke(cue);
#endif
            return;
        }
        // If all voices are busy, retain current sounds instead of changing their pitch.
    }
    public void SetPaused(bool value) { (value ? paused : normal).TransitionTo(.3f); }
    void OnDestroy() {
        if (Instance != this) return;
        SaveSystem.Changed -= ApplyVolumes; SceneManager.sceneLoaded -= SceneLoaded; Instance = null;
    }
}

