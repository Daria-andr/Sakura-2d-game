using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Temporary setup tool. Removed from Assets after configuring the saved scenes.
public static class SakuraMobileSetup
{
    static readonly Color Pink = new Color32(174, 69, 117, 255);
    static readonly Color Dark = new Color32(49, 31, 60, 245);
    static TMP_FontAsset font;

    [MenuItem("Tools/Sakura/Apply Mobile UI %#m")]
    static void Run()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode first.");
        EditorSceneManager.SaveOpenScenes();
        try
        {
            foreach (string name in new[] { "MainMenu", "Level1", "Level2", "BossFight" })
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
                var canvas = UnityEngine.Object.FindObjectsByType<Canvas>().First(c => c.isRootCanvas);
                font = canvas.GetComponentsInChildren<TMP_Text>(true).Select(t => t.font).FirstOrDefault(f => f != null);
                if (font == null) font = TMP_Settings.defaultFontAsset;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 1;

                RectTransform safe = canvas.transform.Find("SafeArea") as RectTransform;
                if (safe == null) safe = Rect("SafeArea", canvas.transform);
                Stretch(safe);
                if (safe.GetComponent<SafeArea>() == null) safe.gameObject.AddComponent<SafeArea>();
                if (name == "MainMenu") MainMenu(canvas, safe);
                else Game(canvas, safe, name);
                foreach (Transform child in canvas.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 5;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");
            File.WriteAllText("Logs/MobileUISetup.txt", "OK: MainMenu, Level1, Level2, BossFight. Saved static UI with SafeArea, pause, score and controls.");
        }
        catch (Exception exception)
        {
            File.WriteAllText("Logs/MobileUISetup.txt", exception.ToString());
            Debug.LogException(exception);
        }
    }

