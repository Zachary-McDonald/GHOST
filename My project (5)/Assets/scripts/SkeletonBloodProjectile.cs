
using UnityEngine;

public class SkeletonBloodProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float damage = 10f;
    public float lifetime = 4f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position +=
            transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerCombatStats stats =
            other.GetComponentInParent<PlayerCombatStats>();

        if (stats == null)
            return;

        stats.TakeDamage(damage);
        Destroy(gameObject);
    }
}
