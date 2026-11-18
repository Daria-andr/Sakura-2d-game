using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sakura.Trial
{
    public static class TrialArena
    {
        public static readonly Vector2 SpawnPoint = Vector2.zero;
        public static readonly Vector2 ExitPoint = new Vector2(31f, 0.9f);
        public static readonly Rect CameraBounds = Rect.MinMaxRect(-3f, -3f, 35f, 7f);
        const string Root = "Assets/ThreeEnemyTrial";
        const string PrefabFolder = Root + "/Prefabs";
        const string ArtFolder = Root + "/Art/Courtyard/";

        public static void Build()
        {
            var court = new GameObject("Sakura • Lantern Courtyard").transform;
            var backdrop = Group("Distant roofs and castle masonry", court);
            var route = Group("Walkway and cover", court);
            var cast = Group("Courtyard guards", court);
            Place("Dusk", backdrop, 16, 3, -50);

            // A single floor collider keeps fast movement seamless at tile boundaries.
            Tiled("Walkway", route, 16, -1, 48, 2, -2);
            Tiled("Foundation", route, 16, -6, 48, 8, -3);
            Solid("Stone walk", route, 16, -2f, 48, 4);
            for (int x = -7; x <= 37; x += 4)
            {
                var brick = Place("InlaidStone", route, x, -2.2f, -2);
                brick.transform.localScale = Vector3.one * 0.68f;
            }

            Arcade(backdrop, -2.5f, true);
            Arcade(backdrop, 10.5f, false);
            Arcade(backdrop, 20f, true);
            Arcade(backdrop, 31f, false);
            Solid("West end of court", route, -3.55f, 4f, 0.7f, 8f);
            Solid("East end of court", route, 34.55f, 4f, 0.7f, 8f);
            // These low crates interrupt sight at eye height and fit Sakura's normal jump.
            Cover(route, 10.5f);
            Cover(route, 19.25f);
            Balcony(route, 13.9f, 1.68f);
            Balcony(route, 24.8f, 1.75f);

            var gate = Place("Gate0", route, ExitPoint.x, 1.75f, -4);
            gate.name = "East gate";
            gate.AddComponent<TrialGate>().Configure(
                Enumerable.Range(0, 5).Select(i => Art("Gate" + i)).ToArray());

            if (!AssetDatabase.IsValidFolder(PrefabFolder))
                AssetDatabase.CreateFolder(Root, "Prefabs");
            Guard<BatPatroller>("Patrol", cast, 6.85f, 3.6f, 8.7f);
            Guard<CrateThrower>("Thrower", cast, 16.3f, 12.1f, 17.75f);
            Guard<CrownCharger>("Charger", cast, 26.5f, 21f, 28.3f);
        }

        static Transform Group(string name, Transform parent)
        {
            var item = new GameObject(name).transform;
            item.SetParent(parent, false);
            return item;
        }

        static Sprite Art(string name)
        {
            var path = ArtFolder + name + ".png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("Missing courtyard sprite: " + path);
            return sprite;
        }

        static GameObject Place(string name, Transform parent, float x, float y, int order)
        {
            var item = Group(name, parent);
            item.localPosition = new Vector3(x, y, 0f);
            var renderer = item.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = Art(name);
            renderer.sortingOrder = order;
            return item.gameObject;
        }

        static void Tiled(string name, Transform parent, float x, float y, float width, float height, int order)
        {
            var render = Place(name, parent, x, y, order).GetComponent<SpriteRenderer>();
            render.drawMode = SpriteDrawMode.Tiled;
            render.size = new Vector2(width, height);
        }

        static void Arcade(Transform parent, float x, bool banner)
        {
            var grouping = Group("Masonry bay", parent);
            grouping.localPosition = Vector3.right * x;
            var pillar = Place("Pillar", grouping, 0, 2.05f, -13);
            pillar.transform.localScale = new Vector3(1f, 0.8f, 1f);
            pillar.GetComponent<SpriteRenderer>().color = new Color(0.85f, 0.76f, 0.81f);
            if (banner)
            {
                var silk = Place("Pennant", grouping, 0f, 3.15f, -10);
                silk.transform.localScale = Vector3.one * 0.48f;
                silk.GetComponent<SpriteRenderer>().color = new Color(0.98f, 0.76f, 0.78f);
            }
            else
            {
                var window = Place("Window", grouping, 0.45f, 3.3f, -11);
                window.transform.localScale = Vector3.one * 0.64f;
            }
        }

        static void Solid(string name, Transform parent, float x, float y, float width, float height)
        {
            var item = Group(name, parent);
            item.localPosition = new Vector2(x, y);
            item.gameObject.layer = LayerMask.NameToLayer("Ground");
            item.tag = "Ground";
            item.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(width, height);
        }

        static void Cover(Transform parent, float x)
        {
            var item = Group("Supply crate", parent);
            var renderer = item.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = TrialArt.GetSprite("Crate");
            renderer.sortingOrder = -1;
            item.localPosition = new Vector2(x, 0.5f);
            Solid("Crate cover", parent, x, 0.48f, 1.28f, 0.96f);
        }

        static void Balcony(Transform parent, float x, float top)
        {
            var item = Place("Ledge", parent, x, top - 0.5f, -2);
            item.transform.localScale = Vector3.one * 0.5f;
            var platform = Group("Jump-through balcony", parent);
            platform.localPosition = new Vector2(x, top - 0.12f);
            platform.tag = "Ground";
            platform.gameObject.layer = LayerMask.NameToLayer("Ground");
            var collider = platform.gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(2.95f, 0.24f);
            collider.usedByEffector = true;
            var effector = platform.gameObject.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 125f;
        }

        static void Guard<T>(string kind, Transform cast, float startX, float left, float right)
            where T : TrialEnemy
        {
            var actor = new GameObject(typeof(T).Name);
            try
            {
                Transform visual = Group("Sprite", actor.transform);
                var renderer = visual.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.LoadAllAssetsAtPath(Root + "/Art/" + kind + "/Idle.png")
                    .OfType<Sprite>().OrderBy(sprite => sprite.name, StringComparer.Ordinal).First();
                renderer.sortingOrder = 4;
                var animator = visual.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = TrialArt.GetController(kind);
                Transform entry = Group("Left waypoint", actor.transform);
                Transform end = Group("Right waypoint", actor.transform);
                entry.localPosition = Vector2.right * (left - startX);
                end.localPosition = Vector2.right * (right - startX);
                var behaviour = actor.AddComponent<T>();
                behaviour.Configure(new[] {entry, end}, LayerMask.GetMask("Ground"));
                if (behaviour is CrateThrower thrower)
                    thrower.SetCrateSprite(TrialArt.GetSprite("Crate"));

                string path = PrefabFolder + "/" + actor.name + ".prefab";
                var prefab = PrefabUtility.SaveAsPrefabAsset(actor, path);
                if (!prefab) throw new InvalidOperationException("Failed to save enemy prefab: " + path);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, cast);
                instance.transform.localPosition = new Vector2(startX, 0f);
            }
            finally { UnityEngine.Object.DestroyImmediate(actor); }
        }
    }
}
