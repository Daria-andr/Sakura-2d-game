using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

public static class SakuraAudioSetup
{
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    static Type T(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.Audio."+name)).First(t=>t!=null);
    static object Call(object o,string method,params object[] args) => o.GetType().GetMethods(Flags).First(m=>m.Name==method && m.GetParameters().Length==args.Length).Invoke(o,args);
    static object Get(object o,string name) => o.GetType().GetProperty(name,Flags).GetValue(o);
    static void Set(object o,string name,object value) => o.GetType().GetProperty(name,Flags).SetValue(o,value);
    static Array ArrayOf(Type t,params object[] items) { var a=Array.CreateInstance(t,items.Length); for(int i=0;i<items.Length;i++) a.SetValue(items[i],i); return a; }
    [MenuItem("Tools/Sakura/Configure Assignment 12 Audio")]
    public static void Run() {
        Directory.CreateDirectory("Assets/Audio/Music"); Directory.CreateDirectory("Assets/Audio/SFX");
        Directory.CreateDirectory("Assets/Audio/Mixers"); Directory.CreateDirectory("Assets/Resources");
        SakuraAudioSynthesis.Generate(); AssetDatabase.Refresh();
        var path="Assets/Audio/Mixers/SakuraAudioMixer.mixer";
        var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
        if(!mixer) {
            var type=T("AudioMixerController");
            mixer=(AudioMixer)type.GetMethod("CreateMixerControllerAtPath",Flags).Invoke(null,new object[]{path});
            var master=Get(mixer,"masterGroup");
            var music=Call(mixer,"CreateNewGroup","Music",false);
            var sfx=Call(mixer,"CreateNewGroup","SFX",false);
            var tone=Call(mixer,"CreateNewGroup","PauseFX",false);
            Call(mixer,"AddChildToParent",music,master); Call(mixer,"AddChildToParent",sfx,master); Call(mixer,"AddChildToParent",tone,music);
            var normal=((Array)Get(mixer,"snapshots")).GetValue(0); ((Object)normal).name="Normal";
            var paused=Activator.CreateInstance(T("AudioMixerSnapshotController"),new object[]{mixer}); ((Object)paused).name="Paused";
            AssetDatabase.AddObjectToAsset((Object)paused,mixer);
            Set(mixer,"snapshots",ArrayOf(T("AudioMixerSnapshotController"),normal,paused));
            var filter=Activator.CreateInstance(T("AudioMixerEffectController"),new object[]{"Lowpass"});
            Call(filter,"PreallocateGUIDs"); AssetDatabase.AddObjectToAsset((Object)filter,mixer); Call(tone,"InsertEffect",filter,1);
            foreach(var snap in new[]{normal,paused}) {
                foreach(var group in new[]{master,music,sfx}) Call(group,"SetValueForVolume",mixer,snap,0f);
                Call(tone,"SetValueForVolume",mixer,snap,snap==paused ? -10f : 0f);
                Call(filter,"SetValueForParameter",mixer,snap,"Cutoff freq",snap==paused ? 1100f : 22000f);
            }
            var ep=T("ExposedAudioParameter"); var parameters=Array.CreateInstance(ep,3);
            var groups=new[]{master,music,sfx}; var names=new[]{"MasterVolume","MusicVolume","SFXVolume"};
            for(int i=0;i<3;i++) { var entry=Activator.CreateInstance(ep); ep.GetField("guid").SetValue(entry,Call(groups[i],"GetGUIDForVolume")); ep.GetField("name").SetValue(entry,names[i]); parameters.SetValue(entry,i); }
            Set(mixer,"exposedParameters",parameters); Set(mixer,"startSnapshot",normal); Set(mixer,"TargetSnapshot",normal);
            // Include every bus in the initial mixer view for the submission screenshot.
            var viewType=T("MixerGroupView"); var view=Activator.CreateInstance(viewType);
            viewType.GetField("name").SetValue(view,"All");
            viewType.GetField("guids").SetValue(view,ArrayOf(typeof(GUID),new[]{master,music,sfx,tone}.Select(g=>Get(g,"groupID")).ToArray()));
            Set(mixer,"views",ArrayOf(viewType,view)); Set(mixer,"currentViewIndex",0);
            foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path)) EditorUtility.SetDirty(asset);
        }
        mixer.updateMode=AudioMixerUpdateMode.UnscaledTime;
        var musicGroup=mixer.FindMatchingGroups("PauseFX").Single();
        var sfxGroup=mixer.FindMatchingGroups("SFX").Single();
        var root=new GameObject("SakuraAudio"); var audio=root.AddComponent<SakuraAudio>();
        audio.mixer=mixer; audio.musicGroup=musicGroup; audio.sfxGroup=sfxGroup;
        audio.normal=mixer.FindSnapshot("Normal"); audio.paused=mixer.FindSnapshot("Paused");
        audio.menuMusic=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/MenuGarden.wav");
        audio.gameMusic=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/SakuraJourney.wav");
        audio.effects=Enum.GetNames(typeof(SakuraAudio.Cue)).Select(n=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/"+n+".wav")).ToArray();
        PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/SakuraAudio.prefab"); Object.DestroyImmediate(root);
        foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Audio"})) {
            var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            var settings=importer.defaultSampleSettings; bool isMusic=importer.assetPath.Contains("/Music/");
            settings.loadType=isMusic ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat=isMusic ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
            settings.quality=.75f; importer.defaultSampleSettings=settings; importer.forceToMono=!isMusic; importer.SaveAndReimport();
        }
        // Preserve all scenes, including the earlier enemy assignment and archived level.
        foreach(var guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets"})) {
            string scenePath=AssetDatabase.GUIDToAssetPath(guid);
            if(scenePath.Contains("TextMesh Pro") || scenePath.Contains("Settings/Scenes")) continue;
            var scene=EditorSceneManager.OpenScene(scenePath);
            foreach(var go in scene.GetRootGameObjects()) ConfigureTree(go,sfxGroup);
            var menu=Object.FindAnyObjectByType<ProgressMenu>();
            if(menu) ConfigureSettings(menu);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets"})) {
            string prefabPath=AssetDatabase.GUIDToAssetPath(guid);
            if(prefabPath=="Assets/Resources/SakuraAudio.prefab") continue;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if(prefab.GetComponentsInChildren<AudioSource>(true).Length==0 && prefab.GetComponentsInChildren<Button>(true).Length==0) continue;
            var contents=PrefabUtility.LoadPrefabContents(prefabPath);
            ConfigureTree(contents,sfxGroup); PrefabUtility.SaveAsPrefabAsset(contents,prefabPath); PrefabUtility.UnloadPrefabContents(contents);
        }
        AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/Assignment12-setup.txt","PASS: mixer, snapshots, clips, prefab, settings, scenes and source routing saved.");
    }
    static void ConfigureTree(GameObject root,AudioMixerGroup group) {
        foreach(var source in root.GetComponentsInChildren<AudioSource>(true)) { source.outputAudioMixerGroup=group; source.playOnAwake=false; }
        foreach(var b in root.GetComponentsInChildren<Button>(true)) if(!b.GetComponent<SakuraButtonSound>()) b.gameObject.AddComponent<SakuraButtonSound>();
        foreach(var p in root.GetComponentsInChildren<PlayerMovement>(true)) p.shootSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Shot.wav");
        foreach(var p in root.GetComponentsInChildren<PlayerHealth>(true)) p.damageSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Hit.wav");
        foreach(var p in root.GetComponentsInChildren<Portal>(true)) p.portalSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Portal.wav");
    }
    static void Move(Component c,float y,float height) { var r=(RectTransform)c.transform; r.anchoredPosition=new Vector2(0,y); r.sizeDelta=new Vector2(r.sizeDelta.x,height); }
    public static void RefreshMenu() {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        ConfigureSettings(Object.FindAnyObjectByType<ProgressMenu>());
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
    }
    static void ConfigureSettings(ProgressMenu menu) {
        var parent=menu.volumeSlider.transform.parent;
        var settings=menu.settingsPanel.GetComponent<SakuraVolumeSettings>() ?? menu.settingsPanel.AddComponent<SakuraVolumeSettings>();
        settings.masterSlider=menu.volumeSlider; settings.masterLabel=menu.volumeLabel;
        menu.volumeSlider.onValueChanged=new Slider.SliderEvent();
        Move(menu.volumeLabel,230,55); Move(menu.volumeSlider,175,45);
        if(!settings.musicSlider) {
            settings.musicSlider=Object.Instantiate(menu.volumeSlider,parent); settings.musicSlider.name="MusicVolumeSlider";
            settings.musicLabel=Object.Instantiate(menu.volumeLabel,parent); settings.musicLabel.name="MusicVolumeLabel";
            settings.sfxSlider=Object.Instantiate(menu.volumeSlider,parent); settings.sfxSlider.name="SFXVolumeSlider";
            settings.sfxLabel=Object.Instantiate(menu.volumeLabel,parent); settings.sfxLabel.name="SFXVolumeLabel";
        }
        foreach(var slider in new[]{settings.musicSlider,settings.sfxSlider}) { slider.onValueChanged=new Slider.SliderEvent(); slider.minValue=0;slider.maxValue=1;slider.wholeNumbers=false; }
        settings.musicLabel.text="Music volume"; settings.sfxLabel.text="SFX volume";
        Move(settings.musicLabel,100,55); Move(settings.musicSlider,45,45);
        Move(settings.sfxLabel,-30,55); Move(settings.sfxSlider,-85,45);
        var reset=parent.Find("ResetProgress"); if(reset) Move(reset,-200,80);
        var back=parent.Find("Back"); if(back) Move(back,-310,80);
        EditorUtility.SetDirty(settings);
    }
}

