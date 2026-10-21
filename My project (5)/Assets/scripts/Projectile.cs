
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float Speed = 15f;
    public float Lifetime = 3f;
    public float damage = 5f;

    private Rigidbody rb;
    private bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * Speed;
        }
        else
        {
            Debug.LogWarning(
                "Projectile: No Rigidbody found on " + gameObject.name
            );
        }

        Destroy(gameObject, Lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    void HandleHit(GameObject hitObject)
    {
        if (hasHit)
            return;

        hasHit = true;

        EnemyHealth enemy =
            hitObject.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            Debug.Log(
                "Player projectile hit skeleton for " +
                damage + " damage!"
            );
        }

        Destroy(gameObject);
    }
}
