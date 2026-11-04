using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryPanelView : MonoBehaviour
{
    private TMP_FontAsset font;
    private Button mainButton;
    private bool isReady;

    public void Prepare(GameManager manager, bool defeat = false)
    {
        if (isReady || manager == null) return;

        RectTransform panel = GetComponent<RectTransform>();
        if (panel == null) return;

        // Берём шрифт из существующей панели в сцене.
        TMP_Text oldText = GetComponentInChildren<TMP_Text>(true);
        font = TMP_Settings.defaultFontAsset;
        if (oldText != null) font = oldText.font;

        // Старые элементы скрываем. Вместо них создаём простой интерфейс.
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        SetPosition(panel, Vector2.zero, Vector2.one);
        Image background = GetComponent<Image>();
        if (background == null) background = gameObject.AddComponent<Image>();
        background.sprite = null;
        background.color = new Color(0.12f, 0.08f, 0.14f, 0.95f);
        background.raycastTarget = true;

        // Панель должна перекрывать игровые кнопки и принимать нажатия.
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        string title = "LEVEL COMPLETE!";
        string buttonText = "CONTINUE";
        UnityAction buttonAction = manager.ContinueGame;

        if (defeat)
        {
            title = "GAME OVER";
            buttonText = "RESTART";
            buttonAction = manager.RestartGame;
        }
        else if (SceneManager.GetActiveScene().name == "BossFight")
        {
            title = "VICTORY!";
            buttonText = "RESTART";
            buttonAction = manager.RestartGame;
        }

        CreateText(transform, title, new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.85f), 48);
        mainButton = CreateButton(buttonText, 0.43f, buttonAction);
        Button menuButton = CreateButton("MAIN MENU", 0.23f, manager.BackToMenu);

        // Стрелки клавиатуры переключают только кнопки этой панели.
        Navigation navigation = new Navigation();
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnDown = menuButton;
        mainButton.navigation = navigation;
        navigation.selectOnDown = null;
        navigation.selectOnUp = mainButton;
        menuButton.navigation = navigation;

        isReady = true;
        if (isActiveAndEnabled) SelectMainButton();
    }

    // Создаём обычную розовую кнопку и назначаем действие при нажатии.
    private Button CreateButton(string caption, float bottom, UnityAction action)
    {
        GameObject buttonObject = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.layer = gameObject.layer;
        buttonObject.transform.SetParent(transform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetPosition(rect, new Vector2(0.25f, bottom), new Vector2(0.75f, bottom + 0.14f));

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color32(180, 70, 110, 255);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateText(buttonObject.transform, caption, Vector2.zero, Vector2.one, 28);
        return button;
    }

    // Надпись по центру указанной области.
    private void CreateText(Transform parent, string caption, Vector2 min, Vector2 max, float size)
    {
        GameObject textObject = new GameObject(caption, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.layer = gameObject.layer;
        textObject.transform.SetParent(parent, false);
        SetPosition(textObject.GetComponent<RectTransform>(), min, max);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = caption;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 10;
        text.fontSizeMax = size;
        text.raycastTarget = false;
    }

    // Координаты от 0 до 1 позволяют подстроиться под размер экрана.
    private void SetPosition(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private void OnEnable()
    {
        if (isReady) SelectMainButton();
    }

    private void SelectMainButton()
    {
        transform.SetAsLastSibling();
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(mainButton.gameObject);
        }
    }
}
