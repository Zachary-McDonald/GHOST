
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public Slider healthBar;

    [Header("Mana")]
    public float maxMana = 10f;
    public float currentMana = 10f;

    [Tooltip("Seconds needed to regenerate from empty mana to full.")]
    public float fullManaRegenTime = 1.5f;

    public Slider manaBar;

    [Header("Death Settings")]
    [Tooltip("Destroy the player after they have disappeared.")]
    public bool destroyPlayerOnDeath = false;

    [Tooltip("Optional delay before destroying the player.")]
    public float destroyDelay = 0.1f;

    private bool isDead;

    void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

        UpdateHealthBar();
        UpdateManaBar();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Update()
    {
        if (isDead)
            return;

        RegenerateMana();
    }

    void RegenerateMana()
    {
        if (currentMana >= maxMana || fullManaRegenTime <= 0f)
            return;

        float regenPerSecond = maxMana / fullManaRegenTime;

        currentMana += regenPerSecond * Time.deltaTime;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

        UpdateManaBar();
    }

    public bool UseMana(float amount)
    {
        if (isDead || amount < 0f || currentMana < amount)
            return false;

        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

        UpdateManaBar();
        return true;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthBar();

        Debug.Log("Player health: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    void UpdateManaBar()
    {
        if (manaBar == null)
            return;

        manaBar.maxValue = maxMana;
        manaBar.value = currentMana;
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Player defeated!");

        // Disable every MonoBehaviour on the player and its children,
        // including movement and combat scripts.
        MonoBehaviour[] scripts =
            GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script != this)
            {
                script.enabled = false;
            }
        }

        // Stop physics and collisions.
        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        CharacterController characterController =
            GetComponent<CharacterController>();

        if (characterController != null)
            characterController.enabled = false;

        Rigidbody[] rigidbodies =
            GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Hide the player model and its child objects.
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }

        // Hide the player's health and mana UI if it is attached
        // underneath the player.
        Canvas[] canvases =
            GetComponentsInChildren<Canvas>(true);

        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = false;
        }

        if (destroyPlayerOnDeath)
        {
            Destroy(gameObject, Mathf.Max(0f, destroyDelay));
        }
    }
}
