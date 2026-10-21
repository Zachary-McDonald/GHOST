
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class SlimeAIPlayable : MonoBehaviour
{
    [Header("Animation Clips")]
    public Animator slimeAnimator;
    public AnimationClip idleClip;
    public AnimationClip jumpClip;
    public float blendSpeed = 8f;

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

    private PlayableGraph graph;
    private AnimationMixerPlayable mixer;
    private AnimationClipPlayable idlePlayable;
    private AnimationClipPlayable jumpPlayable;

    private float idleWeight = 1f;
    private float jumpWeight = 0f;
    private bool wantsJump;
    private bool graphReady;

    void Start()
    {
        if (slimeAnimator == null)
            slimeAnimator = GetComponentInChildren<Animator>();

        if (slimeAnimator == null)
        {
            Debug.LogError("Slime: No Animator found.", this);
            enabled = false;
            return;
        }

        if (idleClip == null || jumpClip == null)
        {
            Debug.LogError(
                "Slime: Assign both Idle Clip and Jump Clip in the Inspector.",
                this
            );
            enabled = false;
            return;
        }

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("Slime: No object tagged Player found.", this);
            enabled = false;
            return;
        }

        player = playerObject.transform;
        groundY = transform.position.y;

        // Stop the old Animator Controller from controlling animations.
        slimeAnimator.runtimeAnimatorController = null;
        slimeAnimator.applyRootMotion = false;

        graph = PlayableGraph.Create("Slime Animation Graph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        idlePlayable = AnimationClipPlayable.Create(graph, idleClip);
        jumpPlayable = AnimationClipPlayable.Create(graph, jumpClip);

        idlePlayable.SetApplyFootIK(false);
        jumpPlayable.SetApplyFootIK(false);

        mixer = AnimationMixerPlayable.Create(graph, 2);

        graph.Connect(idlePlayable, 0, mixer, 0);
        graph.Connect(jumpPlayable, 0, mixer, 1);

        mixer.SetInputWeight(0, 1f);
        mixer.SetInputWeight(1, 0f);

        AnimationPlayableOutput output =
            AnimationPlayableOutput.Create(
                graph, "Slime Animation Output", slimeAnimator
            );

        output.SetSourcePlayable(mixer);

        graph.Play();
        graphReady = true;

        Debug.Log("Slime: Direct animation playback started.", this);
    }

    void Update()
    {
        if (player == null || !graphReady)
            return;

        UpdateAnimationBlend();

        if (isBouncing)
        {
            UpdateBounce();
            return;
        }

        if (isRecovering)
        {
            recoveryTimer -= Time.deltaTime;
            SetGroundY();
            wantsJump = false;

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
            wantsJump = false;
            return;
        }

        Vector3 direction = offset.normalized;
        RotateTowards(direction);

        Vector3 position = transform.position;
        position += direction * moveSpeed * Time.deltaTime;

        hopTimer += Time.deltaTime;
        float duration = Mathf.Max(0.1f, hopDuration);
        float phase = (hopTimer % duration) / duration;

        position.y = groundY +
            Mathf.Sin(phase * Mathf.PI) * hopHeight;

        transform.position = position;
        wantsJump = true;
    }

    void UpdateAnimationBlend()
    {
        float targetJump = wantsJump ? 1f : 0f;

        jumpWeight = Mathf.MoveTowards(
            jumpWeight,
            targetJump,
            blendSpeed * Time.deltaTime
        );

        idleWeight = 1f - jumpWeight;

        mixer.SetInputWeight(0, idleWeight);
        mixer.SetInputWeight(1, jumpWeight);
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
        wantsJump = true;
    }

    void UpdateBounce()
    {
        bounceTimer += Time.deltaTime;

        float duration = Mathf.Max(0.01f, bounceDuration);
        float t = Mathf.Clamp01(bounceTimer / duration);
        float eased = 1f - Mathf.Pow(1f - t, 2f);

        Vector3 position = Vector3.Lerp(
            bounceStart, bounceEnd, eased
        );

        position.y = groundY +
            Mathf.Sin(t * Mathf.PI) * hopHeight;

        transform.position = position;
        RotateTowards(bounceDirection);

        if (t >= 1f)
        {
            transform.position = bounceEnd;
            isBouncing = false;
            isRecovering = true;
            recoveryTimer = recoveryTime;
            wantsJump = false;
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

        Quaternion target = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            target,
            rotationSpeed * Time.deltaTime
        );
    }

    void OnDestroy()
    {
        if (graph.IsValid())
            graph.Destroy();
    }
}
