
using UnityEngine;

public class Hazard : MonoBehaviour
{
    protected virtual void Awake() { LevelBalance.Apply(this); }
    public int damage;
    public float lifetime;

    protected virtual void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
