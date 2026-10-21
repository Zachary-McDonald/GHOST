
using UnityEngine;
using System.Collections;

public class SkeletonMeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float frontKickDamage = 10f;
    public float roundhouseDamage = 15f;

    [Header("Attack Timing")]
    public float firstHitDelay = 0.35f;
    public float secondHitDelay = 0.8f;
    public float comboEndDelay = 0.7f;

    [Header("Blood Ball")]
    public GameObject bloodBallPrefab;
    public Transform bloodBallSpawnPoint;

    [Header("Blood Wave")]
    public GameObject bloodWavePrefab;
    public Transform bloodWaveSpawnPoint;

    private Animator animator;
    private Transform player;
    private float nextAttackTime;
    private float nextDistanceLogTime;
    private bool isAttacking;

    public bool IsAttacking => isAttacking;

    void Start()
    {
        animator = GetComponentInChildren<Animator>(true);

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError(
                "Skeleton: No object tagged Player found!"
            );
            enabled = false;
            return;
        }

        player = playerObject.transform;

        Debug.Log("Player found. Skeleton attack script is running.");

        if (animator == null)
        {
            Debug.LogError("Skeleton: No Animator found!");
            enabled = false;
            return;
        }

        Debug.Log("Skeleton Animator found: " + animator.gameObject.name);
    }

    void Update()
    {
        if (player == null || animator == null || isAttacking)
            return;

        Vector3 skeletonPos = transform.position;
        Vector3 playerPos = player.position;

        skeletonPos.y = 0f;
        playerPos.y = 0f;

        float distance = Vector3.Distance(skeletonPos, playerPos);

        if (Time.time >= nextDistanceLogTime)
        {
            Debug.Log(
                "Skeleton distance to player: " +
                distance.ToString("F2")
            );

            nextDistanceLogTime = Time.time + 1f;
        }

        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            Debug.Log("Skeleton starting combo!");
            StartCoroutine(PerformCombo());
        }
    }

    IEnumerator PerformCombo()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        // Face the player before attacking.
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Start the existing kick combo.
        animator.SetTrigger("Attack");

        // First kick: fire the Blood Ball.
        yield return new WaitForSeconds(firstHitDelay);

        SpawnBloodBall();

        // Wait for the second kick.
        yield return new WaitForSeconds(
            Mathf.Max(0f, secondHitDelay - firstHitDelay)
        );

        // Second kick: fire the Blood Wave.
        SpawnBloodWave();

        // Allow the combo animation to finish.
        yield return new WaitForSeconds(comboEndDelay);

        isAttacking = false;
    }

    void SpawnBloodBall()
    {
        if (bloodBallPrefab == null || bloodBallSpawnPoint == null)
        {
            Debug.LogWarning(
                "Blood Ball prefab or spawn point is not assigned!"
            );
            return;
        }

        Vector3 direction = player.position -
                            bloodBallSpawnPoint.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        GameObject projectile = Instantiate(
            bloodBallPrefab,
            bloodBallSpawnPoint.position,
            Quaternion.LookRotation(direction)
        );

        SkeletonBloodProjectile projectileScript =
            projectile.GetComponent<SkeletonBloodProjectile>();

        if (projectileScript != null)
            projectileScript.damage = frontKickDamage;

        Debug.Log("Blood Ball fired!");
    }

    void SpawnBloodWave()
    {
        if (bloodWavePrefab == null || bloodWaveSpawnPoint == null)
        {
            Debug.LogWarning(
                "Blood Wave prefab or spawn point is not assigned!"
            );
            return;
        }

        GameObject projectile = Instantiate(
            bloodWavePrefab,
            bloodWaveSpawnPoint.position,
            Quaternion.LookRotation(transform.forward)
        );

        SkeletonBloodProjectile projectileScript =
            projectile.GetComponent<SkeletonBloodProjectile>();

        if (projectileScript != null)
            projectileScript.damage = roundhouseDamage;

        Debug.Log("Blood Wave fired!");
    }
}
