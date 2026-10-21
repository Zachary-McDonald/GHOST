
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SkeletonAI : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float rotationSpeed = 8f;
    public float gravity = -9.81f;

    [Header("Player Detection")]
    public float detectionRange = 8f;

    [Header("Patrol Path")]
    public Transform[] patrolPoints;
    public float waypointReachDistance = 0.4f;

    [Header("Idle Settings")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    private CharacterController controller;
    private Animator animator;
    private SkeletonMeleeAttack meleeAttack;
    private Transform player;

    private Vector3 velocity;
    private int currentWaypoint = 0;
    private float stateTimer;

    private bool chasingPlayer = false;
    private bool waitingAtWaypoint = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        meleeAttack = GetComponent<SkeletonMeleeAttack>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log("SkeletonAI: Player found!");
        }
        else
        {
            Debug.LogError(
                "SkeletonAI: No GameObject tagged Player found!"
            );
        }

        if (animator == null)
        {
            Debug.LogError("SkeletonAI: No Animator found!");
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning(
                "SkeletonAI: Assign patrol points in the Inspector!"
            );
        }

        StartIdle();
    }

    void Update()
    {
        // Pause movement while the melee combo is playing.
        if (meleeAttack != null && meleeAttack.IsAttacking)
        {
            SetAnimationSpeed(0f);

            if (controller.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            return;
        }

        // Detect the player.
        if (!chasingPlayer && player != null)
        {
            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            if (distance <= detectionRange)
            {
                chasingPlayer = true;
                waitingAtWaypoint = false;

                Debug.Log("SkeletonAI: Player detected!");
            }
        }

        // Chase or patrol.
        if (chasingPlayer && player != null)
        {
            ChasePlayer();
        }
        else
        {
            PatrolPath();
        }

        // Apply gravity.
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void PatrolPath()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            SetAnimationSpeed(0f);
            return;
        }

        int checkedPoints = 0;

        while (patrolPoints[currentWaypoint] == null)
        {
            currentWaypoint =
                (currentWaypoint + 1) % patrolPoints.Length;

            checkedPoints++;

            if (checkedPoints >= patrolPoints.Length)
            {
                SetAnimationSpeed(0f);
                return;
            }
        }

        if (waitingAtWaypoint)
        {
            stateTimer -= Time.deltaTime;
            SetAnimationSpeed(0f);

            if (stateTimer <= 0f)
            {
                waitingAtWaypoint = false;
            }

            return;
        }

        Transform target = patrolPoints[currentWaypoint];

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= waypointReachDistance)
        {
            currentWaypoint =
                (currentWaypoint + 1) % patrolPoints.Length;

            waitingAtWaypoint = true;

            stateTimer = Random.Range(
                minIdleTime,
                maxIdleTime
            );

            SetAnimationSpeed(0f);
            return;
        }

        SetAnimationSpeed(1f);
        MoveInDirection(direction.normalized);
    }

    void StartIdle()
    {
        waitingAtWaypoint = true;

        stateTimer = Random.Range(
            minIdleTime,
            maxIdleTime
        );

        SetAnimationSpeed(0f);
    }

    void ChasePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            SetAnimationSpeed(1f);
            MoveInDirection(direction.normalized);
        }
        else
        {
            SetAnimationSpeed(0f);
        }
    }

    void MoveInDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        controller.Move(
            direction * walkSpeed * Time.deltaTime
        );
    }

    void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }
    }
}
