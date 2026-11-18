using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Assignment11Setup
{
    static readonly string[] Names = { "MainMenu", "Level1", "Level2", "BossFight" };
    static TMP_FontAsset font;
    static Color Rose = new Color32(113,75,90,255), Pink = new Color32(244,167,187,255), Pale = new Color32(255,228,236,255);
    public static void Run()
    {
        Directory.CreateDirectory("Logs");
        try {
            Directory.CreateDirectory("Assets/Assignment11/Data");
            for(int index=0;index<Names.Length;index++) {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/"+Names[index]+".unity");
                var context = UnityEngine.Object.FindAnyObjectByType<LevelContext>();
                if(context == null) context = new GameObject("Assignment11 - Level and Saves").AddComponent<LevelContext>();
                if(index>0) {
                    string assetPath="Assets/Assignment11/Data/Level"+index+"Data.asset";
                    var data=AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                    if(data == null) {
                        data=ScriptableObject.CreateInstance<LevelData>();
                        data.levelIndex=index-1; data.sceneName=Names[index];
                        Capture(data);
                        data.enemyScore=100; data.bossScore=500;
                        data.enemy.contactDamage=1; data.hazardSpawner.initialDelay=2; data.heartPickup.healAmount=1;
                        // Preserve movement/jump and scene geometry. Increase combat pressure only.
                        data.enemy.maxHealth=index==1?1:2;
                        data.enemy.moveSpeed=index==1?1.5f:2f;
                        data.hazardSpawner.spawnInterval=index==1?3f:index==2?2.5f:2f;
                        AssetDatabase.CreateAsset(data,assetPath);
                    }
                    context.levelData=data;
                    EditorUtility.SetDirty(context);
                }
                if(index==0) ConfigureMenu();
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            EditorBuildSettings.scenes=Names.Select(n=>new EditorBuildSettingsScene("Assets/Scenes/"+n+".unity",true)).ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            File.WriteAllText("Logs/Assignment11-setup.txt","PASS: four scenes saved, three LevelData assets connected, menu buttons wired.");
        } catch(Exception e) { File.WriteAllText("Logs/Assignment11-setup.txt",e.ToString()); throw; }
    }
    static T Source<T>() where T:Component
    {
        var found=UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault();
        if(found != null) return found;
        foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets"})) {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            found=prefab.GetComponentInChildren<T>(true);
            if(found != null) return found;
        }
        return null;
    }
    static void Capture(LevelData data)
    {
        { var source=Source<PlayerMovement>(); if(source != null) {
            data.playerMovement.moveSpeed=source.moveSpeed;
            data.playerMovement.jumpForce=source.jumpForce;
            data.playerMovement.shootCooldown=source.shootCooldown;
            data.playerMovement.bulletSpawnDelay=source.bulletSpawnDelay;
        } }
        { var source=Source<PlayerHealth>(); if(source != null) {
            data.playerHealth.maxHealth=source.maxHealth;
            data.playerHealth.invulnerabilityTime=source.invulnerabilityTime;
            data.playerHealth.deathDelay=source.deathDelay;
        } }
        { var source=Source<Enemy>(); if(source != null) {
            data.enemy.maxHealth=source.maxHealth;
            data.enemy.moveSpeed=source.moveSpeed;
            data.enemy.moveDistance=source.moveDistance;
            data.enemy.idleTime=source.idleTime;
            data.enemy.deathDelay=source.deathDelay;
            data.enemy.contactDamage=source.contactDamage;
        } }
        { var source=Source<Bullet>(); if(source != null) {
            data.bullet.speed=source.speed;
            data.bullet.lifetime=source.lifetime;
            data.bullet.damage=source.damage;
            data.bullet.hitAnimationDuration=source.hitAnimationDuration;
        } }
        { var source=Source<BossController>(); if(source != null) {
            data.bossController.maxHealth=source.maxHealth;
            data.bossController.contactDamage=source.contactDamage;
            data.bossController.attackInterval=source.attackInterval;
            data.bossController.attackAnimationDelay=source.attackAnimationDelay;
            data.bossController.phaseTwoHealthRatio=source.phaseTwoHealthRatio;
            data.bossController.phaseTwoAttackInterval=source.phaseTwoAttackInterval;
            data.bossController.phaseTwoProjectileSpeedMultiplier=source.phaseTwoProjectileSpeedMultiplier;
            data.bossController.deathAnimationDuration=source.deathAnimationDuration;
        } }
        { var source=Source<BossProjectile>(); if(source != null) {
            data.bossProjectile.speed=source.speed;
            data.bossProjectile.lifetime=source.lifetime;
            data.bossProjectile.damage=source.damage;
        } }
        { var source=Source<Hazard>(); if(source != null) {
            data.hazard.damage=source.damage;
            data.hazard.lifetime=source.lifetime;
        } }
        { var source=Source<HazardSpawner>(); if(source != null) {
            data.hazardSpawner.spawnInterval=source.spawnInterval;
            data.hazardSpawner.spawnHeight=source.spawnHeight;
            data.hazardSpawner.minX=source.minX;
            data.hazardSpawner.maxX=source.maxX;
            data.hazardSpawner.initialDelay=source.initialDelay;
        } }
        { var source=Source<Bomb>(); if(source != null) {
            data.bomb.explosionRadius=source.explosionRadius;
            data.bomb.fuseDuration=source.fuseDuration;
            data.bomb.explosionDuration=source.explosionDuration;
        } }
        { var source=Source<Rock>(); if(source != null) {
            data.rock.breakAnimationDuration=source.breakAnimationDuration;
        } }
        { var source=Source<HeartPickup>(); if(source != null) {
            data.heartPickup.healAmount=source.healAmount;
        } }

    }
    static void ConfigureMenu()
    {
        var canvas=UnityEngine.Object.FindObjectsByType<Canvas>().First(c=>c.isRootCanvas);
        font=canvas.GetComponentsInChildren<TMP_Text>(true).Select(t=>t.font).FirstOrDefault(f=>f!=null) ?? TMP_Settings.defaultFontAsset;
        var menu=UnityEngine.Object.FindAnyObjectByType<MainMenu>();
        var ui=menu.GetComponent<ProgressMenu>() ?? menu.gameObject.AddComponent<ProgressMenu>();
        var safe=canvas.transform.Find("SafeArea") ?? canvas.transform;
        var card=safe.Find("Card");
        if(card == null) throw new Exception("Existing menu Card missing.");
        foreach(var child in card.GetComponentsInChildren<Button>(true)) child.gameObject.SetActive(false);
        foreach(var child in card.GetComponentsInChildren<TMP_Text>(true)) child.gameObject.SetActive(false);
        ((RectTransform)card).sizeDelta=new Vector2(760,900);
        card.GetComponent<Image>().color=Pale;
        Label(card,"SakuraTitle","SAKURA",350,60,650,62);
        Label(card,"AssignmentSubtitle","A 2D platform adventure",270,34);
        Button(card,"Continue","PLAY / CONTINUE",150,menu.PlayGame);
        Button(card,"LevelSelect","LEVEL SELECT",25,ui.OpenLevels);
        Button(card,"Settings","SETTINGS",-100,ui.OpenSettings);
        Button(card,"Quit11","QUIT",-225,menu.QuitGame);
        ui.statusLabel=Label(card,"SaveStatus","Progress saved automatically",-350,25,680,90);
        var levels=Overlay(canvas.transform,"LevelSelection","SELECT LEVEL");
        ui.levelsPanel=levels.parent.gameObject;
        ui.levelButtons=new Button[3]; ui.levelLabels=new TMP_Text[3];
        UnityEngine.Events.UnityAction[] actions={ui.Level1,ui.Level2,ui.Level3};
        for(int i=0;i<3;i++) {
            ui.levelButtons[i]=Button(levels,"Level"+(i+1),"Level "+(i+1),140-i*140,actions[i]);
            ui.levelLabels[i]=ui.levelButtons[i].GetComponentInChildren<TMP_Text>();
        }
        Button(levels,"Back","BACK",-300,ui.CloseLevels);
        var settings=Overlay(canvas.transform,"SettingsPanel","SETTINGS");
        ui.settingsPanel=settings.parent.gameObject;
        ui.volumeLabel=Label(settings,"VolumeLabel","Master volume",160,40);
        var track=Rect(settings,"VolumeSlider",40,600,65);
        track.gameObject.AddComponent<Image>().color=Pink;
        var slider=track.gameObject.AddComponent<Slider>();
        var fill=Rect(track,"Fill",0,0,0); Stretch(fill);
        fill.gameObject.AddComponent<Image>().color=Rose;
        var handle=Rect(track,"Handle",0,50,90);
        var hi=handle.gameObject.AddComponent<Image>();hi.color=Color.white;
        slider.fillRect=fill;slider.handleRect=handle;slider.targetGraphic=hi;
        slider.minValue=0;slider.maxValue=1;slider.value=1;ui.volumeSlider=slider;
        UnityEventTools.AddPersistentListener(slider.onValueChanged,ui.VolumeChanged);
        Button(settings,"ResetProgress","RESET PROGRESS",-130,ui.RequestReset);
        Button(settings,"Back","BACK",-300,ui.CloseSettings);
        var confirm=Overlay(canvas.transform,"ResetConfirmation","Reset all progress?");
        ui.confirmPanel=confirm.parent.gameObject;
        Label(confirm,"ResetExplanation","Levels, best scores and volume will reset.",90,32,700,150);
        Button(confirm,"YesReset","YES, RESET",-80,ui.ConfirmReset);
        Button(confirm,"Cancel","CANCEL",-240,ui.CancelReset);
        ui.levelsPanel.SetActive(false);ui.settingsPanel.SetActive(false);ui.confirmPanel.SetActive(false);
        EditorUtility.SetDirty(ui);
    }
    static RectTransform Rect(Transform parent,string name,float y,float w=620,float h=100)
    {
        var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);
        var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);
        r.sizeDelta=new Vector2(w,h);r.anchoredPosition=new Vector2(0,y);return r;
    }
    static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero; }
    static TMP_Text Label(Transform parent,string name,string value,float y,float size,float w=680,float h=80)
    {
        var r=Rect(parent,name,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font=font;t.text=value;t.color=Rose;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=22;t.fontSizeMax=size;
        t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;
    }
    static Button Button(Transform parent,string name,string value,float y,UnityEngine.Events.UnityAction action)
    {
        var r=Rect(parent,name,y);var image=r.gameObject.AddComponent<Image>();image.color=Pink;
        var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;
        var colors=b.colors;colors.disabledColor=new Color(.65f,.65f,.65f,1);b.colors=colors;
        var label=Label(r,"Label",value,0,36,600,90);Stretch(label.rectTransform);
        UnityEventTools.AddPersistentListener(b.onClick,action);return b;
    }
    static RectTransform Overlay(Transform parent,string name,string title)
    {
        var old=parent.Find(name);if(old!=null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root=Rect(parent,name,0);Stretch(root);root.gameObject.AddComponent<Image>().color=new Color(.15f,.06f,.1f,.8f);
        var card=Rect(root,"Panel",0,820,900);card.gameObject.AddComponent<Image>().color=Pale;
        Label(card,"Title",title,350,50,760,120);return card;
    }
}
