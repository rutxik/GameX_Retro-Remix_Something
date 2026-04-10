using System.Collections;
using UnityEngine;

/// <summary>
/// Donkey Kong-style Boss Controller
/// -------------------------------------------------------
/// Phases:
///   Phase 1 (full HP → 50 %): throws normal barrels, jumps + AOE smash
///   Phase 2 (≤ 50 %):         same + explosive barrels + barrel rain
///
/// External requirements (assign in Inspector):
///   • barrelPrefab          – normal rolling barrel
///   • explosiveBarrelPrefab – explosive barrel (separate asset)
///   • rainBarrelPrefab      – barrel used for rain (can share normal barrel)
///   • throwPoint            – Transform where barrels spawn
///   • groundCheck           – Transform used to detect landing
///   • groundLayer           – LayerMask for ground
///   • aoeEffect             – optional particle / animator trigger
///   • player                – the player Transform
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class BossController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    #region Inspector Fields

    [Header("References")]
    public Transform   throwPoint;
    public Transform   groundCheck;
    public Transform   player;
    public GameObject  barrelPrefab;
    public GameObject  explosiveBarrelPrefab;
    public GameObject  rainBarrelPrefab;          // can reuse barrelPrefab
    public GameObject  aoeEffect;                 // ground-shockwave VFX
    public LayerMask   groundLayer;

    [Header("Health")]
    public float maxHealth          = 200f;
    public float phase2Threshold    = 0.5f;       // 50 % of max HP

    [Header("Throw Settings")]
    [Tooltip("How fast the barrel is launched (units/s)")]
    public float barrelThrowSpeed   = 8f;
    [Tooltip("Seconds between normal barrel throws")]
    public float throwCooldown      = 2.5f;
    [Tooltip("0-1 chance boss throws an explosive barrel in phase 2")]
    [Range(0f, 1f)]
    public float explosiveChance    = 0.35f;

    [Header("Barrel Rain (Phase 2)")]
    public int   rainBarrelCount    = 6;
    [Tooltip("Horizontal spread around the player position")]
    public float rainSpreadX        = 5f;
    [Tooltip("Height above player where rain barrels spawn")]
    public float rainSpawnHeight    = 10f;
    [Tooltip("Delay between each barrel drop – keep this generous!")]
    public float rainBarrelInterval = 0.8f;
    [Tooltip("Fall speed for rain barrels")]
    public float rainFallSpeed      = 4f;
    [Tooltip("Cooldown between full rain barrages")]
    public float rainCooldown       = 12f;

    [Header("Jump / AOE Smash")]
    public float jumpForce          = 18f;
    public float jumpCooldown       = 5f;
    [Tooltip("Radius of the AOE shockwave on landing")]
    public float aoeRadius          = 3.5f;
    public float aoeDamage          = 30f;
    public LayerMask playerLayer;

    [Header("Parry / Reflected Barrel")]
    [Tooltip("Damage the boss takes when a barrel is parried back")]
    public float parryDamage        = 40f;

    [Header("Ground Detection")]
    public float groundCheckRadius  = 0.2f;

    #endregion

    // ─────────────────────────────────────────────
    #region Private State

    Rigidbody2D  rb;
    Animator     anim;

    float        currentHealth;
    bool         isPhase2          = false;
    bool         isGrounded        = false;
    bool         isJumping         = false;
    bool         isDead            = false;

    float        throwTimer        = 0f;
    float        jumpTimer         = 0f;
    float        rainTimer         = 0f;

    // Animator parameter hashes (fast lookup)
    static readonly int HashJump     = Animator.StringToHash("Jump");
    static readonly int HashSmash    = Animator.StringToHash("Smash");
    static readonly int HashThrow    = Animator.StringToHash("Throw");
    static readonly int HashHurt     = Animator.StringToHash("Hurt");
    static readonly int HashDie      = Animator.StringToHash("Die");
    static readonly int HashGrounded = Animator.StringToHash("IsGrounded");

    #endregion

    // ─────────────────────────────────────────────
    #region Unity Lifecycle

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        CheckGrounded();
        HandleLanding();
        TickThrow();
        TickJump();
        if (isPhase2) TickRain();
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Ground & Landing Detection

    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        anim.SetBool(HashGrounded, isGrounded);

        // Landed after a jump → trigger AOE smash
        if (!wasGrounded && isGrounded && isJumping)
        {
            isJumping = false;
            StartCoroutine(PerformSmash());
        }
    }

    // Separate method so we can yield after setting the anim trigger
    void HandleLanding() { /* handled inside CheckGrounded via coroutine */ }

    #endregion

    // ─────────────────────────────────────────────
    #region Timers

    void TickThrow()
    {
        throwTimer += Time.deltaTime;
        if (throwTimer >= throwCooldown && isGrounded && !isJumping)
        {
            throwTimer = 0f;
            StartCoroutine(ThrowBarrel());
        }
    }

    void TickJump()
    {
        jumpTimer += Time.deltaTime;
        if (jumpTimer >= jumpCooldown && isGrounded && !isJumping)
        {
            jumpTimer = 0f;
            PerformJump();
        }
    }

    void TickRain()
    {
        rainTimer += Time.deltaTime;
        if (rainTimer >= rainCooldown && isGrounded && !isJumping)
        {
            rainTimer = 0f;
            StartCoroutine(BarrelRain());
        }
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Throw Barrel

    IEnumerator ThrowBarrel()
    {
        anim.SetTrigger(HashThrow);

        // Small wind-up delay so the anim can play
        yield return new WaitForSeconds(0.3f);

        // Phase 2: random chance to throw explosive
        bool throwExplosive = isPhase2 && (Random.value < explosiveChance);
        GameObject prefab   = throwExplosive ? explosiveBarrelPrefab : barrelPrefab;

        if (prefab == null)
        {
            Debug.LogWarning($"[Boss] Barrel prefab not assigned ({(throwExplosive ? "explosive" : "normal")})");
            yield break;
        }

        GameObject barrel = Instantiate(prefab, throwPoint.position, Quaternion.identity);

        // Direction toward player with slight upward arc
        Vector2 dir = ((Vector2)(player.position - throwPoint.position)).normalized;
        dir += Vector2.up * 0.25f;
        dir.Normalize();

        // Give the barrel its velocity – works with either Rigidbody2D or a BarrelMover component
        Rigidbody2D barrelRb = barrel.GetComponent<Rigidbody2D>();
        if (barrelRb != null)
            barrelRb.linearVelocity = dir * barrelThrowSpeed;

        // Tag the barrel so BarrelController can mark it as parriable
        BarrelController bc = barrel.GetComponent<BarrelController>();
        if (bc != null)
        {
            bc.owner       = this;
            bc.isExplosive = throwExplosive;
            bc.canBeParried = !throwExplosive;   // only normal barrels are parriable
        }
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Jump + AOE Smash

    void PerformJump()
    {
        isJumping = true;
        anim.SetTrigger(HashJump);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    IEnumerator PerformSmash()
    {
        anim.SetTrigger(HashSmash);

        // Freeze briefly for dramatic effect
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        yield return new WaitForSeconds(0.15f);
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Spawn shockwave VFX
        if (aoeEffect != null)
            Instantiate(aoeEffect, groundCheck.position, Quaternion.identity);

        // Deal damage to player if in radius
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, aoeRadius, playerLayer);
        if (hit != null)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(aoeDamage);
        }

        // Optional: screen shake signal (implement via a static event or CameraShake singleton)
        CameraShake.Instance?.Shake(0.3f, 0.4f);
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Barrel Rain (Phase 2)

    IEnumerator BarrelRain()
    {
        // Announce via anim if you have a "RainAttack" trigger
        // anim.SetTrigger("RainAttack");

        for (int i = 0; i < rainBarrelCount; i++)
        {
            float randomX = player.position.x + Random.Range(-rainSpreadX, rainSpreadX);
            Vector3 spawnPos = new Vector3(randomX, player.position.y + rainSpawnHeight, 0f);

            GameObject prefab = rainBarrelPrefab != null ? rainBarrelPrefab : barrelPrefab;
            if (prefab == null) yield break;

            GameObject barrel = Instantiate(prefab, spawnPos, Quaternion.identity);

            Rigidbody2D barrelRb = barrel.GetComponent<Rigidbody2D>();
            if (barrelRb != null)
            {
                barrelRb.gravityScale = 0f;             // we control fall manually
                barrelRb.linearVelocity    = Vector2.down * rainFallSpeed;
            }

            BarrelController bc = barrel.GetComponent<BarrelController>();
            if (bc != null)
            {
                bc.owner        = this;
                bc.isExplosive  = false;
                bc.canBeParried = false;                // rain barrels can't be parried
                bc.isRainBarrel = true;
            }

            yield return new WaitForSeconds(rainBarrelInterval);   // slow, readable cadence
        }
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Damage & Parry

    /// <summary>
    /// Called by BarrelController when the player successfully parries a barrel back.
    /// </summary>
    public void TakeParryDamage()
    {
        TakeDamage(parryDamage);
        Debug.Log("[Boss] Parry hit! Ouch.");
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth  = Mathf.Clamp(currentHealth, 0f, maxHealth);

        anim.SetTrigger(HashHurt);

        // Phase transition
        if (!isPhase2 && currentHealth <= maxHealth * phase2Threshold)
        {
            EnterPhase2();
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        rainTimer = rainCooldown * 0.5f;  // trigger first rain sooner
        throwCooldown = Mathf.Max(throwCooldown - 0.5f, 1f);  // speed up throws
        Debug.Log("[Boss] Phase 2 – things are about to get messy!");
        // Play a roar animation / SFX here if desired
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetTrigger(HashDie);
        // Fire game event, unlock door, play defeat music, etc.
        Debug.Log("[Boss] Defeated!");
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Gizmos

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
#endif

    #endregion
}
