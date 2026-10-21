
using UnityEngine;

public class SlimeAI : MonoBehaviour
{
    [Header("Animator")]
    public Animator slimeAnimator;
    public string idleStateName = "idle";
    public string jumpStateName = "jump";
    public float animationBlendTime = 0.1f;

    [Header("Movement")]
    public float detectionRange = 10f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 8f;
    public float stopDistance = 0.9f;

    [Header("Hopping")]
    public float hopHeight = 0.5f;
    public float hopDuration = 0.6f;

    [Header("Contact Damage")]
    public float contactDamage = 10f;
    public float damageCooldown = 1f;

    [Header("Bounce")]
    public float bounceDistance = 2f;
    public float bounceDuration = 0.4f;
    public float recoveryTime = 0.5f;

    private Transform player;
    private bool isBouncing;
    private bool isRecovering;
    private float groundY;
    private float hopTimer;
    private float bounceTimer;
    private float recoveryTimer;
    private float nextDamageTime;

    private Vector3 bounceStart;
    private Vector3 bounceEnd;
    private Vector3 bounceDirection;

    private string currentAnimation = "";

    void Start()
    {
        if (slimeAnimator == null)
            slimeAnimator = GetComponent<Animator>();

        if (slimeAnimator == null)
            slimeAnimator = GetComponentInChildren<Animator>();

        if (slimeAnimator == null)
        {
            Debug.LogError("SlimeAI: Assign the slime's Animator.", this);
            enabled = false;
            return;
        }

        slimeAnimator.applyRootMotion = false;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("SlimeAI: No object tagged Player found.", this);
            enabled = false;
            return;
        }

        player = playerObject.transform;
        groundY = transform.position.y;

        Debug.Log(
            "SlimeAI using controller: " +
            (slimeAnimator.runtimeAnimatorController != null
                ? slimeAnimator.runtimeAnimatorController.name
                : "NONE"),
            this
        );

        PlayAnimation(idleStateName);
    }

    void Update()
    {
        if (player == null || slimeAnimator == null)
            return;

        if (isBouncing)
        {
            UpdateBounce();
            return;
        }

        if (isRecovering)
        {
            recoveryTimer -= Time.deltaTime;
            SetGroundY();
            PlayAnimation(idleStateName);

            if (recoveryTimer <= 0f)
            {
                isRecovering = false;
                hopTimer = 0f;
            }

            return;
        }

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;

        if (distance > detectionRange || distance <= stopDistance)
        {
            hopTimer = 0f;
            SetGroundY();
            PlayAnimation(idleStateName);
            return;
        }

        Vector3 direction = offset.normalized;
        RotateTowards(direction);

        // Move toward the player without orbiting around them.
        Vector3 movement = direction * moveSpeed * Time.deltaTime;
        Vector3 position = transform.position + movement;

        // Repeat the visual hop while chasing.
        hopTimer += Time.deltaTime;
        float duration = Mathf.Max(0.1f, hopDuration);
        float phase = (hopTimer % duration) / duration;

        position.y = groundY +
            Mathf.Sin(phase * Mathf.PI) * hopHeight;

        transform.position = position;
        PlayAnimation(jumpStateName);
    }

    void OnTriggerEnter(Collider other)
    {
        TryDamagePlayer(other.gameObject);
    }

    void OnTriggerStay(Collider other)
    {
        TryDamagePlayer(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    void OnCollisionStay(Collision collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    void TryDamagePlayer(GameObject hitObject)
    {
        if (isBouncing || isRecovering)
            return;

        if (Time.time < nextDamageTime)
            return;

        PlayerCombatStats stats =
            hitObject.GetComponentInParent<PlayerCombatStats>();

        if (stats == null)
            stats = hitObject.GetComponentInChildren<PlayerCombatStats>();

        if (stats == null)
            return;

        stats.TakeDamage(contactDamage);
        nextDamageTime = Time.time + damageCooldown;

        Debug.Log("Slime hit player for " + contactDamage + " damage.", this);

        StartBounce();
    }

    void StartBounce()
    {
        Vector3 away = transform.position - player.position;
        away.y = 0f;

        if (away.sqrMagnitude < 0.001f)
            away = -transform.forward;

        bounceDirection = away.normalized;

        bounceStart = transform.position;
        bounceStart.y = groundY;

        bounceEnd = bounceStart + bounceDirection * bounceDistance;
        bounceEnd.y = groundY;

        bounceTimer = 0f;
        isBouncing = true;

        PlayAnimation(jumpStateName);
    }

    void UpdateBounce()
    {
        bounceTimer += Time.deltaTime;

        float duration = Mathf.Max(0.01f, bounceDuration);
        float t = Mathf.Clamp01(bounceTimer / duration);
        float eased = 1f - Mathf.Pow(1f - t, 2f);

        Vector3 position = Vector3.Lerp(bounceStart, bounceEnd, eased);
        position.y = groundY + Mathf.Sin(t * Mathf.PI) * hopHeight;

        transform.position = position;
        RotateTowards(bounceDirection);

        if (t >= 1f)
        {
            transform.position = bounceEnd;
            isBouncing = false;
            isRecovering = true;
            recoveryTimer = recoveryTime;
            PlayAnimation(idleStateName);
        }
    }

    void SetGroundY()
    {
        Vector3 position = transform.position;
        position.y = groundY;
        transform.position = position;
    }

    void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion target =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            target,
            rotationSpeed * Time.deltaTime
        );
    }

    void PlayAnimation(string stateName)
    {
        if (slimeAnimator == null || string.IsNullOrEmpty(stateName))
            return;

        if (currentAnimation == stateName)
            return;

        if (slimeAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("SlimeAI: Animator Controller is missing.", this);
            return;
        }

        // Animator states are often addressed by their full layer path.
        string fullPath = "Base Layer." + stateName;
        int fullHash = Animator.StringToHash(fullPath);
        int shortHash = Animator.StringToHash(stateName);

        if (slimeAnimator.HasState(0, fullHash))
        {
            slimeAnimator.CrossFadeInFixedTime(
                fullHash, animationBlendTime, 0, 0f
            );
        }
        else if (slimeAnimator.HasState(0, shortHash))
        {
            slimeAnimator.CrossFadeInFixedTime(
                shortHash, animationBlendTime, 0, 0f
            );
        }
        else
        {
            Debug.LogError(
                "SlimeAI: Neither '" + fullPath +
                "' nor '" + stateName +
                "' exists on layer 0 of controller '" +
                slimeAnimator.runtimeAnimatorController.name + "'.",
                this
            );
            return;
        }

        currentAnimation = stateName;
        Debug.Log("Slime animation requested: " + stateName, this);
    }
}
