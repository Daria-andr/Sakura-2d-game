using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds the same responsive victory layout for scene panels and prefab instances.
public class VictoryPanelView : MonoBehaviour
{
    CanvasGroup group;
    RectTransform card;
    Button primary;
    TMP_FontAsset font;
    bool ready;
    static readonly Color Ink = new Color32(66, 37, 66, 255);
    static readonly Color Rose = new Color32(186, 57, 102, 255);

    public void Prepare(GameManager manager)
    {
        if (ready || manager == null) return;
        var original = GetComponentInChildren<TMP_Text>(true);
        font = original != null ? original.font : TMP_Settings.defaultFontAsset;
        foreach (Transform child in transform) child.gameObject.SetActive(false);
        var rect = GetComponent<RectTransform>();
        if (rect == null) return;
        rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one;
        rect.offsetMin=rect.offsetMax=Vector2.zero; rect.localScale=Vector3.one;
        var backdrop=GetComponent<Image>();
        if(backdrop==null) backdrop=gameObject.AddComponent<Image>();
        backdrop.sprite=null; backdrop.color=new Color(0.12f,0.06f,0.17f,0.82f); backdrop.raycastTarget=true;
        var canvas=GetComponent<Canvas>(); if(canvas==null) canvas=gameObject.AddComponent<Canvas>();
        canvas.overrideSorting=true; canvas.sortingOrder=100;
        if(GetComponent<GraphicRaycaster>()==null) gameObject.AddComponent<GraphicRaycaster>();
        group=GetComponent<CanvasGroup>(); if(group==null) group=gameObject.AddComponent<CanvasGroup>();
        var shadow=Box("Card shadow",transform,new Color(0,0,0,0.24f),new Vector2(.19f,.105f),new Vector2(.83f,.865f));
        var border=Box("Rose border",transform,new Color32(229,150,175,255),new Vector2(.18f,.12f),new Vector2(.82f,.88f));
        card=border.rectTransform;
        var surface=Box("Ivory card",card,new Color32(255,245,238,255),Vector2.zero,Vector2.one);
        surface.rectTransform.offsetMin=new Vector2(4,4); surface.rectTransform.offsetMax=new Vector2(-4,-4);
        Box("Top ribbon",surface.transform,Rose,new Vector2(0,.96f),Vector2.one);
        bool final=SceneManager.GetActiveScene().name=="BossFight";
        Label("Chapter",surface.transform,final?"SAKURA  /  FINAL BATTLE":"SAKURA  /  CHAPTER CLEARED",new Vector2(.08f,.80f),new Vector2(.92f,.90f),Rose,25);
        Label("Title",surface.transform,final?"VICTORY!":"LEVEL COMPLETE!",new Vector2(.05f,.59f),new Vector2(.95f,.79f),Ink,66,FontStyles.Bold);
        Label("Subtitle",surface.transform,final?"The guardian has fallen. Peace returns.":"A new path opens beneath the blossoms.",new Vector2(.09f,.46f),new Vector2(.91f,.59f),new Color32(120,88,114,255),26);
        Box("Divider",surface.transform,new Color32(232,190,200,255),new Vector2(.25f,.425f),new Vector2(.75f,.43f));
        UnityAction action=final ? new UnityAction(manager.RestartGame) : new UnityAction(manager.ContinueGame);
        primary=MakeButton(surface.transform,final?"PLAY AGAIN":"CONTINUE",new Vector2(.14f,.235f),new Vector2(.86f,.375f),Rose,Color.white,action);
        var menu=MakeButton(surface.transform,"MAIN MENU",new Vector2(.14f,.065f),new Vector2(.86f,.205f),new Color32(245,223,229,255),Ink,manager.BackToMenu);
        var nav=primary.navigation; nav.mode=Navigation.Mode.Explicit; nav.selectOnDown=menu;primary.navigation=nav;
        nav=menu.navigation;nav.mode=Navigation.Mode.Explicit;nav.selectOnUp=primary;menu.navigation=nav;
        for(int i=0;i<8;i++)
        {
            bool left=i<4; float y=.18f+(i%4)*.2f;
            var petal=Box("Petal",transform,new Color(1,.65f,.76f,.35f),new Vector2(left?.065f:.91f,y),new Vector2(left?.09f:.935f,y+.035f));
            petal.rectTransform.localRotation=Quaternion.Euler(0,0,35+i*13);petal.raycastTarget=false;
        }
        ready=true;
        if(isActiveAndEnabled) OnEnable();
    }
    Image Box(string name,Transform parent,Color color,Vector2 min,Vector2 max)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));obj.layer=gameObject.layer;
        var rt=obj.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=rt.offsetMax=Vector2.zero;
        var image=obj.GetComponent<Image>();image.color=color;image.raycastTarget=false;return image;
    }
    void Label(string name,Transform parent,string text,Vector2 min,Vector2 max,Color color,float size,FontStyles style=FontStyles.Normal)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));obj.layer=gameObject.layer;
        var rt=obj.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=rt.offsetMax=Vector2.zero;
        var label=obj.GetComponent<TextMeshProUGUI>();label.font=font;label.text=text;label.color=color;label.fontStyle=style;
        label.alignment=TextAlignmentOptions.Center;label.enableAutoSizing=true;label.fontSizeMin=12;label.fontSizeMax=size;label.raycastTarget=false;
    }
    Button MakeButton(Transform parent,string text,Vector2 min,Vector2 max,Color color,Color foreground,UnityAction action)
    {
        var image=Box(text,parent,color,min,max);image.raycastTarget=true;
        var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
        var colors=button.colors;colors.highlightedColor=new Color(.92f,.92f,.92f);colors.pressedColor=new Color(.8f,.8f,.8f);button.colors=colors;
        button.onClick.AddListener(action);
        Label("Label",image.transform,text,new Vector2(.04f,.08f),new Vector2(.96f,.92f),foreground,30,FontStyles.Bold);
        return button;
    }
    void OnEnable()
    {
        if(!ready)return;
        transform.SetAsLastSibling();StopAllCoroutines();StartCoroutine(Reveal());
    }
    IEnumerator Reveal()
    {
        group.interactable=false;
        for(float t=0;t<.25f;t+=Time.unscaledDeltaTime)
        {
            float a=Mathf.SmoothStep(0,1,t/.25f);group.alpha=a;card.localScale=Vector3.one*Mathf.Lerp(.95f,1,a);yield return null;
        }
        group.alpha=1;card.localScale=Vector3.one;group.interactable=true;
        if(EventSystem.current!=null) EventSystem.current.SetSelectedGameObject(primary.gameObject);
    }
    void OnDisable(){StopAllCoroutines();}
}