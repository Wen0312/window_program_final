using UnityEngine;

public class Bullet2D : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Bullet2D 找不到 Rigidbody2D（一定要掛在 Bullet 本體）");
        }
    }

    public void Fire(Vector2 dir)
    {
        if (rb == null) return;

        dir = dir.normalized;
        rb.linearVelocity = dir * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Bullet hit: " + other.name + " layer=" + LayerMask.LayerToName(other.gameObject.layer));
        Destroy(gameObject);
    }
}
