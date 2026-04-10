using System.Collections;
using UnityEngine;

/// <summary>
/// BarrelController – attach to every barrel prefab.
///
/// Responsibilities:
///   • Marks itself as parriable or not (set by BossController at spawn)
///   • Detects when the player parries it and sends it back at the boss
///   • Handles rain barrel falling (gravity override)
///   • Self-destructs after a timeout or on hitting ground/walls
///
/// The player's parry system should call TryParry() on contact
/// (via a trigger or physics event from the player's parry hitbox).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BarrelController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    #region Inspector

    [Header("General")]
    [Tooltip("How long before this barrel self-destructs")]
    public float lifetime          = 8f;
    [Tooltip("Damage dealt to the player on contact")]
    public float playerDamage      = 15f;

    [Header("Explosive (only used when isExplosive = true)")]
    public float explosionRadius   = 2.5f;
    public float explosionDamage   = 50f;
    public GameObject explosionVFX;
    public LayerMask  playerLayer;

    [Header("Parry")]
    [Tooltip("Speed the barrel travels back toward the boss after a parry")]
    public float parryReturnSpeed  = 14f;

    #endregion

    // ─────────────────────────────────────────────
    #region Runtime (set by BossController)

    [HideInInspector] public BossController owner;
    [HideInInspector] public bool isExplosive  = false;
    [HideInInspector] public bool canBeParried = true;
    [HideInInspector] public bool isRainBarrel = false;

    #endregion

    // ─────────────────────────────────────────────
    #region Private

    Rigidbody2D rb;
    bool        hasBeenParried = false;
    bool        isAlive        = true;

    #endregion

    // ─────────────────────────────────────────────
    #region Unity Lifecycle

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    // Rain barrels use a fixed downward velocity set by the boss, no extra logic needed here.

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!isAlive) return;

        // Hit the player
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerHealth ph = col.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(playerDamage);

            if (isExplosive) Explode();
            else             Destroy(gameObject);
            return;
        }

        // Parried barrel hits the boss
        if (hasBeenParried && col.gameObject.CompareTag("Boss"))
        {
            if (owner != null) owner.TakeParryDamage();
            if (isExplosive)   Explode();
            else               Destroy(gameObject);
            return;
        }

        // Hit ground or wall – explode if explosive, otherwise just roll (let physics handle it)
        if (col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Wall"))
        {
            if (isExplosive) { Explode(); return; }
            // Normal barrels just bounce/roll – no destruction needed here unless you want it
        }
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Parry

    /// <summary>
    /// Call this from the player's parry hitbox when it overlaps with this barrel.
    /// Returns true if the parry was accepted.
    /// </summary>
    public bool TryParry()
    {
        if (!canBeParried || hasBeenParried || !isAlive) return false;

        hasBeenParried = true;
        canBeParried   = false;

        // Flip toward the boss
        if (owner != null)
        {
            Vector2 dir = ((Vector2)(owner.transform.position - transform.position)).normalized;
            rb.linearVelocity    = dir * parryReturnSpeed;
            rb.gravityScale = 0.2f;  // gentle arc back
        }

        // Visual / audio feedback – fire an event or call a VFX manager here
        Debug.Log("[Barrel] Parried! Flying back at the boss.");
        return true;
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Explosion

    void Explode()
    {
        if (!isAlive) return;
        isAlive = false;

        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        // AOE damage
        Collider2D hit = Physics2D.OverlapCircle(transform.position, explosionRadius, playerLayer);
        if (hit != null)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(explosionDamage);
        }

        CameraShake.Instance?.Shake(0.2f, 0.3f);
        Destroy(gameObject);
    }

    #endregion

    // ─────────────────────────────────────────────
    #region Gizmos

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!isExplosive) return;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif

    #endregion
}
