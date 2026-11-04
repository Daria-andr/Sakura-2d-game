using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class BossSceneRepair
{
    const string Key = "Sakura.BossVisualRepair.v1";
    static BossSceneRepair() { EditorApplication.update += ApplyOnce; }
    static void ApplyOnce()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (File.Exists("Backups/BossVisualRepair/result.txt")) { EditorApplication.update -= ApplyOnce; return; }
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "BossFight") return;
        Directory.CreateDirectory("Backups/BossVisualRepair");
        if (!EditorSceneManager.SaveScene(scene, "Backups/BossVisualRepair/BossFight-before.unity", true)) return;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var animator in root.GetComponentsInChildren<Animator>(true))
            {
                if (animator.GetComponent<BossController>() != null) continue;
                if (animator.runtimeAnimatorController == null || animator.runtimeAnimatorController.name != "BossIdle") continue;
                Undo.RecordObject(animator, "Remove boss animation from background");
                animator.enabled = false;
                var renderer = animator.GetComponent<SpriteRenderer>();
                if (renderer == null) continue;
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath("467b354df6bade24aad08a24572334a3")))
                    if (asset is Sprite sprite && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long id) && id == 984725556270474758)
                    { Undo.RecordObject(renderer, "Restore background"); renderer.sprite = sprite; break; }
            }
            foreach (var boss in root.GetComponentsInChildren<BossController>(true))
            {
                Undo.RecordObject(boss.transform, "Position boss on platform");
                boss.transform.position = new Vector3(-9.62f, 2.6f, 0);
                boss.transform.localScale = new Vector3(4.5f, 4.5f, 1);
            }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) return;
        SessionState.SetBool(Key, true);
        EditorApplication.update -= ApplyOnce;
        File.WriteAllText("Backups/BossVisualRepair/result.txt", "Loaded BossFight repaired and saved.");
    }
}