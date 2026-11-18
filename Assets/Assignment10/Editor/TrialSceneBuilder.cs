using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sakura.Trial
{
    public static class TrialSceneBuilder
    {
        public const string ScenePath = "Assets/ThreeEnemyTrial/CourtyardTrial.unity";
        const string Template = "Assets/Scenes/Level1.unity";

        // Build on an independent copy so scene-local hero and HUD references survive.
        public static void Create()
        {
            if (File.Exists(ScenePath)) throw new InvalidOperationException("The trial scene already exists.");
            TrialArt.Build();
            var previous = SceneManager.GetActiveScene();
            // Open a fully imported scene asset after copying the YAML to its new path.
            File.Copy(Template, ScenePath);
            AssetDatabase.ImportAsset(ScenePath, ImportAssetOptions.ForceSynchronousImport);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            bool saved = false;
            try
            {
                SceneManager.SetActiveScene(scene);
                var roots = scene.GetRootGameObjects();
                var hero = roots.SelectMany(x => x.GetComponentsInChildren<PlayerMovement>(true)).SingleOrDefault();
                if (hero == null) throw new InvalidOperationException("No player in copy: " +
                    string.Join(", ", roots.Select(x => x.name + "[" + string.Join("/", x.GetComponents<Component>().Select(c => c == null ? "missing" : c.GetType().Name)) + "]")));
                var camera = roots.Select(x => x.GetComponent<Camera>()).Single(x => x != null);
                var manager = roots.Select(x => x.GetComponent<GameManager>()).Single(x => x != null);
                var keep = new[] { hero.gameObject, camera.gameObject, manager.gameObject,
                    hero.GetComponent<PlayerHealth>().heartUI.transform.root.gameObject };
                foreach (var root in roots)
                    if (!keep.Contains(root) && root.name != "Canvas" && root.name != "EventSystem" && root.name != "Global Light 2D")
                        UnityEngine.Object.DestroyImmediate(root);

                TrialArena.Build();
                var spawn = TrialArena.SpawnPoint;
                // The reference point is at the character's sole, not its transform origin.
                hero.transform.position = (Vector3)spawn + (hero.transform.position - hero.groundCheck.position) + Vector3.up * .05f;
                var bounds = TrialArena.CameraBounds;
                var follow = camera.GetComponent<CameraFollow>();
                follow.target = hero.transform;
                follow.autoBounds = false;
                follow.useBounds = true;
                follow.minX = bounds.xMin;
                follow.maxX = bounds.xMax;
                follow.minY = -2.6f;
                follow.maxY = 7.0f;
                follow.offset = new Vector3(1f, 0, -10f);
                camera.orthographicSize = 3.8f;
                camera.transform.position = new Vector3(spawn.x, 1.9f, -10f);

                var systems = new GameObject("Courtyard rules");
                var player = systems.AddComponent<TrialPlayer>();
                player.Bind(hero, camera);
                var objective = systems.AddComponent<TrialObjective>();
                var arenaRoots = scene.GetRootGameObjects();
                var guards = arenaRoots.SelectMany(x => x.GetComponentsInChildren<TrialEnemy>(true)).ToArray();
                if (guards.Length != 3) throw new InvalidOperationException("Expected three guard prefabs.");
                var gate = arenaRoots.SelectMany(x => x.GetComponentsInChildren<TrialGate>(true)).Single();
                objective.Configure(player, guards, gate, TrialArena.ExitPoint, manager, manager.enemyText, manager.scoreText);
                manager.enemyText.text = "Defeat the guards";
                manager.scoreText.text = "GUARDS   0 / 3";
                manager.enemyText = manager.scoreText = null;
                manager.portal = null;
                var pauseButtons = manager.pausePanel.GetComponentsInChildren<Button>(true);
                var restart = pauseButtons.Single(x => x.name == "RestartButton");
                restart.onClick = new Button.ButtonClickedEvent();
                UnityEventTools.AddPersistentListener(restart.onClick, objective.Retry);
                // A build for phone testing starts directly in this scene.
                pauseButtons.Single(x => x.name == "MenuButton").gameObject.SetActive(false);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save " + ScenePath);
                saved = true;
                AssetDatabase.SaveAssets();
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
                if (!saved) AssetDatabase.DeleteAsset(ScenePath);
            }
        }

        [MenuItem("Tools/Sakura/Build courtyard trial for Android")]
        static void BuildAndroid()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                EditorUtility.DisplayDialog("Android Build Support", "Install Android Build Support, SDK, NDK and OpenJDK for this editor in Unity Hub.", "OK");
                return;
            }
            string path = EditorUtility.SaveFilePanel("Build the courtyard trial", "", "Sakura-Courtyard", "apk");
            if (string.IsNullOrEmpty(path)) return;
            var previousBundleSetting = EditorUserBuildSettings.buildAppBundle;
            try
            {
                EditorUserBuildSettings.buildAppBundle = false;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = new[] { ScenePath }, locationPathName = path, target = BuildTarget.Android
                });
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                    Debug.LogError("Courtyard APK build: " + report.summary.result);
            }
            finally { EditorUserBuildSettings.buildAppBundle = previousBundleSetting; }
        }
    }
}
