using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime=.2f;
    public Vector3 offset=new Vector3(0,2,-10);
    public bool useBounds=true;
    public float minX=-10,maxX=30,minY=-5,maxY=10;
    public bool autoBounds=true;
    Vector3 velocity;
    Camera cam;
    float preferredSize;
    void Awake(){cam=GetComponent<Camera>();preferredSize=cam.orthographicSize;}
    void Start()
    {
        if(autoBounds)
        {
            bool hasBackground=false,hasGround=false;
            Bounds background=default,ground=default;
            foreach(var r in FindObjectsByType<SpriteRenderer>())
            {
                if(!r.enabled || r.sprite==null || r.gameObject.scene!=gameObject.scene)continue;
                if(r.name.StartsWith("BG_")) {if(!hasBackground)background=r.bounds;else background.Encapsulate(r.bounds);hasBackground=true;}
                if(r.name.StartsWith("Ground")) {if(!hasGround)ground=r.bounds;else ground.Encapsulate(r.bounds);hasGround=true;}
            }
            if(hasBackground && hasGround)
            {
                float left=Mathf.Max(background.min.x,ground.min.x),right=Mathf.Min(background.max.x,ground.max.x);
                if(right>left) {minX=left+.05f;maxX=right-.05f;minY=ground.min.y+.05f;maxY=background.max.y-.05f;}
            }
        }
        ApplyBounds(transform.position);
    }
    void LateUpdate()
    {
        var position=transform.position;
        if(target!=null)position=Vector3.SmoothDamp(position,new Vector3(target.position.x+offset.x,position.y,offset.z),ref velocity,Mathf.Max(.01f,smoothTime));
        ApplyBounds(position);
    }
    void ApplyBounds(Vector3 position)
    {
        if(useBounds && cam!=null && cam.orthographic && maxX>minX && maxY>minY)
        {
            // Fit the entire viewport, including wide and portrait window sizes.
            float fit=Mathf.Min((maxY-minY)*.5f,(maxX-minX)*.5f/Mathf.Max(.01f,cam.aspect));
            cam.orthographicSize=Mathf.Min(preferredSize,fit);
            float halfY=cam.orthographicSize,halfX=halfY*cam.aspect;
            position.x=Mathf.Clamp(position.x,minX+halfX,maxX-halfX);
            position.y=Mathf.Clamp(position.y,minY+halfY,maxY-halfY);
        }
        transform.position=position;
    }
}