    static void Game(Canvas canvas, RectTransform safe, string sceneName)
    {
        var manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (manager == null) throw new Exception("Missing GameManager in " + sceneName);

        // Replace old result panels, including prefab copies, with editable scene UI.
        foreach (Transform child in canvas.transform.Cast<Transform>().ToArray())
        {
            if (child != safe && child.name.Contains("Panel")) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        HeartUI hearts = UnityEngine.Object.FindAnyObjectByType<HeartUI>();
        for (int i = 0; i < hearts.hearts.Length; i++)
        {
            Image heart = hearts.hearts[i];
            if (heart == null) continue;
            heart.transform.SetParent(safe, false);
            Position(heart.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40 + i * 80, -35), new Vector2(64, 64));
            heart.preserveAspect = true;
            heart.raycastTarget = false;
        }
        manager.enemyText.transform.SetParent(safe, false);
        Position(manager.enemyText.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -90), new Vector2(420, 64));
        StyleText(manager.enemyText, 40);
        manager.scoreText = Text("ScoreText", safe, "Score: 0", 44);
        Position(manager.scoreText.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -25), new Vector2(420, 64));
        var hudPlate = Panel("HUD contrast", safe, new Color32(49, 31, 60, 160));
        Position(hudPlate, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -15), new Vector2(460, 148));
        hudPlate.SetAsFirstSibling();
        hudPlate.GetComponent<Image>().raycastTarget = false;

        foreach (MobileController control in canvas.GetComponentsInChildren<MobileController>(true))
        {
            control.transform.SetParent(safe, false);
            RectTransform rect = (RectTransform)control.transform;
            if (control.controlType == MobileController.ControlType.Joystick)
            {
                Position(rect, Vector2.zero, Vector2.zero, new Vector2(50, 50), new Vector2(260, 260));
                control.movementRange = 70;
                if (control.handle != null)
                {
                    Position(control.handle, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(110, 110));
                    var handleImage = control.handle.GetComponent<Image>();
                    if (handleImage != null) { handleImage.color = Pink; handleImage.raycastTarget = false; }
                }
                var image = control.GetComponent<Image>();
                if (image != null) { image.color = new Color32(245, 218, 236, 195); image.raycastTarget = true; }
            }
            else
            {
                bool jump = control.controlType == MobileController.ControlType.Jump;
                Position(rect, Vector2.right, Vector2.right, new Vector2(jump ? -40 : -250, 50), new Vector2(180, 150));
                var image = control.GetComponent<Image>();
                if (image != null) { image.color = Pink; image.sprite = null; image.raycastTarget = true; }
                var label = control.GetComponentInChildren<TMP_Text>(true);
                if (label != null) { Stretch(label.rectTransform); StyleText(label, 40); label.text = jump ? "JUMP" : "SHOOT"; }
            }
        }

        Button pause = Button("PauseButton", safe, "PAUSE", 150, 120);
        Position((RectTransform)pause.transform, Vector2.one, Vector2.one, new Vector2(-40, -30), new Vector2(150, 120));
        UnityEventTools.AddBoolPersistentListener(pause.onClick, manager.SetPaused, true);

        // A full-screen dimmer is separate from its full-screen SafeArea content.
        RectTransform pauseRoot = Overlay(canvas, "PausePanel", out RectTransform pauseSafe);
        RectTransform card = Card(pauseSafe, "PAUSED", 640, 680);
        Button resume = Button("ResumeButton", card, "RESUME", 460, 120);
        Center(resume.transform, 30, 460, 120);
        UnityEventTools.AddBoolPersistentListener(resume.onClick, manager.SetPaused, false);
        Button restart = Button("RestartButton", card, "RESTART", 460, 120);
        Center(restart.transform, -120, 460, 120);
        UnityEventTools.AddPersistentListener(restart.onClick, manager.RestartGame);
        Button menu = Button("MenuButton", card, "MAIN MENU", 460, 120);
        Center(menu.transform, -270, 460, 120);
        UnityEventTools.AddPersistentListener(menu.onClick, manager.BackToMenu);
        manager.pausePanel = pauseRoot.gameObject;
        manager.pausePanel.SetActive(false);

        manager.winPanel = Result(canvas, "WinPanel", sceneName == "BossFight" ? "VICTORY!" : "LEVEL COMPLETE!");
        manager.gameOverPanel = Result(canvas, "GameOverPanel", "GAME OVER");

        BossController boss = UnityEngine.Object.FindAnyObjectByType<BossController>();
        if (boss != null && boss.healthBar != null)
        {
            boss.healthBar.transform.SetParent(safe, false);
            Position((RectTransform)boss.healthBar.transform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -230), new Vector2(500, 35));
            boss.healthBar.interactable = false;
            foreach (TMP_Text old in boss.healthBar.GetComponentsInChildren<TMP_Text>(true)) old.gameObject.SetActive(false);
            TMP_Text label = Text("BossTitle", safe, "BOSS", 36);
            Position(label.rectTransform, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -175), new Vector2(500, 50));
        }
        EditorUtility.SetDirty(manager);
    }

    static void MainMenu(Canvas canvas, RectTransform safe)
    {
        var menu = UnityEngine.Object.FindAnyObjectByType<MainMenu>();
        foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
            if (text.GetComponentInParent<Button>() == null) text.gameObject.SetActive(false);
        foreach (Button button in canvas.GetComponentsInChildren<Button>(true)) button.gameObject.SetActive(false);
        foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
            if (image.name == "Background") { Stretch(image.rectTransform); image.raycastTarget = false; }
        RectTransform card = Card(safe, "SAKURA", 720, 640);
        TMP_Text subtitle = Text("Subtitle", card, "A 2D platform adventure", 36);
        Center(subtitle.transform, 80, 660, 60);
        Button play = Button("Play", card, "PLAY", 460, 120);
        Center(play.transform, -50, 460, 120);
        UnityEventTools.AddPersistentListener(play.onClick, menu.PlayGame);
        Button quit = Button("Quit", card, "QUIT", 460, 120);
        Center(quit.transform, -200, 460, 120);
        UnityEventTools.AddPersistentListener(quit.onClick, menu.QuitGame);
    }

    static GameObject Result(Canvas canvas, string name, string title)
    {
        RectTransform root = Overlay(canvas, name, out RectTransform safe);
        RectTransform card = Card(safe, title, 820, 600);
        Button primary = Button("ContinueOrRestart", card, title == "LEVEL COMPLETE!" ? "CONTINUE" : "RESTART", 460, 120);
        Center(primary.transform, -30, 460, 120);
        Button menu = Button("Menu", card, "MAIN MENU", 460, 120);
        Center(menu.transform, -190, 460, 120);
        var view = root.gameObject.AddComponent<VictoryPanelView>();
        view.titleText = card.Find("Title").GetComponent<TMP_Text>();
        view.primaryButtonText = primary.GetComponentInChildren<TMP_Text>();
        view.primaryButton = primary;
        view.menuButton = menu;
        root.gameObject.SetActive(false);
        return root.gameObject;
    }

    static RectTransform Overlay(Canvas canvas, string name, out RectTransform safe)
    {
        RectTransform root = Panel(name, canvas.transform, new Color(0.1f, .06f, .12f, .75f));
        Stretch(root);
        safe = Rect("SafeArea", root);
        Stretch(safe);
        safe.gameObject.AddComponent<SafeArea>();
        return root;
    }

    static RectTransform Card(Transform parent, string title, float width, float height)
    {
        RectTransform card = Panel("Card", parent, Dark);
        Center(card, 0, width, height);
        TMP_Text label = Text("Title", card, title, 56);
        Center(label.transform, height * .5f - 110, width - 40, 110);
        return card;
    }

    static RectTransform Rect(string name, Transform parent)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.layer = 5;
        obj.transform.SetParent(parent, false);
        return (RectTransform)obj.transform;
    }

    static RectTransform Panel(string name, Transform parent, Color color)
    {
        RectTransform rect = Rect(name, parent);
        rect.gameObject.AddComponent<Image>().color = color;
        return rect;
    }

    static TextMeshProUGUI Text(string name, Transform parent, string value, float size)
    {
        RectTransform rect = Rect(name, parent);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        StyleText(text, size);
        return text;
    }

    static void StyleText(TMP_Text text, float size)
    {
        text.fontSize = size;
        text.enableAutoSizing = false;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    static Button Button(string name, Transform parent, string caption, float width, float height)
    {
        RectTransform rect = Panel(name, parent, Pink);
        Center(rect, 0, width, height);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        TextMeshProUGUI label = Text("Label", rect, caption, 40);
        Stretch(label.rectTransform);
        return button;
    }

    static void Center(Transform transform, float y, float width, float height)
    {
        Position((RectTransform)transform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, y), new Vector2(width, height));
    }

    static void Position(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition3D = new Vector3(position.x, position.y, 0);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        var position = rect.localPosition;
        position.z = 0;
        rect.localPosition = position;
    }
}
