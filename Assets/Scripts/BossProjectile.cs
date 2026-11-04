using UnityEngine;
public class BossProjectile : MonoBehaviour
{
    public float speed=6f, lifetime=4f;
    public int damage=1;
    public Sprite[] flightFrames;
    public float animationFps=12f;
    Vector2 direction=Vector2.left;
    bool used;
    SpriteRenderer visual;
    float age;
    void Awake() {visual=GetComponent<SpriteRenderer>();}
    void Start() {Destroy(gameObject,lifetime);}
    public void SetDirection(int value) {SetVelocityDirection(value<0?Vector2.left:Vector2.right);}
    public void AimAt(Vector2 point) {SetVelocityDirection(point-(Vector2)transform.position);}
    void SetVelocityDirection(Vector2 value)
    {
        direction=value.sqrMagnitude>0.0001f?value.normalized:Vector2.right;
        transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
        if(visual!=null) visual.flipX=false;
    }
    void FixedUpdate()
    {
        if(used) return;
        float distance=Mathf.Max(0,speed)*Time.fixedDeltaTime;
        foreach(var hit in Physics2D.CircleCastAll(transform.position,0.1f,direction,distance))
        { Hit(hit.collider); if(used) return; }
        transform.position+=(Vector3)(direction*distance);
    }
    void Update()
    {
        age+=Time.deltaTime;
        if(visual!=null && flightFrames!=null && flightFrames.Length>0)
            visual.sprite=flightFrames[(int)(age*Mathf.Max(1,animationFps))%flightFrames.Length];
    }
    void OnTriggerEnter2D(Collider2D other) {Hit(other);}
    void OnCollisionEnter2D(Collision2D other) {Hit(other.collider);}
    void Hit(Collider2D other)
    {
        if(used || other==null || other.transform.IsChildOf(transform) || other.GetComponentInParent<BossController>()!=null) return;
        var player=other.GetComponentInParent<PlayerHealth>();
        if(player!=null) {used=true;player.TakeDamage(damage);Destroy(gameObject);}
        else if(!other.isTrigger && other.CompareTag("Ground")) {used=true;Destroy(gameObject);}
    }
}