using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuView : MonoBehaviour
{
    TMP_FontAsset font;
    Color ink=new Color32(255,239,230,255);
    public void Build(MainMenu menu)
    {
        var canvases=FindObjectsByType<Canvas>();
        Canvas canvas=null;
        foreach(var c in canvases)if(c.isRootCanvas && c.gameObject.scene==gameObject.scene){canvas=c;break;}
        if(canvas==null)return;
        var old=canvas.GetComponentInChildren<TMP_Text>(true);font=old!=null?old.font:TMP_Settings.defaultFontAsset;
        Sprite background=null;
        foreach(var image in canvas.GetComponentsInChildren<Image>(true))if(image.name=="Background")background=image.sprite;
        foreach(Transform child in canvas.transform)child.gameObject.SetActive(false);
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>();if(scaler==null)scaler=canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
        if(canvas.GetComponent<GraphicRaycaster>()==null)canvas.gameObject.AddComponent<GraphicRaycaster>();
        var bg=Box("Sakura backdrop",canvas.transform,Vector2.zero,Vector2.one,new Color32(103,63,103,255));
        if(background!=null){bg.sprite=background;bg.color=Color.white;bg.preserveAspect=false;}
        Box("Dusk overlay",canvas.transform,Vector2.zero,Vector2.one,new Color(.12f,.04f,.18f,.4f));
        var card=Box("Menu frame",canvas.transform,new Vector2(.12f,.09f),new Vector2(.88f,.91f),new Color32(224,157,174,255));
        var surface=Box("Menu surface",card.transform,Vector2.zero,Vector2.one,new Color32(49,28,54,242));
        surface.rectTransform.offsetMin=new Vector2(3,3);surface.rectTransform.offsetMax=new Vector2(-3,-3);
        Label(surface.transform,"A JOURNEY THROUGH THE BLOSSOMS",new Vector2(.08f,.83f),new Vector2(.92f,.92f),22,new Color32(230,177,185,255));
        Label(surface.transform,"SAKURA",new Vector2(.04f,.57f),new Vector2(.96f,.84f),110,ink,FontStyles.Bold);
        Label(surface.transform,"A quiet world. A brave heart. A new adventure.",new Vector2(.07f,.46f),new Vector2(.93f,.57f),24,new Color32(226,199,216,255));
        Box("Gold divider",surface.transform,new Vector2(.36f,.42f),new Vector2(.64f,.425f),new Color32(232,184,122,255));
        var play=Button(surface.transform,"PLAY",new Vector2(.22f,.235f),new Vector2(.78f,.365f),new Color32(190,69,112,255),menu.PlayGame);
        var quit=Button(surface.transform,"QUIT",new Vector2(.22f,.075f),new Vector2(.78f,.205f),new Color32(81,50,82,255),menu.QuitGame);
        var n=play.navigation;n.mode=Navigation.Mode.Explicit;n.selectOnDown=quit;play.navigation=n;
        n=quit.navigation;n.mode=Navigation.Mode.Explicit;n.selectOnUp=play;quit.navigation=n;
        for(int i=0;i<10;i++)
        {
            float x=i%2==0?.035f:.94f,y=.08f+(i/2)*.18f;
            var petal=Box("Petal",canvas.transform,new Vector2(x,y),new Vector2(x+.018f,y+.03f),new Color(1,.68f,.78f,.55f));
            petal.rectTransform.localRotation=Quaternion.Euler(0,0,30+i*11);
        }
        if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(play.gameObject);
    }
    Image Box(string name,Transform parent,Vector2 min,Vector2 max,Color color)
    {
        var o=new GameObject(name,typeof(RectTransform),typeof(Image));o.layer=parent.gameObject.layer;
        var r=o.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
        var image=o.GetComponent<Image>();image.color=color;image.raycastTarget=false;return image;
    }
    void Label(Transform parent,string text,Vector2 min,Vector2 max,float size,Color color,FontStyles style=FontStyles.Normal)
    {
        var o=new GameObject(text,typeof(RectTransform),typeof(TextMeshProUGUI));o.layer=parent.gameObject.layer;
        var r=o.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
        var t=o.GetComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.color=color;t.fontStyle=style;t.alignment=TextAlignmentOptions.Center;
        t.enableAutoSizing=true;t.fontSizeMin=12;t.fontSizeMax=size;t.raycastTarget=false;
    }
    Button Button(Transform parent,string text,Vector2 min,Vector2 max,Color color,UnityEngine.Events.UnityAction action)
    {
        var image=Box(text,parent,min,max,color);image.raycastTarget=true;
        var shadow=image.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.3f);shadow.effectDistance=new Vector2(0,-4);
        var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);
        var c=b.colors;c.highlightedColor=new Color(1,.87f,.9f);c.selectedColor=c.highlightedColor;c.pressedColor=new Color(.78f,.65f,.75f);b.colors=c;
        Label(image.transform,text,new Vector2(.05f,.1f),new Vector2(.95f,.9f),30,ink,FontStyles.Bold);return b;
    }
}