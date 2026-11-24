#if UNITY_EDITOR
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

public sealed class SakuraAudioTestRunner:MonoBehaviour
{
    int passed;
    void Awake() { Application.runInBackground=true; }
    void Check(bool value,string label) {
        File.AppendAllText("Logs/Assignment12-tests.txt",(value?"PASS ":"FAIL ")+label+"\n");
        if(!value) throw new Exception(label); passed++;
    }
    IEnumerator Start() {
        DontDestroyOnLoad(gameObject);
        var tests=Checks();
        while(true) {
            object next=null; bool more=false;
            try { more=tests.MoveNext(); if(more) next=tests.Current; }
            catch(Exception e) { File.AppendAllText("Logs/Assignment12-tests.txt",e+"\n"); Finish(1); yield break; }
            if(!more) break;
            yield return next;
        }
        File.AppendAllText("Logs/Assignment12-tests.txt", "Completed "+passed+" assertions. No physical mobile or listening test implied.\n"); Finish(0);
    }
    void Finish(int code) {
        Time.timeScale=1; SessionState.SetBool("SakuraAudioTesting",false);
        if(Application.isBatchMode) EditorApplication.Exit(code); else EditorApplication.isPlaying=false;
    }
    static object Invoke(object target,string name,params object[] args) => target.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance).Invoke(target,args);
    IEnumerator Checks() {
        yield return null; yield return new WaitForSecondsRealtime(1.4f);
        var audio=SakuraAudio.Instance;
        Check(audio!=null,"persistent service bootstrapped");
        Check(Object.FindObjectsByType<SakuraAudio>(FindObjectsSortMode.None).Length==1,"one audio manager");
        Check(audio.effects.Length==12 && audio.effects.All(c=>c && c.samples>0),"12 populated effect clips");
        Check(audio.menuMusic!=audio.gameMusic && audio.menuMusic.length>30 && audio.gameMusic.length>30,"two distinct full music loops");
        Check(audio.normal && audio.paused,"both real mixer snapshots linked");
        Check(Mathf.Abs(SakuraAudio.ToDecibels(0)+80)<.01 && Mathf.Abs(SakuraAudio.ToDecibels(.5f)+6.0206f)<.001,"logarithmic volume and mute");
        SaveSystem.TestPath=Path.GetFullPath("Logs/Assignment12TestSave/progress.json");
        Directory.CreateDirectory(Path.GetDirectoryName(SaveSystem.TestPath));
        File.WriteAllText(SaveSystem.TestPath,"{\"version\":1,\"highestUnlocked\":1,\"currentLevel\":1,\"completed\":[true,false,false],\"bestScores\":[700,0,0],\"volume\":0.6}");
        SaveSystem.Reload();
        Check(SaveSystem.Data.highestUnlocked==1 && SaveSystem.Data.bestScores[0]==700,"old save migration preserves progress");
        Check(SaveSystem.Data.musicVolume==.75f && SaveSystem.Data.sfxVolume==.85f,"missing audio fields get defaults");
        var menu=Object.FindAnyObjectByType<ProgressMenu>(); menu.OpenSettings();
        var ui=menu.settingsPanel.GetComponent<SakuraVolumeSettings>();
        Check(ui.musicSlider.value==.75f && ui.sfxSlider.value==.85f,"settings show loaded values");
        ui.musicSlider.value=.32f; ui.sfxSlider.value=.48f; menu.volumeSlider.value=.64f;
        yield return null;
        audio.mixer.GetFloat("MusicVolume",out float musicDb); audio.mixer.GetFloat("SFXVolume",out float sfxDb);
        Check(Mathf.Abs(musicDb-SakuraAudio.ToDecibels(.32f))<.01 && Mathf.Abs(sfxDb-SakuraAudio.ToDecibels(.48f))<.01,"sliders independently control mixer in decibels");
        SaveSystem.Reload();
        Check(SaveSystem.Data.musicVolume==.32f && SaveSystem.Data.sfxVolume==.48f && SaveSystem.Data.volume==.64f,"three volumes survive disk reload");
        Check(SaveSystem.Data.bestScores[0]==700,"volume saves retain scores");
        for(int i=0;i<3;i++) { menu.CloseSettings(); menu.OpenSettings(); }
        Check(ui.musicSlider.value==.32f,"reopened settings retain values");
        menu.CloseSettings();
        foreach(SakuraAudio.Cue cue in Enum.GetValues(typeof(SakuraAudio.Cue))) SakuraAudio.Play(cue);
        yield return null;
        var voices=audio.GetComponentsInChildren<AudioSource>();
        Check(voices.Count(s=>s.outputAudioMixerGroup==audio.sfxGroup && s.isPlaying)>=8,"mandatory cue assets play on separate voices");
        Check(voices.Where(s=>s.outputAudioMixerGroup==audio.sfxGroup).All(s=>s.pitch>=.92f && s.pitch<=1.08f),"effect pitch range");
        SceneManager.LoadScene("Level1"); yield return null; yield return new WaitForSecondsRealtime(1.3f);
        Check(SakuraAudio.Instance==audio,"service survives menu to gameplay");
        var track=audio.GetComponentsInChildren<AudioSource>().Single(s=>s.clip==audio.gameMusic && s.isPlaying);
        Check(track.volume>.6f,"gameplay crossfade completes");
        float time=track.time;
        GameManager.Instance.SetPaused(true); yield return new WaitForSecondsRealtime(.5f);
        Check(Time.timeScale==0 && track.time>time,"music continues during timeScale zero");
        audio.mixer.GetFloat("MusicVolume",out musicDb);
        Check(Mathf.Abs(musicDb-SakuraAudio.ToDecibels(.32f))<.01,"pause preserves user gain");
        GameManager.Instance.SetPaused(false); yield return new WaitForSecondsRealtime(.4f);
        Check(Time.timeScale==1,"resume restores game time");
        foreach(string scene in new[]{"Level2","BossFight","Level1"}) {
            time=track.time; SceneManager.LoadScene(scene); yield return null; yield return new WaitForSecondsRealtime(.15f);
            Check(track.isPlaying && track.time>=time,"music position retained on "+scene);
            Check(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.enabled)==1,"one listener in "+scene);
            Check(Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).All(s=>s.outputAudioMixerGroup),"all sources routed in "+scene);
            Check(Object.FindObjectsByType<MobileController>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length>0,"mobile controls retained in "+scene);
        }
        // Exercise actual boss attack coroutine and timestamp emitted audio.
        SceneManager.LoadScene("BossFight"); yield return null; yield return new WaitForSecondsRealtime(.2f);
        var boss=Object.FindAnyObjectByType<BossController>(); boss.enabled=false;
        var calls=new System.Collections.Generic.List<System.Tuple<SakuraAudio.Cue,float>>();
        System.Action<SakuraAudio.Cue> record=cue=>calls.Add(System.Tuple.Create(cue,Time.time));
        audio.CuePlayed+=record;
        boss.StartCoroutine((IEnumerator)Invoke(boss,"Attack"));
        yield return new WaitForSeconds(.8f);
        var warning=calls.FirstOrDefault(c=>c.Item1==SakuraAudio.Cue.Windup);
        var attack=calls.FirstOrDefault(c=>c.Item1==SakuraAudio.Cue.Attack);
        Check(warning!=null && attack!=null && attack.Item2-warning.Item2>=.44f,"actual boss warning precedes projectile by at least 0.45 seconds");
        audio.CuePlayed-=record;
        SceneManager.LoadScene("Level1"); yield return null; yield return new WaitForSeconds(.5f);
        var actor=Object.FindAnyObjectByType<Enemy>(); var hero=Object.FindAnyObjectByType<PlayerHealth>();
        calls.Clear(); audio.CuePlayed+=record;
        actor.StartCoroutine((IEnumerator)Invoke(actor,"ContactAttack",hero));
        yield return new WaitForSeconds(.1f);
        Check(calls.Count(c=>c.Item1==SakuraAudio.Cue.Windup)==1 && !calls.Any(c=>c.Item1==SakuraAudio.Cue.Attack),"contact attack warns once before resolution");
        yield return new WaitForSeconds(.4f);
        audio.CuePlayed-=record;
        // Trigger real gameplay methods, with isolated progress only.
        var health=Object.FindAnyObjectByType<PlayerHealth>();
        health.TakeDamage(1); yield return new WaitForSecondsRealtime(.05f);
        Check(voices.Any(s=>s.outputAudioMixerGroup==audio.sfxGroup && s.isPlaying),"actual damage triggers sound");
        GameManager.Instance.FinishLevel(); yield return null;
        Check(Time.timeScale==0,"actual victory path remains functional");
        SceneManager.LoadScene("Level1"); yield return null;
        GameManager.Instance.GameOver(); yield return null;
        Check(Time.timeScale==0,"actual defeat path remains functional");
        Time.timeScale=1;
        SaveSystem.SetAudioVolume(true,.32f); SaveSystem.SetAudioVolume(false,.48f);
        SaveSystem.Save();
    }
}

#endif

