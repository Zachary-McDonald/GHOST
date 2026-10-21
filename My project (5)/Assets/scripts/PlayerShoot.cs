
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject ProjectilePrefab;
    public Transform FirePoint;
    public float ProjectileSpeed = 15f;
    public float ShootCooldown = 0.25f;

    [Header("Mana")]
    public float ManaCost = 1f;

    private PlayerCombatStats combatStats;
    private float nextShootTime;

    void Awake()
    {
        combatStats = GetComponent<PlayerCombatStats>();

        if (combatStats == null)
        {
            combatStats = GetComponentInParent<PlayerCombatStats>();
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        if (Time.time < nextShootTime)
            return;

        if (ProjectilePrefab == null || FirePoint == null)
        {
            Debug.LogError(
                "PlayerShoot: Assign ProjectilePrefab and FirePoint in the Inspector."
            );
            return;
        }

        if (combatStats == null)
        {
            Debug.LogError(
                "PlayerShoot: PlayerCombatStats was not found on this object or its parent."
            );
            return;
        }

        if (!combatStats.UseMana(ManaCost))
            return;

        Shoot();
        nextShootTime = Time.time + ShootCooldown;
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(
            ProjectilePrefab,
            FirePoint.position,
            FirePoint.rotation
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = FirePoint.forward * ProjectileSpeed;
        }
        else
        {
            Debug.LogError(
                "PlayerShoot: The projectile prefab needs a Rigidbody component."
            );
        }
    }
}
