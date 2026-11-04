using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class BuildBossAnimations
{
    const string Folder = "Assets/Art/BossV2/Animations";
    const string Result = "Backups/BossAnimationSetup/result.txt";
    static BuildBossAnimations() { EditorApplication.update += Run; }
    static void Run()
    {
        if (File.Exists(Result)) { EditorApplication.update -= Run; return; }
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "BossFight") return;
        EditorApplication.update -= Run;
        try { Build(); } catch (Exception ex) { File.WriteAllText("Backups/BossAnimationSetup/error.txt", ex.ToString()); Debug.LogException(ex); }
    }
    [MenuItem("Tools/Sakura/Build Boss Animations")]
    static void Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "BossFight" || EditorApplication.isPlaying) return;
        Directory.CreateDirectory("Backups/BossAnimationSetup");
        if (!EditorSceneManager.SaveScene(scene, "Backups/BossAnimationSetup/BossFight-before.unity", true)) throw new Exception("Scene backup failed");
        const string sheet = "Assets/Art/BossV2/BossV2-grid.png";
        if (!File.Exists(sheet)) File.Copy("Assets/Art/BossV2/BossV2-sheet.png", sheet);
        AssetDatabase.ImportAsset(sheet, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(sheet);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 4096;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(sheet);
        int width=texture.width, height=texture.height;
        var cells = new SpriteMetaData[36];
        for(int row=0;row<6;row++) for(int col=0;col<6;col++)
        {
            int x0=col*width/6, x1=(col+1)*width/6;
            int y0=(5-row)*height/6, y1=(6-row)*height/6;
            cells[row*6+col]=new SpriteMetaData {name="BossV2_"+(row*6+col).ToString("D2"),rect=new Rect(x0,y0,x1-x0,y1-y0),alignment=0,pivot=new Vector2(0.5f,0.5f)};
        }
#pragma warning disable 0618
        importer.spritesheet=cells;
#pragma warning restore 0618
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (sprites.Length != 36) throw new Exception("Expected 36 sliced sprites, found " + sprites.Length);
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        var path = Folder + "/BossV2.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null || controller.layers.Length == 0) {
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        if (controller.layers.Length == 0) controller.AddLayer("Base Layer");
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Phase2", AnimatorControllerParameterType.Bool);
        var sm = controller.layers[0].stateMachine;
        var idle = new AnimatorState[2];
        for (int phase = 0; phase < 2; phase++)
        {
            int row = phase * 3;
            string suffix = phase == 0 ? "" : "_Phase2";
            idle[phase] = State(sm, "Idle" + suffix, Clip("Idle"+suffix, sprites.Skip(row*6).Take(6).ToArray(), true, 8));
            var attack = State(sm, "Attack"+suffix, Clip("Attack"+suffix, sprites.Skip((row+1)*6).Take(6).ToArray(), false, 12));
            var hurt = State(sm, "Hit"+suffix, Clip("Hit"+suffix, sprites.Skip((row+2)*6).Take(2).ToArray(), false, 12));
            var death = State(sm, "Death"+suffix, Clip("Death"+suffix, sprites.Skip((row+2)*6+2).Take(4).ToArray(), false, 8));
            Any(sm, death, "Death", phase);
            Any(sm, attack, "Attack", phase);
            Any(sm, hurt, "Hit", phase);
            Return(attack, idle[phase]); Return(hurt, idle[phase]);
        }
        sm.defaultState = idle[0];
        var change = idle[0].AddTransition(idle[1]); change.hasExitTime=false; change.duration=0;
        change.AddCondition(AnimatorConditionMode.If, 0, "Phase2");
        }
        AssetDatabase.SaveAssets();
        var bosses = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BossController>(true)).ToArray();
        if (bosses.Length > 1) throw new Exception("Multiple gameplay bosses; scene left unchanged.");
        BossController boss;
        if (bosses.Length == 1) boss = bosses[0];
        else
        {
            var candidates=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<SpriteRenderer>(true))
                .Where(r=>r.sprite!=null && (AssetDatabase.GetAssetPath(r.sprite)==sheet || AssetDatabase.GetAssetPath(r.sprite)=="Assets/Art/BossV2/BossV2-sheet.png")).ToArray();
            if (candidates.Length > 1) throw new Exception("Multiple new boss visuals; refusing to choose arbitrarily.");
            var obj=candidates.Length==1 ? candidates[0].gameObject : new GameObject("BossV2");
            if(candidates.Length==0) {Undo.RegisterCreatedObjectUndo(obj,"Create boss");obj.transform.position=new Vector3(-9.62f,2.6f,0);obj.transform.localScale=new Vector3(2.5f,2.5f,1);}
            boss=Undo.AddComponent<BossController>(obj);
        }
        if (boss.GetComponent<Rigidbody2D>()==null) { var body=Undo.AddComponent<Rigidbody2D>(boss.gameObject);body.gravityScale=0;body.constraints=RigidbodyConstraints2D.FreezeAll; }
        if(boss.GetComponent<Collider2D>()==null) {var box=Undo.AddComponent<BoxCollider2D>(boss.gameObject);box.size=sprites[0].bounds.size*0.75f;}
        var bars=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<UnityEngine.UI.Slider>(true)).Where(r=>r.name=="BossHealthBar").ToArray();
        if(bars.Length==1) boss.healthBar=bars[0];
        foreach(var manager in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<GameManager>(true))) {Undo.RecordObject(manager,"Connect boss");manager.boss=boss;EditorUtility.SetDirty(manager);}
        var animator = boss.GetComponent<Animator>() ?? boss.gameObject.AddComponent<Animator>();
        Undo.RecordObject(animator, "Assign boss animations"); animator.runtimeAnimatorController = controller; animator.enabled=true;
        var renderer = boss.GetComponent<SpriteRenderer>() ?? boss.gameObject.AddComponent<SpriteRenderer>();
        Undo.RecordObject(renderer, "Assign boss sprite"); renderer.sprite=sprites[0]; renderer.color=Color.white; renderer.enabled=true;
        // Disable decorative copies of the new boss sheet; gameplay stays on one object.
        foreach (var r in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true)))
            if (r != renderer && r.sprite != null && AssetDatabase.GetAssetPath(r.sprite)=="Assets/Art/BossV2/BossV2-sheet.png")
            { Undo.RecordObject(r, "Disable duplicate boss visual"); r.enabled=false; var a=r.GetComponent<Animator>(); if(a!=null) {Undo.RecordObject(a,"Disable duplicate animator");a.enabled=false;} }
        Undo.RecordObject(boss, "Synchronize boss attack"); boss.attackAnimationDelay=0.25f; boss.deathAnimationDuration=0.6f;
        boss.projectilePrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BossProjectile.prefab");
        if (boss.shootPoint==null) { var point=new GameObject("ShootPoint"); Undo.RegisterCreatedObjectUndo(point,"Create shoot point");point.transform.SetParent(boss.transform,false); point.transform.localPosition=new Vector3(0.6f,0.15f,0);boss.shootPoint=point.transform; }
        EditorUtility.SetDirty(boss); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Scene save failed");
        if (File.Exists("Backups/BossAnimationSetup/error.txt")) File.Delete("Backups/BossAnimationSetup/error.txt");
        File.WriteAllText(Result, "Created 8 animation clips and 2-phase Animator. Connected to one boss; scene saved. Sprites: " + sprites.Length);
    }
    static AnimationClip Clip(string name, Sprite[] frames, bool loop, float fps)
    {
        var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(Folder+"/"+name+".anim");
        if(existing!=null) return existing;
        var clip=new AnimationClip {name=name,frameRate=fps};
        var keys=new ObjectReferenceKeyframe[frames.Length+1];
        for(int i=0;i<frames.Length;i++) keys[i]=new ObjectReferenceKeyframe {time=i/fps,value=frames[i]};
        keys[frames.Length]=new ObjectReferenceKeyframe {time=frames.Length/fps,value=frames[frames.Length-1]};
        AnimationUtility.SetObjectReferenceCurve(clip,new EditorCurveBinding {path="",type=typeof(SpriteRenderer),propertyName="m_Sprite"},keys);
        var settings=AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime=loop; AnimationUtility.SetAnimationClipSettings(clip,settings);
        AssetDatabase.CreateAsset(clip,Folder+"/"+name+".anim"); return clip;
    }
    static AnimatorState State(AnimatorStateMachine sm,string name,AnimationClip clip) {var state=sm.AddState(name);state.motion=clip;return state;}
    static void Any(AnimatorStateMachine sm,AnimatorState state,string trigger,int phase)
    {var t=sm.AddAnyStateTransition(state);t.duration=0;t.hasExitTime=false;t.canTransitionToSelf=false;t.AddCondition(AnimatorConditionMode.If,0,trigger);t.AddCondition(phase==0?AnimatorConditionMode.IfNot:AnimatorConditionMode.If,0,"Phase2");}
    static void Return(AnimatorState from,AnimatorState to) {var t=from.AddTransition(to);t.hasExitTime=true;t.exitTime=1;t.duration=0;}
}