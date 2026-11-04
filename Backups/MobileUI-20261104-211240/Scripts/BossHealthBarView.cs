using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarView : MonoBehaviour
{
    BossController boss;
    RectTransform fill, trail;
    Image fillImage;
    TMP_Text title, counter;
    float delayed=1, wait;
    int lastHp=-1;
    bool lastPhase;
    public void Prepare(BossController target)
    {
        if(boss!=null || target==null)return;
        boss=target;
        var old=GetComponentInChildren<TMP_Text>(true);
        var font=old!=null?old.font:TMP_Settings.defaultFontAsset;
        foreach(Transform child in transform)child.gameObject.SetActive(false);
        var slider=GetComponent<Slider>();
        if(slider!=null){slider.interactable=false;slider.targetGraphic=null;slider.fillRect=null;slider.handleRect=null;}
        var root=GetComponent<RectTransform>();
        root.anchorMin=new Vector2(.20f,.85f);root.anchorMax=new Vector2(.80f,.97f);
        root.offsetMin=root.offsetMax=Vector2.zero;root.localScale=Vector3.one;
        var panel=GetComponent<Image>();if(panel==null)panel=gameObject.AddComponent<Image>();
        panel.sprite=null;panel.color=new Color32(32,21,40,235);panel.raycastTarget=false;
        Make("Top accent",transform,new Vector2(.02f,.95f),new Vector2(.98f,.98f),new Color32(234,174,121,255));
        title=Text("Boss name",font,new Vector2(.04f,.48f),new Vector2(.74f,.91f),TextAlignmentOptions.MidlineLeft,24);
        counter=Text("HP",font,new Vector2(.75f,.48f),new Vector2(.96f,.91f),TextAlignmentOptions.MidlineRight,23);
        var border=Make("Frame",transform,new Vector2(.035f,.14f),new Vector2(.965f,.43f),new Color32(174,116,130,255));
        var track=Make("Track",border.transform,Vector2.zero,Vector2.one,new Color32(64,35,52,255));
        track.rectTransform.offsetMin=new Vector2(2,2);track.rectTransform.offsetMax=new Vector2(-2,-2);
        trail=Make("Damage trail",track.transform,Vector2.zero,Vector2.one,new Color32(255,208,131,255)).rectTransform;
        fillImage=Make("Health",track.transform,Vector2.zero,Vector2.one,new Color32(230,88,117,255));fill=fillImage.rectTransform;
        Make("Highlight",fill,new Vector2(0,.7f),Vector2.one,new Color(1,1,1,.22f));
        lastHp=-1;Update();
    }
    void Update()
    {
        if(boss==null || fill==null)return;
        float value=Mathf.Clamp01((float)boss.CurrentHealth/Mathf.Max(1,boss.maxHealth));
        if(lastHp!=boss.CurrentHealth || lastPhase!=boss.IsPhaseTwo)
        {
            if(lastHp>=0 && boss.CurrentHealth<lastHp)wait=.3f;
            if(lastHp<0)delayed=value;
            lastHp=boss.CurrentHealth;lastPhase=boss.IsPhaseTwo;
            counter.text=lastHp+" / "+Mathf.Max(1,boss.maxHealth);
            title.text=boss.IsPhaseTwo?"LAVA GUARDIAN   /   PHASE II":"LAVA GUARDIAN   /   PHASE I";
            fillImage.color=boss.IsPhaseTwo?new Color32(255,124,65,255):new Color32(230,88,117,255);
        }
        fill.anchorMax=new Vector2(value,1);
        wait-=Time.unscaledDeltaTime;
        if(wait<=0)delayed=Mathf.MoveTowards(delayed,value,Time.unscaledDeltaTime*.65f);
        trail.anchorMax=new Vector2(delayed,1);
    }
    Image Make(string name,Transform parent,Vector2 min,Vector2 max,Color color)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(Image));obj.layer=gameObject.layer;
        var rt=obj.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=rt.offsetMax=Vector2.zero;
        var image=obj.GetComponent<Image>();image.color=color;image.raycastTarget=false;return image;
    }
    TMP_Text Text(string name,TMP_FontAsset font,Vector2 min,Vector2 max,TextAlignmentOptions alignment,float size)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));obj.layer=gameObject.layer;
        var rt=obj.GetComponent<RectTransform>();rt.SetParent(transform,false);rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=rt.offsetMax=Vector2.zero;
        var text=obj.GetComponent<TextMeshProUGUI>();text.font=font;text.fontStyle=FontStyles.Bold;text.color=new Color32(255,232,223,255);text.alignment=alignment;
        text.enableAutoSizing=true;text.fontSizeMin=10;text.fontSizeMax=size;text.raycastTarget=false;return text;
    }
}